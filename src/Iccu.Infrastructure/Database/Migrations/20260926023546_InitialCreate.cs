using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iccu.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "iccu");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateSequence<int>(
                name: "card_number_seq",
                schema: "iccu",
                minValue: 1L,
                maxValue: 9999999L);

            migrationBuilder.CreateSequence<int>(
                name: "registration_code_seq",
                schema: "iccu",
                minValue: 1L,
                maxValue: 9999L,
                cyclic: true);

            migrationBuilder.CreateTable(
                name: "readers",
                schema: "iccu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_number = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('iccu.card_number_seq')"),
                    category = table.Column<int>(type: "integer", nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    document_type = table.Column<int>(type: "integer", nullable: false),
                    document_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    photo_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false),
                    issued_on = table.Column<DateOnly>(type: "date", nullable: false),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: false),
                    print_count = table.Column<int>(type: "integer", nullable: false),
                    last_printed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    search_text = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(last_name || ' ' || first_name || ' ' || coalesce(middle_name, '')), '‘’ʻʼ`´', '''''''''''''')", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_readers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "registration_requests",
                schema: "iccu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('iccu.registration_code_seq')"),
                    status = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    phone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    document_type = table.Column<int>(type: "integer", nullable: false),
                    document_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    photo_file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reader_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registration_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stored_files",
                schema: "iccu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    storage_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stored_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "iccu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    password_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "iccu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "iccu",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_readers_card_number",
                schema: "iccu",
                table: "readers",
                column: "card_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_readers_created_at",
                schema: "iccu",
                table: "readers",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_readers_document_type_document_number",
                schema: "iccu",
                table: "readers",
                columns: new[] { "document_type", "document_number" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_readers_expires_on",
                schema: "iccu",
                table: "readers",
                column: "expires_on");

            migrationBuilder.CreateIndex(
                name: "ix_readers_phone",
                schema: "iccu",
                table: "readers",
                column: "phone")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_readers_photo_file_id",
                schema: "iccu",
                table: "readers",
                column: "photo_file_id");

            migrationBuilder.CreateIndex(
                name: "ix_readers_search_text",
                schema: "iccu",
                table: "readers",
                column: "search_text")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                schema: "iccu",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_expires_at",
                schema: "iccu",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_registration_requests_code",
                schema: "iccu",
                table: "registration_requests",
                column: "code",
                unique: true,
                filter: "status = 0");

            migrationBuilder.CreateIndex(
                name: "ix_registration_requests_document_type_document_number",
                schema: "iccu",
                table: "registration_requests",
                columns: new[] { "document_type", "document_number" });

            migrationBuilder.CreateIndex(
                name: "ix_registration_requests_photo_file_id",
                schema: "iccu",
                table: "registration_requests",
                column: "photo_file_id");

            migrationBuilder.CreateIndex(
                name: "ix_registration_requests_status_submitted_at",
                schema: "iccu",
                table: "registration_requests",
                columns: new[] { "status", "submitted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_stored_files_created_at",
                schema: "iccu",
                table: "stored_files",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                schema: "iccu",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "readers",
                schema: "iccu");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "iccu");

            migrationBuilder.DropTable(
                name: "registration_requests",
                schema: "iccu");

            migrationBuilder.DropTable(
                name: "stored_files",
                schema: "iccu");

            migrationBuilder.DropTable(
                name: "users",
                schema: "iccu");

            migrationBuilder.DropSequence(
                name: "card_number_seq",
                schema: "iccu");

            migrationBuilder.DropSequence(
                name: "registration_code_seq",
                schema: "iccu");
        }
    }
}
