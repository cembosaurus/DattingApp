using Microsoft.EntityFrameworkCore.Migrations;

namespace DatingApp.API.Migrations
{
    public partial class ChangePropertyNameInMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateSent",
                table: "Messages",
                newName: "MessageSent");

            migrationBuilder.RenameColumn(
                name: "DateRead",
                table: "Messages",
                newName: "MessageRead");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageSent",
                table: "Messages",
                newName: "DateSent");

            migrationBuilder.RenameColumn(
                name: "MessageRead",
                table: "Messages",
                newName: "DateRead");
        }
    }
}
