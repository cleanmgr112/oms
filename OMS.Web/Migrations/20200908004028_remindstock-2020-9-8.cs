using Microsoft.EntityFrameworkCore.Migrations;

namespace OMS.Web.Migrations
{
    public partial class remindstock202098 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RemindStock",
                table: "RemindTemplate",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemindStock",
                table: "RemindTemplate");
        }
    }
}
