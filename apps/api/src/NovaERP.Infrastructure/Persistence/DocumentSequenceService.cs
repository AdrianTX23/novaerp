using Microsoft.EntityFrameworkCore;
using NovaERP.Application.Common.Interfaces;

namespace NovaERP.Infrastructure.Persistence;

public sealed class DocumentSequenceService(NovaErpDbContext db) : IDocumentSequenceService
{
    /// <summary>
    /// INSERT ... ON CONFLICT DO UPDATE es una sola sentencia atómica en
    /// Postgres: el motor toma el lock de fila necesario internamente, así que
    /// dos requests concurrentes del mismo tenant+tipo nunca leen el mismo
    /// valor, sin necesitar un advisory lock ni una transacción explícita
    /// desde este lado. Reemplaza el `CountAsync + 1` anterior, que sí tenía
    /// una ventana de carrera real bajo concurrencia.
    /// </summary>
    public async Task<int> NextAsync(Guid tenantId, string documentType, CancellationToken ct)
    {
        var rows = await db.Database.SqlQuery<int>($"""
            INSERT INTO document_sequences (tenant_id, document_type, next_value)
            VALUES ({tenantId}, {documentType}, 2)
            ON CONFLICT (tenant_id, document_type)
            DO UPDATE SET next_value = document_sequences.next_value + 1
            RETURNING next_value - 1
            """).ToListAsync(ct);

        return rows[0];
    }
}
