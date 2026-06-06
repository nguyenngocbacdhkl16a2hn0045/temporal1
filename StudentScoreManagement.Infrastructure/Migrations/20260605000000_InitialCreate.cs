using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StudentScoreManagement.Infrastructure.Data;

#nullable disable

namespace StudentScoreManagement.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260605000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                StudentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                ClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Students", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Subjects",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SubjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                SubjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Credit = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Subjects", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AppUsers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                StudentId = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppUsers", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppUsers_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Scores",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                StudentId = table.Column<int>(type: "int", nullable: false),
                SubjectId = table.Column<int>(type: "int", nullable: false),
                ScoreValue = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                Semester = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                SchoolYear = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Scores", x => x.Id);
                table.ForeignKey(
                    name: "FK_Scores_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Scores_Subjects_SubjectId",
                    column: x => x.SubjectId,
                    principalTable: "Subjects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_StudentId",
            table: "AppUsers",
            column: "StudentId");

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_Username",
            table: "AppUsers",
            column: "Username",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Scores_StudentId_SubjectId_Semester_SchoolYear",
            table: "Scores",
            columns: new[] { "StudentId", "SubjectId", "Semester", "SchoolYear" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Scores_SubjectId",
            table: "Scores",
            column: "SubjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Students_StudentCode",
            table: "Students",
            column: "StudentCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Subjects_SubjectCode",
            table: "Subjects",
            column: "SubjectCode",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AppUsers");
        migrationBuilder.DropTable(name: "Scores");
        migrationBuilder.DropTable(name: "Students");
        migrationBuilder.DropTable(name: "Subjects");
    }
}
