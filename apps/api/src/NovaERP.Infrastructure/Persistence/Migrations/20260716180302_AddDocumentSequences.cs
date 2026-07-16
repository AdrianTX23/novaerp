using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovaERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sin DbSet/entidad EF a propósito: se accede solo vía SQL crudo
            // (DocumentSequenceService) con un INSERT ... ON CONFLICT atómico,
            // así que no necesita ChangeTracker ni el filtro global de tenant.
            migrationBuilder.CreateTable(
                name: "document_sequences",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    next_value = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_sequences", x => new { x.tenant_id, x.document_type });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "document_sequences");
        }
    }
}
