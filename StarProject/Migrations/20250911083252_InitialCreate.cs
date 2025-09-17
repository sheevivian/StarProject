using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StarProject.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dept",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeptCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DeptName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeptDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dept", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fee = table.Column<int>(type: "int", nullable: true),
                    Deposit = table.Column<int>(type: "int", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "FAQCategory",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQCategory", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "Knowledge",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Like = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Knowledge", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "LostInfo",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FoundDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OwnerPhone = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LostInfo", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransaction",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantTradeNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SourceID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentWay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProviderTransID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    PaidAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaidTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    RawResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentT__3214EC27F6F5A3F0", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "ProCategory",
                columns: table => new
                {
                    No = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ProCateg__3214D4A860C92A06", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "Promotion",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Limit = table.Column<int>(type: "int", nullable: true),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reuse = table.Column<bool>(type: "bit", nullable: false),
                    UsesTime = table.Column<string>(type: "nchar(10)", fixedLength: true, maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotion_PromotionNo", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Permissions = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "StarMap",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    MapLatitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    MapLongitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarMap", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "TicCategory",
                columns: table => new
                {
                    No = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TicCateg__3214D4A80970CBCD", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Account = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.No);
                });

            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    Event_No = table.Column<int>(type: "int", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Executed = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_Schedule_event",
                        column: x => x.Event_No,
                        principalTable: "Event",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "FAQ",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category_No = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQ", x => x.No);
                    table.ForeignKey(
                        name: "FK_FAQ_FAQCategory",
                        column: x => x.Category_No,
                        principalTable: "FAQCategory",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeContent",
                columns: table => new
                {
                    Knowledge_No = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    BlockType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_KnowledgeContent_Knowledge",
                        column: x => x.Knowledge_No,
                        principalTable: "Knowledge",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProCategory_No = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Product__3214D4A837C62CC8", x => x.No);
                    table.ForeignKey(
                        name: "FK_Product_ProCategory",
                        column: x => x.ProCategory_No,
                        principalTable: "ProCategory",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "PromotionRule",
                columns: table => new
                {
                    Promotion_No = table.Column<int>(type: "int", nullable: false),
                    Rule = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_PromotionRule_PromotionNo",
                        column: x => x.Promotion_No,
                        principalTable: "Promotion",
                        principalColumn: "No",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Emps",
                columns: table => new
                {
                    No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Role_No = table.Column<int>(type: "int", nullable: false),
                    Dept_No = table.Column<int>(type: "int", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmpCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    ForceChangePassword = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emps", x => x.No);
                    table.ForeignKey(
                        name: "FK_Emps_Emps",
                        column: x => x.Dept_No,
                        principalTable: "Dept",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_Emps_Roles",
                        column: x => x.Role_No,
                        principalTable: "Roles",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TicCategory_No = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Desc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ticket__3214D4A8DAF4E467", x => x.No);
                    table.ForeignKey(
                        name: "FK_Ticket_TicCategory",
                        column: x => x.TicCategory_No,
                        principalTable: "TicCategory",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "Cart",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AllTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAtTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cart", x => x.No);
                    table.ForeignKey(
                        name: "FK_Cart_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "LoginLogs",
                columns: table => new
                {
                    No = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogoutTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginLogs", x => x.No);
                    table.ForeignKey(
                        name: "FK_LoginLogs_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "OrderMaster",
                columns: table => new
                {
                    No = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Deliveryway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Deliveryfee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    AllTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MerchantTradeNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Order__3214D4A8D66B9260", x => x.No);
                    table.ForeignKey(
                        name: "FK_Order_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "Participant",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Event_No = table.Column<int>(type: "int", nullable: false),
                    Users_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegisterdDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Payment_No = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participant", x => x.No);
                    table.ForeignKey(
                        name: "FK_Participant_Event",
                        column: x => x.Event_No,
                        principalTable: "Event",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_Participant_PaymentTransaction",
                        column: x => x.Payment_No,
                        principalTable: "PaymentTransaction",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_Participant_Users",
                        column: x => x.Users_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "PromotionUsage",
                columns: table => new
                {
                    Promotion_No = table.Column<int>(type: "int", nullable: false),
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_PromotionUsage_Promotion",
                        column: x => x.Promotion_No,
                        principalTable: "Promotion",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_PromotionUsage_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AchievedLevel = table.Column<int>(type: "int", nullable: false),
                    AchievedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.No);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "UserSecurity",
                columns: table => new
                {
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TwoFAEnabled = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TwoFASecret = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastPasswordChange = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_UserSecurity_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "FAQKeyword",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FAQ_No = table.Column<int>(type: "int", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQKeyword", x => x.No);
                    table.ForeignKey(
                        name: "FK_FAQKeyword_FAQ",
                        column: x => x.FAQ_No,
                        principalTable: "FAQ",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "Collection",
                columns: table => new
                {
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Product_No = table.Column<int>(type: "int", nullable: true),
                    Event_No = table.Column<int>(type: "int", nullable: true),
                    Knowledge_No = table.Column<int>(type: "int", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_Collection_Event",
                        column: x => x.Event_No,
                        principalTable: "Event",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_Collection_Knowledge",
                        column: x => x.Knowledge_No,
                        principalTable: "Knowledge",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_Collection_Product",
                        column: x => x.Product_No,
                        principalTable: "Product",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_Collection_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "ProductImage",
                columns: table => new
                {
                    Product_No = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImgOrder = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "Img_ProNo_FK",
                        column: x => x.Product_No,
                        principalTable: "Product",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "ProductIntroduce",
                columns: table => new
                {
                    Product_No = table.Column<int>(type: "int", nullable: false),
                    Point = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "Intro_ProNo_FK",
                        column: x => x.Product_No,
                        principalTable: "Product",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "ProductReply",
                columns: table => new
                {
                    Product_No = table.Column<int>(type: "int", nullable: false),
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ratings = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "ProRe_ProNo_FK",
                        column: x => x.Product_No,
                        principalTable: "Product",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "ProRe_UserNo_FK",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "ProductStock",
                columns: table => new
                {
                    Product_No = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TransQuantity = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_ProductStock_ProductNo",
                        column: x => x.Product_No,
                        principalTable: "Product",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    No = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Emp_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionTime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.No);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Emps",
                        column: x => x.Emp_No,
                        principalTable: "Emps",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "CustomerService",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Reply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplyDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Emp_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerService", x => x.No);
                    table.ForeignKey(
                        name: "FK_CustomerService_Emps",
                        column: x => x.Emp_No,
                        principalTable: "Emps",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_CustomerService_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "ProductEdit",
                columns: table => new
                {
                    Emp_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Product_No = table.Column<int>(type: "int", nullable: false),
                    motion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Update = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_ProductEdit_Emps",
                        column: x => x.Emp_No,
                        principalTable: "Emps",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_ProductEdit_Product",
                        column: x => x.Product_No,
                        principalTable: "Product",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "TicketStock",
                columns: table => new
                {
                    Ticket_No = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "Tic_No_FK",
                        column: x => x.Ticket_No,
                        principalTable: "Ticket",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "CartItem",
                columns: table => new
                {
                    CartItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cart_No = table.Column<int>(type: "int", nullable: false),
                    Product_No = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DiscountedPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItem", x => x.CartItemId);
                    table.ForeignKey(
                        name: "FK_CartItem_Cart",
                        column: x => x.Cart_No,
                        principalTable: "Cart",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_CartItem_Product",
                        column: x => x.Product_No,
                        principalTable: "Product",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "OrderCS",
                columns: table => new
                {
                    No = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Order_No = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Reply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplyDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Emp_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCS_1", x => x.No);
                    table.ForeignKey(
                        name: "FK_OrderCS_Emps",
                        column: x => x.Emp_No,
                        principalTable: "Emps",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_OrderCS_Order",
                        column: x => x.Order_No,
                        principalTable: "OrderMaster",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_OrderCS_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "OrderDelivery",
                columns: table => new
                {
                    Order_No = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    User_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Receiver = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_OrderDelivery_OrderMaster",
                        column: x => x.Order_No,
                        principalTable: "OrderMaster",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_OrderDelivery_Users",
                        column: x => x.User_No,
                        principalTable: "Users",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Order_No = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DiscountedPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DiscountType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_OrderItem_Order",
                        column: x => x.Order_No,
                        principalTable: "OrderMaster",
                        principalColumn: "No",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatus",
                columns: table => new
                {
                    Order_No = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Motion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Update = table.Column<DateTime>(type: "datetime", nullable: false),
                    Emp_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_OrderEdit_Order",
                        column: x => x.Order_No,
                        principalTable: "OrderMaster",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_OrderStatus_Emps",
                        column: x => x.Emp_No,
                        principalTable: "Emps",
                        principalColumn: "No");
                });

            migrationBuilder.CreateTable(
                name: "EventNotif",
                columns: table => new
                {
                    Event_No = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Participant_No = table.Column<int>(type: "int", nullable: false),
                    Senttime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_EventNotif_Participant",
                        column: x => x.Participant_No,
                        principalTable: "Participant",
                        principalColumn: "No");
                    table.ForeignKey(
                        name: "FK_eventNotif_event",
                        column: x => x.Event_No,
                        principalTable: "Event",
                        principalColumn: "No");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Emp_No",
                table: "AuditLogs",
                column: "Emp_No");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_User_No",
                table: "Cart",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_Cart_No",
                table: "CartItem",
                column: "Cart_No");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_Product_No",
                table: "CartItem",
                column: "Product_No");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_Event_No",
                table: "Collection",
                column: "Event_No");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_Knowledge_No",
                table: "Collection",
                column: "Knowledge_No");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_Product_No",
                table: "Collection",
                column: "Product_No");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_User_No",
                table: "Collection",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerService_Emp_No",
                table: "CustomerService",
                column: "Emp_No");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerService_User_No",
                table: "CustomerService",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "UQ_Dept_DeptCode",
                table: "Dept",
                column: "DeptCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Dept_DeptName",
                table: "Dept",
                column: "DeptName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Emps_Dept_No",
                table: "Emps",
                column: "Dept_No");

            migrationBuilder.CreateIndex(
                name: "IX_Emps_Role_No",
                table: "Emps",
                column: "Role_No");

            migrationBuilder.CreateIndex(
                name: "UQ_Emps_EmpCode",
                table: "Emps",
                column: "EmpCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventNotif_Event_No",
                table: "EventNotif",
                column: "Event_No");

            migrationBuilder.CreateIndex(
                name: "IX_EventNotif_Participant_No",
                table: "EventNotif",
                column: "Participant_No");

            migrationBuilder.CreateIndex(
                name: "IX_FAQ_Category_No",
                table: "FAQ",
                column: "Category_No");

            migrationBuilder.CreateIndex(
                name: "IX_FAQKeyword_FAQ_No",
                table: "FAQKeyword",
                column: "FAQ_No");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeContent_Knowledge_No",
                table: "KnowledgeContent",
                column: "Knowledge_No");

            migrationBuilder.CreateIndex(
                name: "IX_LoginLogs_User_No",
                table: "LoginLogs",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCS_Emp_No",
                table: "OrderCS",
                column: "Emp_No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCS_Order_No",
                table: "OrderCS",
                column: "Order_No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCS_User_No",
                table: "OrderCS",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDelivery_Order_No",
                table: "OrderDelivery",
                column: "Order_No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDelivery_User_No",
                table: "OrderDelivery",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_Order_No",
                table: "OrderItem",
                column: "Order_No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMaster_User_No",
                table: "OrderMaster",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatus_Emp_No",
                table: "OrderStatus",
                column: "Emp_No");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatus_Order_No",
                table: "OrderStatus",
                column: "Order_No");

            migrationBuilder.CreateIndex(
                name: "IX_Participant_Event_No",
                table: "Participant",
                column: "Event_No");

            migrationBuilder.CreateIndex(
                name: "IX_Participant_Payment_No",
                table: "Participant",
                column: "Payment_No");

            migrationBuilder.CreateIndex(
                name: "IX_Participant_Users_No",
                table: "Participant",
                column: "Users_No");

            migrationBuilder.CreateIndex(
                name: "UQ__PaymentT__D6311911D4524583",
                table: "PaymentTransaction",
                column: "MerchantTradeNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProCategory_No",
                table: "Product",
                column: "ProCategory_No");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEdit_Emp_No",
                table: "ProductEdit",
                column: "Emp_No");

            migrationBuilder.CreateIndex(
                name: "IX_ProductEdit_Product_No",
                table: "ProductEdit",
                column: "Product_No");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImage_Product_No",
                table: "ProductImage",
                column: "Product_No");

            migrationBuilder.CreateIndex(
                name: "IX_ProductIntroduce_Product_No",
                table: "ProductIntroduce",
                column: "Product_No");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReply_Product_No",
                table: "ProductReply",
                column: "Product_No");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReply_User_No",
                table: "ProductReply",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStock_Product_No",
                table: "ProductStock",
                column: "Product_No");

            migrationBuilder.CreateIndex(
                name: "UK_Promotion_CouponCode",
                table: "Promotion",
                column: "CouponCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionRule_Promotion_No",
                table: "PromotionRule",
                column: "Promotion_No");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionUsage_Promotion_No",
                table: "PromotionUsage",
                column: "Promotion_No");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionUsage_User_No",
                table: "PromotionUsage",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_Event_No",
                table: "Schedule",
                column: "Event_No");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_TicCategory_No",
                table: "Ticket",
                column: "TicCategory_No");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStock_Ticket_No",
                table: "TicketStock",
                column: "Ticket_No");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_User_No",
                table: "UserAchievements",
                column: "User_No");

            migrationBuilder.CreateIndex(
                name: "UK_Users_Account",
                table: "Users",
                column: "Account",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Users_IdNumber",
                table: "Users",
                column: "IdNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UK_Users_Phone",
                table: "Users",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSecurity_User_No",
                table: "UserSecurity",
                column: "User_No");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CartItem");

            migrationBuilder.DropTable(
                name: "Collection");

            migrationBuilder.DropTable(
                name: "CustomerService");

            migrationBuilder.DropTable(
                name: "EventNotif");

            migrationBuilder.DropTable(
                name: "FAQKeyword");

            migrationBuilder.DropTable(
                name: "KnowledgeContent");

            migrationBuilder.DropTable(
                name: "LoginLogs");

            migrationBuilder.DropTable(
                name: "LostInfo");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "OrderCS");

            migrationBuilder.DropTable(
                name: "OrderDelivery");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "OrderStatus");

            migrationBuilder.DropTable(
                name: "ProductEdit");

            migrationBuilder.DropTable(
                name: "ProductImage");

            migrationBuilder.DropTable(
                name: "ProductIntroduce");

            migrationBuilder.DropTable(
                name: "ProductReply");

            migrationBuilder.DropTable(
                name: "ProductStock");

            migrationBuilder.DropTable(
                name: "PromotionRule");

            migrationBuilder.DropTable(
                name: "PromotionUsage");

            migrationBuilder.DropTable(
                name: "Schedule");

            migrationBuilder.DropTable(
                name: "StarMap");

            migrationBuilder.DropTable(
                name: "TicketStock");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserSecurity");

            migrationBuilder.DropTable(
                name: "Cart");

            migrationBuilder.DropTable(
                name: "Participant");

            migrationBuilder.DropTable(
                name: "FAQ");

            migrationBuilder.DropTable(
                name: "Knowledge");

            migrationBuilder.DropTable(
                name: "OrderMaster");

            migrationBuilder.DropTable(
                name: "Emps");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Promotion");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "PaymentTransaction");

            migrationBuilder.DropTable(
                name: "FAQCategory");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Dept");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ProCategory");

            migrationBuilder.DropTable(
                name: "TicCategory");
        }
    }
}
