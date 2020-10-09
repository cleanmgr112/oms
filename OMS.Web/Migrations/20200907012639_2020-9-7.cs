using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OMS.Web.Migrations
{
    public partial class _202097 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateCode",
                table: "UserMessage");

            migrationBuilder.DropColumn(
                name: "TemplateCode",
                table: "RemindTitle");

            migrationBuilder.AddColumn<string>(
                name: "ProductId",
                table: "RemindTemplate",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RemindProduct",
                columns: table => new
                {
                    CreateTime = table.Column<DateTime>(nullable: false),
                    EditorTime = table.Column<DateTime>(nullable: true),
                    Edtior = table.Column<string>(maxLength: 10, nullable: true),
                    Isdelete = table.Column<bool>(nullable: false),
                    Id = table.Column<string>(nullable: false),
                    ProductCode = table.Column<string>(maxLength: 10, nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    En = table.Column<string>(maxLength: 100, nullable: false),
                    Stock = table.Column<int>(nullable: false),
                    Price = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemindProduct", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RemindTemplate_ProductId",
                table: "RemindTemplate",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_RemindTemplate_RemindProduct_ProductId",
                table: "RemindTemplate",
                column: "ProductId",
                principalTable: "RemindProduct",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RemindTemplate_RemindProduct_ProductId",
                table: "RemindTemplate");

            migrationBuilder.DropTable(
                name: "RemindProduct");

            migrationBuilder.DropIndex(
                name: "IX_RemindTemplate_ProductId",
                table: "RemindTemplate");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "RemindTemplate");

            migrationBuilder.AddColumn<string>(
                name: "TemplateCode",
                table: "UserMessage",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateCode",
                table: "RemindTitle",
                nullable: true);
        }
    }
}
