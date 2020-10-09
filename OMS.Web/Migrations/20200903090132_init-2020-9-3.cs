using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OMS.Web.Migrations
{
    public partial class init202093 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RemindTemplate",
                columns: table => new
                {
                    CreateTime = table.Column<DateTime>(nullable: false),
                    EditorTime = table.Column<DateTime>(nullable: true),
                    Edtior = table.Column<string>(maxLength: 10, nullable: true),
                    Isdelete = table.Column<bool>(nullable: false),
                    TemplateId = table.Column<string>(maxLength: 16, nullable: false),
                    TemplateCode = table.Column<string>(maxLength: 10, nullable: false),
                    TemplateTitle = table.Column<string>(maxLength: 80, nullable: true),
                    User = table.Column<string>(maxLength: 100, nullable: true),
                    SaleProductId = table.Column<int>(nullable: false),
                    Statu = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemindTemplate", x => x.TemplateId);
                });

            migrationBuilder.CreateTable(
                name: "RemindTitle",
                columns: table => new
                {
                    CreateTime = table.Column<DateTime>(nullable: false),
                    EditorTime = table.Column<DateTime>(nullable: true),
                    Edtior = table.Column<string>(maxLength: 10, nullable: true),
                    Isdelete = table.Column<bool>(nullable: false),
                    TitleId = table.Column<string>(nullable: false),
                    RemindTitle = table.Column<string>(maxLength: 50, nullable: true),
                    IsRead = table.Column<bool>(nullable: false),
                    TemplateCode = table.Column<string>(nullable: true),
                    RemindTemplateModelTemplateId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemindTitle", x => x.TitleId);
                    table.ForeignKey(
                        name: "FK_RemindTitle_RemindTemplate_RemindTemplateModelTemplateId",
                        column: x => x.RemindTemplateModelTemplateId,
                        principalTable: "RemindTemplate",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserMessage",
                columns: table => new
                {
                    CreateTime = table.Column<DateTime>(nullable: false),
                    EditorTime = table.Column<DateTime>(nullable: true),
                    Edtior = table.Column<string>(maxLength: 10, nullable: true),
                    Isdelete = table.Column<bool>(nullable: false),
                    MessageId = table.Column<string>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    TemplateCode = table.Column<string>(nullable: true),
                    RemindTemplateModelTemplateId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMessage", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_UserMessage_RemindTemplate_RemindTemplateModelTemplateId",
                        column: x => x.RemindTemplateModelTemplateId,
                        principalTable: "RemindTemplate",
                        principalColumn: "TemplateId",
                        onDelete: ReferentialAction.Restrict);
                });


        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropTable(
                name: "RemindTitle");


            migrationBuilder.DropTable(
                name: "UserMessage");

          

            migrationBuilder.DropTable(
                name: "RemindTemplate");

         
        }
    }
}
