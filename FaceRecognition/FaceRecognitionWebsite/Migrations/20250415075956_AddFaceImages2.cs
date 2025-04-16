using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaceRecognitionWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddFaceImages2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "People");

            migrationBuilder.CreateTable(
                name: "FaceImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ImagePath = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    PersonId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaceImages_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaceImages_PersonId",
                table: "FaceImages",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaceImages");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "People",
                type: "NVARCHAR2(2000)",
                nullable: false,
                defaultValue: "");
        }
    }
}
