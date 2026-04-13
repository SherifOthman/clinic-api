using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoLocationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeoCountries",
                columns: table => new
                {
                    GeonameId   = table.Column<int>(nullable: false),
                    CountryCode = table.Column<string>(maxLength: 2,   nullable: false),
                    NameEn      = table.Column<string>(maxLength: 100, nullable: false),
                    NameAr      = table.Column<string>(maxLength: 100, nullable: false),
                },
                constraints: table => table.PrimaryKey("PK_GeoCountries", x => x.GeonameId));

            migrationBuilder.CreateIndex(
                name: "IX_GeoCountries_CountryCode",
                table: "GeoCountries",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateTable(
                name: "GeoStates",
                columns: table => new
                {
                    GeonameId        = table.Column<int>(nullable: false),
                    CountryGeonameId = table.Column<int>(nullable: false),
                    NameEn           = table.Column<string>(maxLength: 150, nullable: false),
                    NameAr           = table.Column<string>(maxLength: 150, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoStates", x => x.GeonameId);
                    table.ForeignKey(
                        name: "FK_GeoStates_GeoCountries_CountryGeonameId",
                        column: x => x.CountryGeonameId,
                        principalTable: "GeoCountries",
                        principalColumn: "GeonameId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeoStates_CountryGeonameId",
                table: "GeoStates",
                column: "CountryGeonameId");

            migrationBuilder.CreateTable(
                name: "GeoCities",
                columns: table => new
                {
                    GeonameId      = table.Column<int>(nullable: false),
                    StateGeonameId = table.Column<int>(nullable: false),
                    NameEn         = table.Column<string>(maxLength: 150, nullable: false),
                    NameAr         = table.Column<string>(maxLength: 150, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoCities", x => x.GeonameId);
                    table.ForeignKey(
                        name: "FK_GeoCities_GeoStates_StateGeonameId",
                        column: x => x.StateGeonameId,
                        principalTable: "GeoStates",
                        principalColumn: "GeonameId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeoCities_StateGeonameId",
                table: "GeoCities",
                column: "StateGeonameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GeoCities");
            migrationBuilder.DropTable(name: "GeoStates");
            migrationBuilder.DropTable(name: "GeoCountries");
        }
    }
}
