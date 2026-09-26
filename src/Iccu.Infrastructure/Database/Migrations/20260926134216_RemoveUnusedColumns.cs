using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iccu.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_registration_requests_document_type_document_number",
                schema: "iccu",
                table: "registration_requests");

            migrationBuilder.DropColumn(
                name: "password_changed_at",
                schema: "iccu",
                table: "users");

            migrationBuilder.DropColumn(
                name: "extension",
                schema: "iccu",
                table: "stored_files");

            migrationBuilder.DropColumn(
                name: "original_name",
                schema: "iccu",
                table: "stored_files");

            migrationBuilder.DropColumn(
                name: "replaced_by_token_id",
                schema: "iccu",
                table: "refresh_tokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "password_changed_at",
                schema: "iccu",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "extension",
                schema: "iccu",
                table: "stored_files",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "original_name",
                schema: "iccu",
                table: "stored_files",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "replaced_by_token_id",
                schema: "iccu",
                table: "refresh_tokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_registration_requests_document_type_document_number",
                schema: "iccu",
                table: "registration_requests",
                columns: new[] { "document_type", "document_number" });
        }
    }
}
