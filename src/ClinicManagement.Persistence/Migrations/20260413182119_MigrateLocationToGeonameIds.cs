using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrateLocationToGeonameIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patient_CityNameEn_CityNameAr",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_Patient_CountryNameEn_CountryNameAr",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_Patient_StateNameEn_StateNameAr",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "CityNameAr",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "CityNameEn",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "CountryNameAr",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "CountryNameEn",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "StateNameAr",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "StateNameEn",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "CityNameAr",
                table: "ClinicBranch");

            migrationBuilder.DropColumn(
                name: "CityNameEn",
                table: "ClinicBranch");

            migrationBuilder.DropColumn(
                name: "StateNameAr",
                table: "ClinicBranch");

            migrationBuilder.DropColumn(
                name: "StateNameEn",
                table: "ClinicBranch");

            migrationBuilder.AddColumn<int>(
                name: "CityGeonameId",
                table: "Patient",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryGeonameId",
                table: "Patient",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateGeonameId",
                table: "Patient",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityGeonameId",
                table: "ClinicBranch",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateGeonameId",
                table: "ClinicBranch",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patient_CityGeonameId",
                table: "Patient",
                column: "CityGeonameId");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_CountryGeonameId",
                table: "Patient",
                column: "CountryGeonameId");

            migrationBuilder.CreateIndex(
                name: "IX_Patient_StateGeonameId",
                table: "Patient",
                column: "StateGeonameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patient_CityGeonameId",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_Patient_CountryGeonameId",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_Patient_StateGeonameId",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "CityGeonameId",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "CountryGeonameId",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "StateGeonameId",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "CityGeonameId",
                table: "ClinicBranch");

            migrationBuilder.DropColumn(
                name: "StateGeonameId",
                table: "ClinicBranch");

            migrationBuilder.AddColumn<string>(
                name: "CityNameAr",
                table: "Patient",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityNameEn",
                table: "Patient",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryNameAr",
                table: "Patient",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryNameEn",
                table: "Patient",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateNameAr",
                table: "Patient",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateNameEn",
                table: "Patient",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityNameAr",
                table: "ClinicBranch",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityNameEn",
                table: "ClinicBranch",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateNameAr",
                table: "ClinicBranch",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateNameEn",
                table: "ClinicBranch",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patient_CityNameEn_CityNameAr",
                table: "Patient",
                columns: new[] { "CityNameEn", "CityNameAr" });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_CountryNameEn_CountryNameAr",
                table: "Patient",
                columns: new[] { "CountryNameEn", "CountryNameAr" });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_StateNameEn_StateNameAr",
                table: "Patient",
                columns: new[] { "StateNameEn", "StateNameAr" });
        }
    }
}
