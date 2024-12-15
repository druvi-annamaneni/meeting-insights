using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranscriptionsProcessor.Migrations
{
    /// <inheritdoc />
    public partial class ActionItemDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Deadline",
                table: "ActionItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "ActionItems");
        }
    }
}
