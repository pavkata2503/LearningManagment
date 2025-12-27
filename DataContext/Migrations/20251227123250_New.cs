using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataContext.Migrations
{
    /// <inheritdoc />
    public partial class New : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserTestResults_StudyMaterialId",
                table: "UserTestResults",
                column: "StudyMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTestResults_StudyMaterials_StudyMaterialId",
                table: "UserTestResults",
                column: "StudyMaterialId",
                principalTable: "StudyMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTestResults_StudyMaterials_StudyMaterialId",
                table: "UserTestResults");

            migrationBuilder.DropIndex(
                name: "IX_UserTestResults_StudyMaterialId",
                table: "UserTestResults");
        }
    }
}
