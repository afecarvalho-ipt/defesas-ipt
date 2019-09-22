using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Schedules.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Description = table.Column<string>(maxLength: 1024, nullable: true),
                    Location = table.Column<string>(maxLength: 128, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 128, nullable: false),
                    MaxStudentsPerSlot = table.Column<int>(nullable: false),
                    When = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentNumber = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentNumber);
                });

            migrationBuilder.CreateTable(
                name: "Schedule_Student",
                columns: table => new
                {
                    Student_Id = table.Column<string>(nullable: false),
                    Schedule_Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule_Student", x => new { x.Student_Id, x.Schedule_Id });
                    table.ForeignKey(
                        name: "FK_Schedule_Student_Schedules_Schedule_Id",
                        column: x => x.Schedule_Id,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedule_Student_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalTable: "Students",
                        principalColumn: "StudentNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleSlots",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(maxLength: 512, nullable: true),
                    IsAvailable = table.Column<bool>(nullable: false),
                    StartsAt = table.Column<DateTime>(nullable: false),
                    EndsAt = table.Column<DateTime>(nullable: false),
                    CompletedAt = table.Column<DateTime>(nullable: true),
                    ReservedAt = table.Column<DateTime>(nullable: true),
                    ReservedBy_Id = table.Column<string>(nullable: true),
                    Schedule_Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleSlots_Students_ReservedBy_Id",
                        column: x => x.ReservedBy_Id,
                        principalTable: "Students",
                        principalColumn: "StudentNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleSlots_Schedules_Schedule_Id",
                        column: x => x.Schedule_Id,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleSlot_Student",
                columns: table => new
                {
                    Student_Id = table.Column<string>(nullable: false),
                    ScheduleSlot_Id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleSlot_Student", x => new { x.ScheduleSlot_Id, x.Student_Id });
                    table.ForeignKey(
                        name: "FK_ScheduleSlot_Student_ScheduleSlots_ScheduleSlot_Id",
                        column: x => x.ScheduleSlot_Id,
                        principalTable: "ScheduleSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleSlot_Student_Students_Student_Id",
                        column: x => x.Student_Id,
                        principalTable: "Students",
                        principalColumn: "StudentNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_Student_Schedule_Id",
                table: "Schedule_Student",
                column: "Schedule_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlot_Student_Student_Id",
                table: "ScheduleSlot_Student",
                column: "Student_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_ReservedBy_Id",
                table: "ScheduleSlots",
                column: "ReservedBy_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_Schedule_Id",
                table: "ScheduleSlots",
                column: "Schedule_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumber",
                table: "Students",
                column: "StudentNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedule_Student");

            migrationBuilder.DropTable(
                name: "ScheduleSlot_Student");

            migrationBuilder.DropTable(
                name: "ScheduleSlots");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Schedules");
        }
    }
}
