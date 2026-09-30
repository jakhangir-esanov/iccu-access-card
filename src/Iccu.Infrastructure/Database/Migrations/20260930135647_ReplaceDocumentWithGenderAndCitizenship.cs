using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iccu.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDocumentWithGenderAndCitizenship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_readers_document_type_document_number",
                schema: "iccu",
                table: "readers");

            migrationBuilder.DropColumn(
                name: "document_number",
                schema: "iccu",
                table: "registration_requests");

            migrationBuilder.DropColumn(
                name: "document_type",
                schema: "iccu",
                table: "registration_requests");

            migrationBuilder.DropColumn(
                name: "document_number",
                schema: "iccu",
                table: "readers");

            migrationBuilder.DropColumn(
                name: "document_type",
                schema: "iccu",
                table: "readers");

            migrationBuilder.AddColumn<int>(
                name: "citizenship",
                schema: "iccu",
                table: "registration_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender",
                schema: "iccu",
                table: "registration_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "citizenship",
                schema: "iccu",
                table: "readers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "gender",
                schema: "iccu",
                table: "readers",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "citizenship",
                schema: "iccu",
                table: "registration_requests");

            migrationBuilder.DropColumn(
                name: "gender",
                schema: "iccu",
                table: "registration_requests");

            migrationBuilder.DropColumn(
                name: "citizenship",
                schema: "iccu",
                table: "readers");

            migrationBuilder.DropColumn(
                name: "gender",
                schema: "iccu",
                table: "readers");

            migrationBuilder.AddColumn<string>(
                name: "document_number",
                schema: "iccu",
                table: "registration_requests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "document_type",
                schema: "iccu",
                table: "registration_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "document_number",
                schema: "iccu",
                table: "readers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "document_type",
                schema: "iccu",
                table: "readers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_readers_document_type_document_number",
                schema: "iccu",
                table: "readers",
                columns: new[] { "document_type", "document_number" },
                unique: true,
                filter: "deleted_at IS NULL");
        }
    }
}
