using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KALS.DataAccess.Persistent.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("e1f55677-a46e-4b23-be05-aad9b2823184"));

            migrationBuilder.AddColumn<string>(
                name: "WarrantyCode",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyExpired",
                table: "OrderItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WarrantyRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestContent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ResponseContent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyRequest_OrderItem_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyRequestImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WarrantyRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyRequestImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantyRequestImage_WarrantyRequest_WarrantyRequestId",
                        column: x => x.WarrantyRequestId,
                        principalTable: "WarrantyRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "FullName", "Password", "PhoneNumber", "Role", "Username" },
                values: new object[] { new Guid("3ac85209-cac7-4a7d-a73a-3937fc29ee7b"), "Admin", "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", "0123456789", "Manager", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequest_OrderItemId",
                table: "WarrantyRequest",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyRequestImage_WarrantyRequestId",
                table: "WarrantyRequestImage",
                column: "WarrantyRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarrantyRequestImage");

            migrationBuilder.DropTable(
                name: "WarrantyRequest");

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("3ac85209-cac7-4a7d-a73a-3937fc29ee7b"));

            migrationBuilder.DropColumn(
                name: "WarrantyCode",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "WarrantyExpired",
                table: "OrderItem");

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "FullName", "Password", "PhoneNumber", "Role", "Username" },
                values: new object[] { new Guid("e1f55677-a46e-4b23-be05-aad9b2823184"), "Admin", "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", "0123456789", "Manager", "admin" });
        }
    }
}
