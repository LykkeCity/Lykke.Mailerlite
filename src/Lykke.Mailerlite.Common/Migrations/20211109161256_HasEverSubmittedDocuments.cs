using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Mailerlite.Common.Migrations
{
    public partial class HasEverSubmittedDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasEverSubmittedDocuments",
                schema: "lykke-mailerlite",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasEverSubmittedDocuments",
                schema: "lykke-mailerlite",
                table: "customers");
        }
    }
}
