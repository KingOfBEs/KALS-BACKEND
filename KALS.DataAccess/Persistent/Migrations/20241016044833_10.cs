using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KALS.DataAccess.Persistent.Migrations
{
    /// <inheritdoc />
    public partial class _10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("18ef1a50-9177-4efb-89bd-a767cfcc8533"));

            migrationBuilder.CreateTable(
                name: "SupportMessageImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupportMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportMessageImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportMessageImage_SupportMessage_SupportMessageId",
                        column: x => x.SupportMessageId,
                        principalTable: "SupportMessage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "FullName", "Password", "PhoneNumber", "Role", "Username" },
                values: new object[] { new Guid("77df5578-4c5f-4e99-9578-6fa0265773b2"), "Admin", "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", "0123456789", "Manager", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_SupportMessageImage_SupportMessageId",
                table: "SupportMessageImage",
                column: "SupportMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportMessageImage");

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("77df5578-4c5f-4e99-9578-6fa0265773b2"));

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "FullName", "Password", "PhoneNumber", "Role", "Username" },
                values: new object[] { new Guid("18ef1a50-9177-4efb-89bd-a767cfcc8533"), "Admin", "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", "0123456789", "Manager", "admin" });
        }
    }
}
