using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace final_project_back_end_akostryba.Migrations
{
    /// <inheritdoc />
    public partial class FixedName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Useres",
                table: "Useres");

            migrationBuilder.RenameTable(
                name: "Useres",
                newName: "Users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Useres");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Useres",
                table: "Useres",
                column: "userId");
        }
    }
}
