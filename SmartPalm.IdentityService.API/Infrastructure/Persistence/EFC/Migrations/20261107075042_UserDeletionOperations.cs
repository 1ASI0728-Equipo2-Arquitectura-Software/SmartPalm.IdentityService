using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPalm.IdentityService.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class UserDeletionOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_deletion_operations",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    expected_services = table.Column<int>(type: "integer", nullable: false),
                    completed_services = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_deletion_operations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_deletion_service_results",
                schema: "identity",
                columns: table => new
                {
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    deleted_records = table.Column<int>(type: "integer", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_deletion_service_results", x => new { x.operation_id, x.service_name });
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_deletion_operations_user_id_status",
                schema: "identity",
                table: "user_deletion_operations",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_deletion_operations",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_deletion_service_results",
                schema: "identity");
        }
    }
}
