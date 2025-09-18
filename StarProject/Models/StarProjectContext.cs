using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StarProject.Models;

public partial class StarProjectContext : DbContext
{
    public StarProjectContext(DbContextOptions<StarProjectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<CompanyNotify> CompanyNotifies { get; set; }

    public virtual DbSet<CustomerService> CustomerServices { get; set; }

    public virtual DbSet<Dept> Depts { get; set; }

    public virtual DbSet<Emp> Emps { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventNotif> EventNotifs { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<Faqcategory> Faqcategories { get; set; }

    public virtual DbSet<Faqkeyword> Faqkeywords { get; set; }

    public virtual DbSet<Knowledge> Knowledges { get; set; }

    public virtual DbSet<KnowledgeContent> KnowledgeContents { get; set; }

    public virtual DbSet<LeaveApplication> LeaveApplications { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }

    public virtual DbSet<LoginLog> LoginLogs { get; set; }

    public virtual DbSet<LostInfo> LostInfos { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<NewsImage> NewsImages { get; set; }

    public virtual DbSet<OrderC> OrderCs { get; set; }

    public virtual DbSet<OrderDelivery> OrderDeliveries { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderMaster> OrderMasters { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<ProCategory> ProCategories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductEdit> ProductEdits { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductIntroduce> ProductIntroduces { get; set; }

    public virtual DbSet<ProductReply> ProductReplies { get; set; }

    public virtual DbSet<ProductStock> ProductStocks { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionRule> PromotionRules { get; set; }

    public virtual DbSet<PromotionUsage> PromotionUsages { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<StarMap> StarMaps { get; set; }

    public virtual DbSet<TicCategory> TicCategories { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketStock> TicketStocks { get; set; }

    public virtual DbSet<TicketTransStock> TicketTransStocks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAchievement> UserAchievements { get; set; }

    public virtual DbSet<UserSecurity> UserSecurities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.Property(e => e.EmpNo)
                .HasMaxLength(50)
                .HasColumnName("Emp_No");

            entity.HasOne(d => d.EmpNoNavigation).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.EmpNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttendanceRecordds_Emps");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.ActionTime).HasColumnType("datetime");
            entity.Property(e => e.EmpNo)
                .HasMaxLength(50)
                .HasColumnName("Emp_No");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.TableName).HasMaxLength(50);

            entity.HasOne(d => d.EmpNoNavigation).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.EmpNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLogs_Emps");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("Cart");

            entity.Property(e => e.AllTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.CreatedTime).HasColumnType("datetime");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAtTime).HasColumnType("datetime");
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.UserNoNavigation).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cart_Users");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItem");

            entity.Property(e => e.CartNo).HasColumnName("Cart_No");
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountedPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductNo).HasColumnName("Product_No");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.CartNoNavigation).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartItem_Cart");

            entity.HasOne(d => d.ProductNoNavigation).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartItem_Product");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Collection");

            entity.Property(e => e.Category).HasMaxLength(20);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.EventNo).HasColumnName("Event_No");
            entity.Property(e => e.KnowledgeNo).HasColumnName("Knowledge_No");
            entity.Property(e => e.ProductNo).HasColumnName("Product_No");
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.EventNoNavigation).WithMany()
                .HasForeignKey(d => d.EventNo)
                .HasConstraintName("FK_Collection_Event");

            entity.HasOne(d => d.KnowledgeNoNavigation).WithMany()
                .HasForeignKey(d => d.KnowledgeNo)
                .HasConstraintName("FK_Collection_Knowledge");

            entity.HasOne(d => d.ProductNoNavigation).WithMany()
                .HasForeignKey(d => d.ProductNo)
                .HasConstraintName("FK_Collection_Product");

            entity.HasOne(d => d.UserNoNavigation).WithMany()
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Collection_Users");
        });

        modelBuilder.Entity<CompanyNotify>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("CompanyNotify");

            entity.Property(e => e.Category).HasMaxLength(30);
            entity.Property(e => e.PublishDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<CustomerService>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("CustomerService");

            entity.Property(e => e.Category).HasMaxLength(30);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.EmpNo)
                .HasMaxLength(50)
                .HasColumnName("Emp_No");
            entity.Property(e => e.ReplyDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.EmpNoNavigation).WithMany(p => p.CustomerServices)
                .HasForeignKey(d => d.EmpNo)
                .HasConstraintName("FK_CustomerService_Emps");

            entity.HasOne(d => d.UserNoNavigation).WithMany(p => p.CustomerServices)
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerService_Users");
        });

        modelBuilder.Entity<Dept>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("Dept");

            entity.HasIndex(e => e.DeptCode, "UQ_Dept_DeptCode").IsUnique();

            entity.HasIndex(e => e.DeptName, "UQ_Dept_DeptName").IsUnique();

            entity.Property(e => e.DeptCode).HasMaxLength(5);
            entity.Property(e => e.DeptDescription).HasMaxLength(255);
            entity.Property(e => e.DeptName).HasMaxLength(50);
        });

        modelBuilder.Entity<Emp>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.HasIndex(e => e.EmpCode, "UQ_Emps_EmpCode").IsUnique();

            entity.Property(e => e.No).HasMaxLength(50);
            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.DeptNo).HasColumnName("Dept_No");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmpCode).HasMaxLength(50);
            entity.Property(e => e.HireDate).HasColumnType("datetime");
            entity.Property(e => e.IdNumber).HasMaxLength(50);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(50);
            entity.Property(e => e.PasswordSalt).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.RoleNo).HasColumnName("Role_No");

            entity.HasOne(d => d.DeptNoNavigation).WithMany(p => p.Emps)
                .HasForeignKey(d => d.DeptNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Emps_Dept");

            entity.HasOne(d => d.RoleNoNavigation).WithMany(p => p.Emps)
                .HasForeignKey(d => d.RoleNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Emps_Roles");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK_event");

            entity.ToTable("Event");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedTime).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<EventNotif>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("EventNotif");

            entity.HasIndex(e => new { e.EventNo, e.ParticipantNo, e.Category }, "UQ_EventNotif_Event_Participant_Category").IsUnique();

            entity.Property(e => e.Category).HasMaxLength(20);
            entity.Property(e => e.EventNo).HasColumnName("Event_No");
            entity.Property(e => e.ParticipantNo).HasColumnName("Participant_No");
            entity.Property(e => e.Senttime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.EventNoNavigation).WithMany()
                .HasForeignKey(d => d.EventNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_eventNotif_event");

            entity.HasOne(d => d.ParticipantNoNavigation).WithMany()
                .HasForeignKey(d => d.ParticipantNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EventNotif_Participant");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("FAQ");

            entity.Property(e => e.CategoryNo).HasColumnName("Category_No");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.CategoryNoNavigation).WithMany(p => p.Faqs)
                .HasForeignKey(d => d.CategoryNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FAQ_FAQCategory");
        });

        modelBuilder.Entity<Faqcategory>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("FAQCategory");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Faqkeyword>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("FAQKeyword");

            entity.Property(e => e.FaqNo).HasColumnName("FAQ_No");
            entity.Property(e => e.Keyword).HasMaxLength(50);

            entity.HasOne(d => d.FaqNoNavigation).WithMany(p => p.Faqkeywords)
                .HasForeignKey(d => d.FaqNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FAQKeyword_FAQ");
        });

        modelBuilder.Entity<Knowledge>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("Knowledge");

            entity.Property(e => e.Author).HasMaxLength(20);
            entity.Property(e => e.Category).HasMaxLength(30);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<KnowledgeContent>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("KnowledgeContent");

            entity.Property(e => e.BlockType).HasMaxLength(20);
            entity.Property(e => e.KnowledgeNo).HasColumnName("Knowledge_No");

            entity.HasOne(d => d.KnowledgeNoNavigation).WithMany()
                .HasForeignKey(d => d.KnowledgeNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KnowledgeContent_Knowledge");
        });

        modelBuilder.Entity<LeaveApplication>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.Property(e => e.EmpNo)
                .HasMaxLength(50)
                .HasColumnName("Emp_No");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalDays).HasColumnType("decimal(3, 1)");

            entity.HasOne(d => d.EmpNoNavigation).WithMany(p => p.LeaveApplications)
                .HasForeignKey(d => d.EmpNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveApplications_Emps");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveApplications)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveApplications_LeaveTypes");
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.Property(e => e.TypeCode).HasMaxLength(20);
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<LoginLog>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.Property(e => e.DeviceInfo).HasMaxLength(200);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.UserNoNavigation).WithMany(p => p.LoginLogs)
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoginLogs_Users");
        });

        modelBuilder.Entity<LostInfo>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("LostInfo");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.FoundDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.OwnerName).HasMaxLength(20);
            entity.Property(e => e.OwnerPhone).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(30);
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.Property(e => e.Category).HasMaxLength(30);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.PublishDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<NewsImage>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("NewsImage");

            entity.Property(e => e.NewsNo).HasColumnName("News_No");

            entity.HasOne(d => d.NewsNoNavigation).WithMany(p => p.NewsImages)
                .HasForeignKey(d => d.NewsNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NewsImage_News");
        });

        modelBuilder.Entity<OrderC>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK_OrderCS_1");

            entity.ToTable("OrderCS");

            entity.Property(e => e.Category).HasMaxLength(30);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.EmpNo)
                .HasMaxLength(50)
                .HasColumnName("Emp_No");
            entity.Property(e => e.OrderNo)
                .HasMaxLength(10)
                .HasColumnName("Order_No");
            entity.Property(e => e.ReplyDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(100);
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.EmpNoNavigation).WithMany(p => p.OrderCs)
                .HasForeignKey(d => d.EmpNo)
                .HasConstraintName("FK_OrderCS_Emps");

            entity.HasOne(d => d.OrderNoNavigation).WithMany(p => p.OrderCs)
                .HasForeignKey(d => d.OrderNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderCS_Order");

            entity.HasOne(d => d.UserNoNavigation).WithMany(p => p.OrderCs)
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderCS_Users");
        });

        modelBuilder.Entity<OrderDelivery>(entity =>
        {
            entity.HasKey(e => e.DeliveryId);

            entity.ToTable("OrderDelivery");

            entity.Property(e => e.Notes).HasMaxLength(200);
            entity.Property(e => e.OrderNo)
                .HasMaxLength(10)
                .HasColumnName("Order_No");
            entity.Property(e => e.RecipientAddress).HasMaxLength(200);
            entity.Property(e => e.RecipientName).HasMaxLength(50);
            entity.Property(e => e.RecipientPhone).HasMaxLength(20);
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.OrderNoNavigation).WithMany(p => p.OrderDeliveries)
                .HasForeignKey(d => d.OrderNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDelivery_OrderMaster");

            entity.HasOne(d => d.UserNoNavigation).WithMany(p => p.OrderDeliveries)
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDelivery_Users");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.ListId);

            entity.ToTable("OrderItem");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.Discount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountType).HasMaxLength(50);
            entity.Property(e => e.DiscountedPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.OrderNo)
                .HasMaxLength(10)
                .HasColumnName("Order_No");
            entity.Property(e => e.ProductNo).HasColumnName("Product_No");
            entity.Property(e => e.TicketNo).HasColumnName("Ticket_No");
            entity.Property(e => e.Type).HasMaxLength(30);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.OrderNoNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItem_OrderMaster");

            entity.HasOne(d => d.TicketNoNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.TicketNo)
                .HasConstraintName("FK_OrderItem_Ticket");
        });

        modelBuilder.Entity<OrderMaster>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK__Order__3214D4A8D66B9260");

            entity.ToTable("OrderMaster");

            entity.Property(e => e.No).HasMaxLength(10);
            entity.Property(e => e.AllTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Deliveryfee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Deliveryway).HasMaxLength(50);
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DiscountType).HasMaxLength(50);
            entity.Property(e => e.MerchantTradeNo).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.UserNoNavigation).WithMany(p => p.OrderMasters)
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Users");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__OrderSta__C8EE2063A62086B4");

            entity.ToTable("OrderStatus");

            entity.Property(e => e.Notes).HasMaxLength(200);
            entity.Property(e => e.StatusTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StatusType).HasMaxLength(50);

            entity.HasOne(d => d.Delivery).WithMany(p => p.OrderStatuses)
                .HasForeignKey(d => d.DeliveryId)
                .HasConstraintName("FK_OrderStatus_Delivery");
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("Participant");

            entity.HasIndex(e => e.Code, "UQ_Participant_Code").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(7);
            entity.Property(e => e.EventNo).HasColumnName("Event_No");
            entity.Property(e => e.PaymentNo)
                .HasMaxLength(50)
                .HasColumnName("Payment_No");
            entity.Property(e => e.RegisteredDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UsersNo)
                .HasMaxLength(50)
                .HasColumnName("Users_No");

            entity.HasOne(d => d.EventNoNavigation).WithMany(p => p.Participants)
                .HasForeignKey(d => d.EventNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Participant_Event");

            entity.HasOne(d => d.PaymentNoNavigation).WithMany(p => p.Participants)
                .HasForeignKey(d => d.PaymentNo)
                .HasConstraintName("FK_Participant_PaymentTransaction");

            entity.HasOne(d => d.UsersNoNavigation).WithMany(p => p.Participants)
                .HasForeignKey(d => d.UsersNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Participant_Users");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK__PaymentT__3214EC27F6F5A3F0");

            entity.ToTable("PaymentTransaction");

            entity.HasIndex(e => e.MerchantTradeNo, "UQ__PaymentT__D6311911D4524583").IsUnique();

            entity.Property(e => e.No).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MerchantTradeNo).HasMaxLength(30);
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaidTime).HasColumnType("datetime");
            entity.Property(e => e.PaymentProvider).HasMaxLength(20);
            entity.Property(e => e.PaymentWay).HasMaxLength(20);
            entity.Property(e => e.ProviderTransId)
                .HasMaxLength(100)
                .HasColumnName("ProviderTransID");
            entity.Property(e => e.SourceId)
                .HasMaxLength(10)
                .HasColumnName("SourceID");
            entity.Property(e => e.SourceType).HasMaxLength(10);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING");
        });

        modelBuilder.Entity<ProCategory>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK__ProCateg__3214D4A88AEDB099");

            entity.ToTable("ProCategory");

            entity.Property(e => e.No).HasMaxLength(6);
            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK__Product__3214D4A837C62CC8");

            entity.ToTable("Product");

            entity.Property(e => e.No).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ProCategoryNo)
                .HasMaxLength(6)
                .HasColumnName("ProCategory_No");
            entity.Property(e => e.ReleaseDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(10);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.ProCategoryNoNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProCategoryNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_ProCategory");
        });

        modelBuilder.Entity<ProductEdit>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ProductEdit");

            entity.Property(e => e.EmpNo)
                .HasMaxLength(50)
                .HasColumnName("Emp_No");
            entity.Property(e => e.Motion)
                .HasMaxLength(100)
                .HasColumnName("motion");
            entity.Property(e => e.ProductNo).HasColumnName("Product_No");
            entity.Property(e => e.Update).HasColumnType("datetime");

            entity.HasOne(d => d.EmpNoNavigation).WithMany()
                .HasForeignKey(d => d.EmpNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductEdit_Emps");

            entity.HasOne(d => d.ProductNoNavigation).WithMany()
                .HasForeignKey(d => d.ProductNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductEdit_Product");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("ProductImage");

            entity.Property(e => e.ProductNo).HasColumnName("Product_No");

            entity.HasOne(d => d.ProductNoNavigation).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Img_ProNo_FK");
        });

        modelBuilder.Entity<ProductIntroduce>(entity =>
        {
            entity.HasKey(e => e.ProductNo);

            entity.ToTable("ProductIntroduce");

            entity.Property(e => e.ProductNo)
                .ValueGeneratedNever()
                .HasColumnName("Product_No");
            entity.Property(e => e.Point).HasMaxLength(50);

            entity.HasOne(d => d.ProductNoNavigation).WithOne(p => p.ProductIntroduce)
                .HasForeignKey<ProductIntroduce>(d => d.ProductNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Intro_ProNo_FK");
        });

        modelBuilder.Entity<ProductReply>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ProductReply");

            entity.Property(e => e.ProductNo).HasColumnName("Product_No");
            entity.Property(e => e.Ratings).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.ProductNoNavigation).WithMany()
                .HasForeignKey(d => d.ProductNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ProRe_ProNo_FK");

            entity.HasOne(d => d.UserNoNavigation).WithMany()
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ProRe_UserNo_FK");
        });

        modelBuilder.Entity<ProductStock>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK__ProductS__3214D4A8DFF6CDC9");

            entity.ToTable("ProductStock");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.ProductNo).HasColumnName("Product_No");
            entity.Property(e => e.Type).HasMaxLength(10);

            entity.HasOne(d => d.ProductNoNavigation).WithMany(p => p.ProductStocks)
                .HasForeignKey(d => d.ProductNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductStock_Product");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK_Promotion_PromotionNo");

            entity.ToTable("Promotion");

            entity.HasIndex(e => e.CouponCode, "UK_Promotion_CouponCode").IsUnique();

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<PromotionRule>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PromotionRule");

            entity.Property(e => e.ConditionType).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MemberLevel).HasMaxLength(50);
            entity.Property(e => e.PromotionNo).HasColumnName("Promotion_No");
            entity.Property(e => e.RuleType)
                .HasMaxLength(50)
                .HasDefaultValue("Percentage");
            entity.Property(e => e.TargetCategory)
                .HasMaxLength(50)
                .HasDefaultValue("ALL");

            entity.HasOne(d => d.PromotionNoNavigation).WithMany()
                .HasForeignKey(d => d.PromotionNo)
                .HasConstraintName("FK_PromotionRule_PromotionNo");
        });

        modelBuilder.Entity<PromotionUsage>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PromotionUsage");

            entity.Property(e => e.PromotionNo).HasColumnName("Promotion_No");
            entity.Property(e => e.UsedDate).HasColumnType("datetime");
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.PromotionNoNavigation).WithMany()
                .HasForeignKey(d => d.PromotionNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionUsage_Promotion");

            entity.HasOne(d => d.UserNoNavigation).WithMany()
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionUsage_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.Property(e => e.CoNe).HasColumnName("CoNE");
            entity.Property(e => e.CoNlist).HasColumnName("CoNList");
            entity.Property(e => e.Cs).HasColumnName("CS");
            entity.Property(e => e.Oa).HasColumnName("OA");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.EventNo);

            entity.ToTable("Schedule");

            entity.Property(e => e.EventNo)
                .ValueGeneratedNever()
                .HasColumnName("Event_No");
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.ReleaseDate).HasColumnType("datetime");

            entity.HasOne(d => d.EventNoNavigation).WithOne(p => p.Schedule)
                .HasForeignKey<Schedule>(d => d.EventNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_event");
        });

        modelBuilder.Entity<StarMap>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("StarMap");

            entity.Property(e => e.MapLatitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.MapLongitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TicCategory>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK__TicCateg__3214D4A8E1C7D298");

            entity.ToTable("TicCategory");

            entity.Property(e => e.No).HasMaxLength(6);
            entity.Property(e => e.Name).HasMaxLength(20);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.No).HasName("PK__Ticket__3214D4A8DAF4E467");

            entity.ToTable("Ticket");

            entity.Property(e => e.No).ValueGeneratedNever();
            entity.Property(e => e.Desc).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ReleaseDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(10);
            entity.Property(e => e.TicCategoryNo)
                .HasMaxLength(6)
                .HasColumnName("TicCategory_No");
            entity.Property(e => e.Type).HasMaxLength(30);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");

            entity.HasOne(d => d.TicCategoryNoNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TicCategoryNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_TicCategory");
        });

        modelBuilder.Entity<TicketStock>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("TicketStock");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.TicketNo).HasColumnName("Ticket_No");

            entity.HasOne(d => d.TicketNoNavigation).WithMany(p => p.TicketStocks)
                .HasForeignKey(d => d.TicketNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Tic_No_FK");
        });

        modelBuilder.Entity<TicketTransStock>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.ToTable("TicketTransStock");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(50);
            entity.Property(e => e.TicketNo).HasColumnName("Ticket_No");
            entity.Property(e => e.Type).HasMaxLength(10);

            entity.HasOne(d => d.TicketNoNavigation).WithMany(p => p.TicketTransStocks)
                .HasForeignKey(d => d.TicketNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketTransStock_Ticket");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.HasIndex(e => e.Account, "UK_Users_Account").IsUnique();

            entity.HasIndex(e => e.Email, "UK_Users_Email").IsUnique();

            entity.HasIndex(e => e.IdNumber, "UK_Users_IdNumber").IsUnique();

            entity.HasIndex(e => e.Phone, "UK_Users_Phone").IsUnique();

            entity.Property(e => e.No).HasMaxLength(50);
            entity.Property(e => e.Account).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.IdNumber).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasKey(e => e.No);

            entity.Property(e => e.AchievedDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.UserNoNavigation).WithMany(p => p.UserAchievements)
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAchievements_Users");
        });

        modelBuilder.Entity<UserSecurity>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserSecurity");

            entity.Property(e => e.TwoFaenabled)
                .HasMaxLength(50)
                .HasColumnName("TwoFAEnabled");
            entity.Property(e => e.TwoFasecret)
                .HasMaxLength(50)
                .HasColumnName("TwoFASecret");
            entity.Property(e => e.UserNo)
                .HasMaxLength(50)
                .HasColumnName("User_No");

            entity.HasOne(d => d.UserNoNavigation).WithMany()
                .HasForeignKey(d => d.UserNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSecurity_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
