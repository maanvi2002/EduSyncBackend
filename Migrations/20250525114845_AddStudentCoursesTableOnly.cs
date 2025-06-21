using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSyncProject.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCoursesTableOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentCourses",
                columns: table => new
                {
                    EnrolledCoursesCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentsUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCourses", x => new { x.EnrolledCoursesCourseId, x.StudentsUserId });
                    table.ForeignKey(
                        name: "FK_StudentCourses_Course_EnrolledCoursesCourseId",
                        column: x => x.EnrolledCoursesCourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentCourses_User_StudentsUserId",
                        column: x => x.StudentsUserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourses_StudentsUserId",
                table: "StudentCourses",
                column: "StudentsUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentCourses");
        }
    }
}
