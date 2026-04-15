using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPatientGeoFKCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patient_GeoCities_CityGeonameId",
                table: "Patient");

            migrationBuilder.DropForeignKey(
                name: "FK_Patient_GeoCountries_CountryGeonameId",
                table: "Patient");

            migrationBuilder.DropForeignKey(
                name: "FK_Patient_GeoStates_StateGeonameId",
                table: "Patient");

            migrationBuilder.AddForeignKey(
                name: "FK_Patient_GeoCities_CityGeonameId",
                table: "Patient",
                column: "CityGeonameId",
                principalTable: "GeoCities",
                principalColumn: "GeonameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patient_GeoCountries_CountryGeonameId",
                table: "Patient",
                column: "CountryGeonameId",
                principalTable: "GeoCountries",
                principalColumn: "GeonameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patient_GeoStates_StateGeonameId",
                table: "Patient",
                column: "StateGeonameId",
                principalTable: "GeoStates",
                principalColumn: "GeonameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patient_GeoCities_CityGeonameId",
                table: "Patient");

            migrationBuilder.DropForeignKey(
                name: "FK_Patient_GeoCountries_CountryGeonameId",
                table: "Patient");

            migrationBuilder.DropForeignKey(
                name: "FK_Patient_GeoStates_StateGeonameId",
                table: "Patient");

            migrationBuilder.AddForeignKey(
                name: "FK_Patient_GeoCities_CityGeonameId",
                table: "Patient",
                column: "CityGeonameId",
                principalTable: "GeoCities",
                principalColumn: "GeonameId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Patient_GeoCountries_CountryGeonameId",
                table: "Patient",
                column: "CountryGeonameId",
                principalTable: "GeoCountries",
                principalColumn: "GeonameId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Patient_GeoStates_StateGeonameId",
                table: "Patient",
                column: "StateGeonameId",
                principalTable: "GeoStates",
                principalColumn: "GeonameId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
