using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SubscriptionPayment");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SubscriptionPayment");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SubscriptionPayment");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SubscriptionPayment");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Specialization");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Specialization");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Specialization");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Specialization");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ClinicUsageMetrics");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClinicUsageMetrics");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ClinicUsageMetrics");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ClinicUsageMetrics",
                newName: "LastAggregatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastAggregatedAt",
                table: "ClinicUsageMetrics",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "SubscriptionPayment",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "SubscriptionPayment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "SubscriptionPayment",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "SubscriptionPayment",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Specialization",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Specialization",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Specialization",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Specialization",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ClinicUsageMetrics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "ClinicUsageMetrics",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "ClinicUsageMetrics",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
