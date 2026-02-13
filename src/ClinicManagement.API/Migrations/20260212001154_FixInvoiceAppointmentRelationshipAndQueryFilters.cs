using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.API.Migrations
{
    /// <inheritdoc />
    public partial class FixInvoiceAppointmentRelationshipAndQueryFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Invoices_InvoiceId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Appointments_AppointmentId1",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_AppointmentId1",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_InvoiceId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AppointmentId1",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AppointmentId",
                table: "Invoices",
                column: "AppointmentId",
                unique: true,
                filter: "[AppointmentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Appointments_AppointmentId",
                table: "Invoices",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Appointments_AppointmentId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_AppointmentId",
                table: "Invoices");

            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId1",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AppointmentId1",
                table: "Invoices",
                column: "AppointmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_InvoiceId",
                table: "Appointments",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Invoices_InvoiceId",
                table: "Appointments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Appointments_AppointmentId1",
                table: "Invoices",
                column: "AppointmentId1",
                principalTable: "Appointments",
                principalColumn: "Id");
        }
    }
}
