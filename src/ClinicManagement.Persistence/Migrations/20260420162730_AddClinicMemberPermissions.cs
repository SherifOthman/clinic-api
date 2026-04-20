using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicMemberPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicMemberPermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicMemberPermission", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicMemberPermission_ClinicMember_ClinicMemberId",
                        column: x => x.ClinicMemberId,
                        principalTable: "ClinicMember",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicMemberPermission_ClinicMemberId_Permission",
                table: "ClinicMemberPermission",
                columns: new[] { "ClinicMemberId", "Permission" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicMemberPermission");
        }
    }
}
