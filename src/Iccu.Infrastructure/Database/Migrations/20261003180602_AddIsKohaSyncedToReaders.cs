using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Iccu.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIsKohaSyncedToReaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_koha_synced",
                schema: "iccu",
                table: "readers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_koha_synced",
                schema: "iccu",
                table: "readers");
        }
    }
}
