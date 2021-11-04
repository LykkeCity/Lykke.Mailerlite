using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Mailerlite.Common.Migrations
{
    public partial class DepositedFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deposited",
                schema: "lykke-mailerlite",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deposited",
                schema: "lykke-mailerlite",
                table: "customers");
        }
    }
}
