using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGeoCityUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeoCities_StateGeonameId_NameEn",
                table: "GeoCities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GeoCities_StateGeonameId_NameEn",
                table: "GeoCities",
                columns: new[] { "StateGeonameId", "NameEn" },
                unique: true);
        }
    }
}
