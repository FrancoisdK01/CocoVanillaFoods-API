using API.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.Linq;

namespace API.Data
{
    public class MyDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<EarlyBird>(eb =>
            {
                eb.HasKey(e => e.EarlyBirdID);
            });

            modelBuilder.Entity<Event>(e =>
            {
                e.HasKey(e => e.EventID);
                e.HasOne(e => e.EarlyBird)
                    .WithMany(eb => eb.Events)
                    .HasForeignKey(e => e.EarlyBirdID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SystemPrivilege>().HasKey(sp => sp.Id);

            // Cart and CartItem relationship
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
               .HasOne(c => c.Customer)
               .WithOne(cu => cu.Cart)
                .HasForeignKey<Cart>(c => c.CustomerID);

            // CartItem and Wine relationship
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Wine)
                .WithMany(w => w.CartItems)
                .HasForeignKey(ci => ci.WineID)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<WineOrder>()
             .HasOne(o => o.OrderStatus)
                .WithMany(os => os.WineOrders)
                 .HasForeignKey(o => o.OrderStatusId)
                     .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<SupplierOrder>()
            .HasOne<Supplier>(so => so.Supplier)
            .WithMany(s => s.SupplierOrders)
            .HasForeignKey(so => so.SupplierID)
            .OnDelete(DeleteBehavior.Restrict);

            // Wine and WineType relationship
            modelBuilder.Entity<Wine>()
                .HasOne(w => w.WineType)
                .WithMany(wt => wt.Wines)
                .HasForeignKey(w => w.WineTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            // Wine and Varietal relationship
            modelBuilder.Entity<Wine>()
                .HasOne(w => w.Varietal)
                .WithMany(v => v.Wines)
                .HasForeignKey(w => w.VarietalID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WineOrderItem>()
              .HasOne(oi => oi.WineOrder)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId) // <- change here
                 .OnDelete(DeleteBehavior.Cascade);


            //WishlistItem and Wine
            modelBuilder.Entity<WishlistItem>()
             .HasOne(wli => wli.Wine)
             .WithMany(w => w.WishlistItems)
             .HasForeignKey(wli => wli.WineID)
             .OnDelete(DeleteBehavior.Cascade);

            //WishListitem and Wishlist
            modelBuilder.Entity<WishlistItem>()
            .HasOne(wli => wli.Wishlist)
            .WithMany(wl => wl.WishlistItems)
            .HasForeignKey(wli => wli.WishlistID)
            .OnDelete(DeleteBehavior.Cascade);

            //Customer and Wishlist
            modelBuilder.Entity<Wishlist>()
            .HasOne(wl => wl.Customer)
            .WithOne(c => c.Wishlist)
            .HasForeignKey<Wishlist>(wl => wl.CustomerID)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<WriteOff>()
                .HasOne(w => w.Employee)
                .WithMany(e => e.WriteOffs)
                .HasForeignKey(w => w.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<RefundRequest>()
                .HasOne(rr => rr.WineOrder)
                  .WithMany(wo => wo.RefundRequests) // Assuming there's a collection of RefundRequests in WineOrder
                    .HasForeignKey(rr => rr.WineId);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.SuperUser)
                .WithMany(su => su.Employees)
                .HasForeignKey(e => e.SuperUserID);

            modelBuilder.Entity<SystemPrivilege>()
            .HasKey(sp => sp.Id);

            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<OrderStatus>()
               .Property(o => o.OrderStatusId)
                     .ValueGeneratedNever();


            modelBuilder.Entity<OrderStatus>().HasData(
              new OrderStatus { OrderStatusId = 1, StatusName = "ClientOrderPlaced" },
                new OrderStatus { OrderStatusId = 2, StatusName = "SupplierOrderPlaced" },
                  new OrderStatus { OrderStatusId = 3, StatusName = "Received" },
                        new OrderStatus { OrderStatusId = 4, StatusName = "Collected" });

            //modelBuilder.Entity<TicketPurchasedStatus>()
            //.HasKey(tps => tps.Id);  // Primary Key

            modelBuilder.Entity<TicketPurchasedStatus>()
                .HasOne(p => p.TicketPurchase)
                     .WithOne(i => i.TicketPurchasedStatus)
                       .HasForeignKey<TicketPurchasedStatus>(p => p.TicketPurchaseId);

            modelBuilder.Entity<SupplierOrder>()
                .HasOne(so => so.Supplier)
                 .WithMany(s => s.SupplierOrders)
                 .HasForeignKey(so => so.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);

            // Add these lines for SupplierOrderStatus
            modelBuilder.Entity<SupplierOrder>()
                .HasOne(so => so.SupplierOrderStatus)
                .WithOne(sos => sos.SupplierOrder)
                .HasForeignKey<SupplierOrderStatus>(sos => sos.SupplierOrderID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupplierOrderStatus>()
                .HasKey(sos => sos.SupplierOrderStatusID);

            // ... (existing model configurations)

            base.OnModelCreating(modelBuilder);
        }


        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Blacklist> Blacklists { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventPrice> EventPrices { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<EarlyBird> EarlyBird { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<OrderStatus> OrderStatus { get; set; }
        public DbSet<WineOrderItem> OrderItems { get; set; }
        public DbSet<RefundResponse> RefundResponses { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierOrder> SupplierOrders { get; set; }
        public DbSet<SupplierOrderStatus> SupplierOrderStatuses { get; set; }
        public DbSet<SystemPrivilege> SystemPrivileges { get; set; }
        public DbSet<VAT> VATs { get; set; }
        public DbSet<Varietal> Varietals { get; set; }
        public DbSet<Wine> Wines { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<WineType> WineTypes { get; set; }
        public DbSet<WriteOff> WriteOffs { get; set; }
        public DbSet<WriteOff_Reason> WriteOffReasons { get; set; }
        public DbSet<SuperUser> SuperUser { get; set; }
        public DbSet<EventPayments> EventsPayments { get; set; }
        public DbSet<TicketPurchase> TicketPurchases { get; set; }
        public DbSet<TicketPurchasedStatus> TicketPurchasedStatuses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<WineOrder> WineOrders { get; set; }
        public DbSet<WineOrderItem> WineOrderItems { get; set; }
        public DbSet<RefundRequest> RefundRequests { get; set; }
        public DbSet<HelpResource> HelpResources { get; set; }




    }
}