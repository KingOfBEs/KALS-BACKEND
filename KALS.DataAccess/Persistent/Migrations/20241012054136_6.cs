using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KALS.DataAccess.Persistent.Migrations
{
    /// <inheritdoc />
    public partial class _6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupportRequest_LabId",
                table: "SupportRequest");

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("eda40278-0f80-4e24-a56f-0d50679f600e"));

            migrationBuilder.DropColumn(
                name: "NumberOfRequest",
                table: "SupportRequest");

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "SupportRequest",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRequest",
                table: "LabMember",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "FullName", "Password", "PhoneNumber", "Role", "Username" },
                values: new object[] { new Guid("7aab6022-8250-44e9-8a2d-9d5d81e9f538"), "Admin", "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", "0123456789", "Manager", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_LabId_MemberId",
                table: "SupportRequest",
                columns: new[] { "LabId", "MemberId" });

            migrationBuilder.AddForeignKey(
                name: "FK_SupportRequest_LabMember_LabId_MemberId",
                table: "SupportRequest",
                columns: new[] { "LabId", "MemberId" },
                principalTable: "LabMember",
                principalColumns: new[] { "LabId", "MemberId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportRequest_LabMember_LabId_MemberId",
                table: "SupportRequest");

            migrationBuilder.DropIndex(
                name: "IX_SupportRequest_LabId_MemberId",
                table: "SupportRequest");

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("7aab6022-8250-44e9-8a2d-9d5d81e9f538"));

            migrationBuilder.DropColumn(
                name: "NumberOfRequest",
                table: "LabMember");

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "SupportRequest",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRequest",
                table: "SupportRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "FullName", "Password", "PhoneNumber", "Role", "Username" },
                values: new object[] { new Guid("eda40278-0f80-4e24-a56f-0d50679f600e"), "Admin", "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", "0123456789", "Manager", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportRequest_LabId",
                table: "SupportRequest",
                column: "LabId");
        }
    }
}
