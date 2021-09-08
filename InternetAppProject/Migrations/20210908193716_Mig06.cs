using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InternetAppProject.Migrations
{
    public partial class Mig06 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriveId",
                table: "Image",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Drive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Current_usage = table.Column<int>(type: "int", nullable: false),
                    TypeIdId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drive", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drive_DriveType_TypeIdId",
                        column: x => x.TypeIdId,
                        principalTable: "DriveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Zip = table.Column<int>(type: "int", nullable: false),
                    Credit_card = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Visual_mode = table.Column<bool>(type: "bit", nullable: false),
                    Create_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Drive_DId",
                        column: x => x.DId,
                        principalTable: "Drive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserIDId = table.Column<int>(type: "int", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseEvent_User_UserIDId",
                        column: x => x.UserIDId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Image_DriveId",
                table: "Image",
                column: "DriveId");

            migrationBuilder.CreateIndex(
                name: "IX_Drive_TypeIdId",
                table: "Drive",
                column: "TypeIdId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseEvent_UserIDId",
                table: "PurchaseEvent",
                column: "UserIDId");

            migrationBuilder.CreateIndex(
                name: "IX_User_DId",
                table: "User",
                column: "DId",
                unique: true,
                filter: "[DId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Image_Drive_DriveId",
                table: "Image",
                column: "DriveId",
                principalTable: "Drive",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Image_Drive_DriveId",
                table: "Image");

            migrationBuilder.DropTable(
                name: "PurchaseEvent");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Drive");

            migrationBuilder.DropIndex(
                name: "IX_Image_DriveId",
                table: "Image");

            migrationBuilder.DropColumn(
                name: "DriveId",
                table: "Image");
        }
    }
}
