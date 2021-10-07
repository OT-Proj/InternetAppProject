using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InternetAppProject.Migrations
{
    public partial class mig1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriveType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Max_Capacity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    Last_change = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriveType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Current_usage = table.Column<int>(type: "int", nullable: false),
                    TypeIdId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "Image",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UploadTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    DIdId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Image", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Image_Drive_DIdId",
                        column: x => x.DIdId,
                        principalTable: "Drive",
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
                    Type = table.Column<int>(type: "int", nullable: false),
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
                name: "ImageTag",
                columns: table => new
                {
                    ImagesId = table.Column<int>(type: "int", nullable: false),
                    TagsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageTag", x => new { x.ImagesId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ImageTag_Image_ImagesId",
                        column: x => x.ImagesId,
                        principalTable: "Image",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageTag_Tag_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_Drive_TypeIdId",
                table: "Drive",
                column: "TypeIdId");

            migrationBuilder.CreateIndex(
                name: "IX_Image_DIdId",
                table: "Image",
                column: "DIdId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageTag_TagsId",
                table: "ImageTag",
                column: "TagsId");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageTag");

            migrationBuilder.DropTable(
                name: "PurchaseEvent");

            migrationBuilder.DropTable(
                name: "Image");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Drive");

            migrationBuilder.DropTable(
                name: "DriveType");
        }
    }
}
