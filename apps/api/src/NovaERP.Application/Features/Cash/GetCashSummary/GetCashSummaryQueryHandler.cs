using MediatR;
using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;
using NovaERP.Application.Features.Cash.Common;
using NovaERP.Domain.Cash;

namespace NovaERP.Application.Features.Cash.GetCashSummary;

/// <summary>
/// Resume la caja combinando dos fuentes: los pagos de facturas (ingresos) y los
/// movimientos manuales (ingresos/egresos). El saldo es ingresos − egresos.
/// </summary>
public sealed class GetCashSummaryQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetCashSummaryQuery, CashSummaryDto>
{
    public async Task<CashSummaryDto> Handle(GetCashSummaryQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var paymentIncome = await db.Payments.SumAsync(p => p.Amount, ct);
        var paymentIncomeMonth = await db.Payments
            .Where(p => p.PaidAt >= monthStart && p.PaidAt < nextMonth)
            .SumAsync(p => p.Amount, ct);

        var manualIncome = await db.CashMovements
            .Where(m => m.Type == CashMovementType.Income).SumAsync(m => m.Amount, ct);
        var manualExpense = await db.CashMovements
            .Where(m => m.Type == CashMovementType.Expense).SumAsync(m => m.Amount, ct);

        var manualIncomeMonth = await db.CashMovements
            .Where(m => m.Type == CashMovementType.Income && m.Date >= monthStart && m.Date < nextMonth)
            .SumAsync(m => m.Amount, ct);
        var manualExpenseMonth = await db.CashMovements
            .Where(m => m.Type == CashMovementType.Expense && m.Date >= monthStart && m.Date < nextMonth)
            .SumAsync(m => m.Amount, ct);

        var totalIncome = paymentIncome + manualIncome;
        var totalExpense = manualExpense;

        return new CashSummaryDto(
            Balance: totalIncome - totalExpense,
            TotalIncome: totalIncome,
            TotalExpense: totalExpense,
            IncomeThisMonth: paymentIncomeMonth + manualIncomeMonth,
            ExpenseThisMonth: manualExpenseMonth);
    }
}
