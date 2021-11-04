using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Mailerlite.Common.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "lykke-mailerlite");

            migrationBuilder.CreateSequence(
                name: "id_generator_deposit_updates",
                schema: "lykke-mailerlite",
                startValue: 500000000L);

            migrationBuilder.CreateTable(
                name: "customers",
                schema: "lykke-mailerlite",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    KycState = table.Column<string>(type: "text", nullable: true),
                    KycStateTimestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "id_generator",
                schema: "lykke-mailerlite",
                columns: table => new
                {
                    IdempotencyId = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_id_generator", x => x.IdempotencyId);
                });

            migrationBuilder.CreateTable(
                name: "outbox",
                schema: "lykke-mailerlite",
                columns: table => new
                {
                    IdempotencyId = table.Column<string>(type: "text", nullable: false),
                    Response = table.Column<string>(type: "text", nullable: true),
                    Events = table.Column<string>(type: "text", nullable: true),
                    Commands = table.Column<string>(type: "text", nullable: true),
                    IsDispatched = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox", x => x.IdempotencyId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customers",
                schema: "lykke-mailerlite");

            migrationBuilder.DropTable(
                name: "id_generator",
                schema: "lykke-mailerlite");

            migrationBuilder.DropTable(
                name: "outbox",
                schema: "lykke-mailerlite");

            migrationBuilder.DropSequence(
                name: "id_generator_deposit_updates",
                schema: "lykke-mailerlite");
        }
    }
}
