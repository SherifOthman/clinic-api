using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedSoftDeleteFromAuditableEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClinicMember_ClinicId_IsDeleted_IsActive",
                table: "ClinicMember");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SubscriptionPayment");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Specialization");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Medicine");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MedicalSupply");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MedicalService");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EmailQueue");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ClinicSubscription");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ClinicMember");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ClinicBranch");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Appointment");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicMember_ClinicId_IsActive",
                table: "ClinicMember",
                columns: new[] { "ClinicId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClinicMember_ClinicId_IsActive",
                table: "ClinicMember");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SubscriptionPayment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Specialization",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RefreshToken",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Notification",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Medicine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MedicalSupply",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MedicalService",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Invoice",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EmailQueue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ClinicSubscription",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ClinicMember",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ClinicBranch",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Appointment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicMember_ClinicId_IsDeleted_IsActive",
                table: "ClinicMember",
                columns: new[] { "ClinicId", "IsDeleted", "IsActive" });
        }
    }
}
