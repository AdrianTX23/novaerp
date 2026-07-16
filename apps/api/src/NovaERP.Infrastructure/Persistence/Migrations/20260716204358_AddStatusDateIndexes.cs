using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusDateIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_sales_orders_TenantId_Status_OrderDate",
                table: "sales_orders",
                columns: new[] { "TenantId", "Status", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_TenantId_Status_OrderDate",
                table: "purchase_orders",
                columns: new[] { "TenantId", "Status", "OrderDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sales_orders_TenantId_Status_OrderDate",
                table: "sales_orders");

            migrationBuilder.DropIndex(
                name: "IX_purchase_orders_TenantId_Status_OrderDate",
                table: "purchase_orders");
        }
    }
}
