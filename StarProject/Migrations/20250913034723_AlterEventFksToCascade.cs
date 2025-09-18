using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarProject.Migrations
{
	public partial class AlterEventFksToCascade : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// 先把舊的 FK（非級聯）移除（如果存在才移除，避免名稱對不到時失敗）
			migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Schedule_event')
    ALTER TABLE [Schedule]    DROP CONSTRAINT [FK_Schedule_event];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Participant_Event')
    ALTER TABLE [Participant] DROP CONSTRAINT [FK_Participant_Event];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_eventNotif_event')
    ALTER TABLE [EventNotif]  DROP CONSTRAINT [FK_eventNotif_event];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Collection_Event')
    ALTER TABLE [Collection]  DROP CONSTRAINT [FK_Collection_Event];
");

			// 重新建立為 ON DELETE CASCADE
			migrationBuilder.AddForeignKey(
				name: "FK_Schedule_event",
				table: "Schedule",
				column: "Event_No",
				principalTable: "Event",
				principalColumn: "No",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_Participant_Event",
				table: "Participant",
				column: "Event_No",
				principalTable: "Event",
				principalColumn: "No",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_eventNotif_event",
				table: "EventNotif",
				column: "Event_No",
				principalTable: "Event",
				principalColumn: "No",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_Collection_Event",
				table: "Collection",
				column: "Event_No",
				principalTable: "Event",
				principalColumn: "No",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// 還原成「不級聯」
			migrationBuilder.DropForeignKey("FK_Schedule_event", "Schedule");
			migrationBuilder.DropForeignKey("FK_Participant_Event", "Participant");
			migrationBuilder.DropForeignKey("FK_eventNotif_event", "EventNotif");
			migrationBuilder.DropForeignKey("FK_Collection_Event", "Collection");

			migrationBuilder.AddForeignKey(
				name: "FK_Schedule_event",
				table: "Schedule",
				column: "Event_No",
				principalTable: "Event",
				principalColumn: "No",
				onDelete: ReferentialAction.NoAction);

			migrationBuilder.AddForeignKey(
				name: "FK_Participant_Event",
				table: "Participant",
				column: "Event_No",
				principalTable: "Event",
				principalColumn: "No",
				onDelete: ReferentialAction.NoAction);

			migrationBuilder.AddForeignKey(
				name: "FK_eventNotif_event",
				table: "EventNotif",
				column: "Event_No",
				principalTable: "Event",
				principalColumn: "No",
				onDelete: ReferentialAction.NoAction);

			migrationBuilder.AddForeignKey(
				name: "FK_Collection_Event",
				table: "Collection",
				column: "Event_No",
				principalTable: "Event",
				principalColumn: "No",
				onDelete: ReferentialAction.NoAction);
		}
	}
}
