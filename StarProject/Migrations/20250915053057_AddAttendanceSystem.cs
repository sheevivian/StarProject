using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarProject.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emps_Emps",
                table: "Emps");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Order",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK__TicCateg__3214D4A80970CBCD",
                table: "TicCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProCateg__3214D4A860C92A06",
                table: "ProCategory");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_Order_No",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "PromotionRule");

            migrationBuilder.DropColumn(
                name: "Rule",
                table: "PromotionRule");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "PromotionRule");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Participant");

            migrationBuilder.RenameColumn(
                name: "RegisterdDate",
                table: "Participant",
                newName: "RegisteredDate");

            migrationBuilder.AddColumn<bool>(
                name: "CS",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CoNE",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CoNList",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Emp",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Event",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Info",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OA",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Order",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pd",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pm",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Tic",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "User",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ConditionAmount",
                table: "PromotionRule",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PromotionRule",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                table: "PromotionRule",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuleType",
                table: "PromotionRule",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Percentage");

            migrationBuilder.AddColumn<string>(
                name: "TargetCategory",
                table: "PromotionRule",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ALL");

            migrationBuilder.AddColumn<int>(
                name: "No",
                table: "ProductImage",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Payment_No",
                table: "Participant",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "LostInfo",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "ForceChangePassword",
                table: "Emps",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__TicCateg__3214D4A8F8D1D924",
                table: "TicCategory",
                column: "No");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage",
                column: "No");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProCateg__3214D4A8C77E0BC9",
                table: "ProCategory",
                column: "No");

            migrationBuilder.CreateTable(
                name: "CompanyNotify",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PublishDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyNotify", x => x.No);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Emps_Dept",
                table: "Emps",
                column: "Dept_No",
                principalTable: "Dept",
                principalColumn: "No");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emps_Dept",
                table: "Emps");

            migrationBuilder.DropTable(
                name: "CompanyNotify");

            migrationBuilder.DropPrimaryKey(
                name: "PK__TicCateg__3214D4A8F8D1D924",
                table: "TicCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProCateg__3214D4A8C77E0BC9",
                table: "ProCategory");

            migrationBuilder.DropColumn(
                name: "CS",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CoNE",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CoNList",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Emp",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Event",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Info",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "OA",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Pd",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Pm",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Tic",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "User",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ConditionAmount",
                table: "PromotionRule");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PromotionRule");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                table: "PromotionRule");

            migrationBuilder.DropColumn(
                name: "RuleType",
                table: "PromotionRule");

            migrationBuilder.DropColumn(
                name: "TargetCategory",
                table: "PromotionRule");

            migrationBuilder.DropColumn(
                name: "No",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "LostInfo");

            migrationBuilder.RenameColumn(
                name: "RegisteredDate",
                table: "Participant",
                newName: "RegisterdDate");

            migrationBuilder.AddColumn<string>(
                name: "Permissions",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "PromotionRule",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rule",
                table: "PromotionRule",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "PromotionRule",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Payment_No",
                table: "Participant",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Participant",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<bool>(
                name: "ForceChangePassword",
                table: "Emps",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddPrimaryKey(
                name: "PK__TicCateg__3214D4A80970CBCD",
                table: "TicCategory",
                column: "No");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProCateg__3214D4A860C92A06",
                table: "ProCategory",
                column: "No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_Order_No",
                table: "OrderItem",
                column: "Order_No");

            migrationBuilder.AddForeignKey(
                name: "FK_Emps_Emps",
                table: "Emps",
                column: "Dept_No",
                principalTable: "Dept",
                principalColumn: "No");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Order",
                table: "OrderItem",
                column: "Order_No",
                principalTable: "OrderMaster",
                principalColumn: "No",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
