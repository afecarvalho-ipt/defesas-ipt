using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Schedules.Migrations
{
    public partial class ScheduleSlotTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Timestamp",
                table: "ScheduleSlots",
                maxLength: 8,
                rowVersion: true,
                nullable: true);

            migrationBuilder.Sql(@"
                CREATE TRIGGER SetScheduleSlotsTimestampOnInsert
                AFTER INSERT ON ScheduleSlots
                BEGIN
	                UPDATE ScheduleSlots
	                SET Timestamp = randomblob(8)
	                WHERE rowid = NEW.rowid;
                END
            ");
            
            migrationBuilder.Sql(@" 
                CREATE TRIGGER SetScheduleSlotsTimestampOnUpdate
                AFTER UPDATE ON ScheduleSlots
                BEGIN
	                UPDATE ScheduleSlots
	                SET Timestamp = randomblob(8)
	                WHERE rowid = NEW.rowid;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER SetScheduleSlotsTimestampOnUpdate");
            migrationBuilder.Sql("DROP TRIGGER SetScheduleSlotsTimestampOnInsert");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "ScheduleSlots");
        }
    }
}
