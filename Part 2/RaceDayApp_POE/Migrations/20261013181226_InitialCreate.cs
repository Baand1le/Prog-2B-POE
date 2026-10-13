using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDayApp_POE.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    roleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roleType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.roleId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    userID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    roleID = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.userID);
                    table.ForeignKey(
                        name: "FK_User_UserRole_roleID",
                        column: x => x.roleID,
                        principalTable: "UserRole",
                        principalColumn: "roleId");
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    eventID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    organiserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Distance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    eventType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.eventID);
                    table.ForeignKey(
                        name: "FK_Event_User_organiserID",
                        column: x => x.organiserID,
                        principalTable: "User",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    categoryID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    eventID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    categoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ageGroup = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.categoryID);
                    table.ForeignKey(
                        name: "FK_Category_Event_eventID",
                        column: x => x.eventID,
                        principalTable: "Event",
                        principalColumn: "eventID");
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    enrollmentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    participantID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    eventID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    categoryID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    enrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => x.enrollmentID);
                    table.ForeignKey(
                        name: "FK_Enrollment_Category_categoryID",
                        column: x => x.categoryID,
                        principalTable: "Category",
                        principalColumn: "categoryID");
                    table.ForeignKey(
                        name: "FK_Enrollment_Event_eventID",
                        column: x => x.eventID,
                        principalTable: "Event",
                        principalColumn: "eventID");
                    table.ForeignKey(
                        name: "FK_Enrollment_User_participantID",
                        column: x => x.participantID,
                        principalTable: "User",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "Result",
                columns: table => new
                {
                    resultID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    enrollmentID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    finishTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    finishPosition = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Result", x => x.resultID);
                    table.ForeignKey(
                        name: "FK_Result_Enrollment_enrollmentID",
                        column: x => x.enrollmentID,
                        principalTable: "Enrollment",
                        principalColumn: "enrollmentID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_eventID",
                table: "Category",
                column: "eventID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_categoryID",
                table: "Enrollment",
                column: "categoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_eventID",
                table: "Enrollment",
                column: "eventID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_participantID",
                table: "Enrollment",
                column: "participantID");

            migrationBuilder.CreateIndex(
                name: "IX_Event_organiserID",
                table: "Event",
                column: "organiserID");

            migrationBuilder.CreateIndex(
                name: "IX_Result_enrollmentID",
                table: "Result",
                column: "enrollmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_roleID",
                table: "User",
                column: "roleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Result");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "UserRole");
        }
    }
}
