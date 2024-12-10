using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeadHunter.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Resumes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Resumes");
        }
    }
}
