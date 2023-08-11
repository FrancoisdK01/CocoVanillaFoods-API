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

        //Not deleted
        //.OnDelete(DeleteBehavior.Restrict);

        //Delete
        //.OnDelete(DeleteBehavior.Cascade);

        //INF 370 team git test

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
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SystemPrivilege>().HasKey(sp => sp.Id);

            //Wine and Inventory
            //modelBuilder.Entity<Inventory>()
            //.HasOne<Wine>(i => i.Wine)
            //.WithMany(w => w.Inventories)
            //.HasForeignKey(i => i.WineID)
            //.OnDelete(DeleteBehavior.Restrict);

            //Wine and Employee
            //modelBuilder.Entity<Wine>()
            // .HasOne<Employee>(w => w.Employee)
            // .WithMany(e => e.Wines)
            // .HasForeignKey(w => w.EmployeeID)
            // .OnDelete(DeleteBehavior.Restrict);


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



            //Customer and Order
            modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerID)
            .OnDelete(DeleteBehavior.Restrict);

            //Order and OrderStatus
            modelBuilder.Entity<Order>()
             .HasOne(o => o.OrderStatus)
             .WithMany(os => os.Orders)
             .HasForeignKey(o => o.OrderStatusID)
             .OnDelete(DeleteBehavior.Restrict);

            //Inventory and stockTake_Item
            modelBuilder.Entity<Inventory>()
            .HasMany<StockTake_Item>(i => i.StockTake_Items)
            .WithOne(s => s.Inventory)
            .HasForeignKey(s => s.InventoryID)
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


            //OrderItem and Wine
            //modelBuilder.Entity<WineOrderItem>()
            // .HasOne(oi => oi.Wine)
            // .WithMany(w => w.OrderItems)
            // .HasForeignKey(oi => oi.WineId)
            // .OnDelete(DeleteBehavior.Restrict);

            //OrderItem and Refund
            //modelBuilder.Entity<WineOrderItem>()
            //.HasMany(o => o.Refunds)
            //.WithOne(r => r.OrderItem)
            //.HasForeignKey(r => r.OrderItemID)
            //.OnDelete(DeleteBehavior.Restrict);


            //Wine and WriteOffItem
            modelBuilder.Entity<Wine>()
             .HasMany(w => w.WriteOffItems)
             .WithOne(wi => wi.Wine)
             .HasForeignKey(wi => wi.WineID)
             .OnDelete(DeleteBehavior.Restrict);

            //WriteOffReason and WriteOffItem


            //WriteOff and WriteOffItem
            modelBuilder.Entity<WriteOff>()
            .HasMany(w => w.WriteOffItems)
            .WithOne(wi => wi.WriteOff)
            .HasForeignKey(wi => wi.WriteOffID);

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

            //Order and ShippingDetails
            modelBuilder.Entity<Order>()
            .HasOne(o => o.ShippingDetails)
            .WithOne(sd => sd.Order)
            .HasForeignKey<ShippingDetails>(sd => sd.OrderID)
            .OnDelete(DeleteBehavior.Cascade);

            //Order and OrderPayment
            modelBuilder.Entity<Order>()
             .HasOne(o => o.OrderPayment)
             .WithOne(op => op.Order)
             .HasForeignKey<OrderPayment>(op => op.OrderID)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WriteOff>()
                .HasOne(w => w.Employee)
                .WithMany(e => e.WriteOffs)
                .HasForeignKey(w => w.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
            .HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Booking>()
            //.HasMany(b => b.Tickets)
            //.WithOne(t => t.Booking)
            //.HasForeignKey(t => t.BookingId)
            //.OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
            .HasOne(b => b.BookingPayment)
            .WithOne(p => p.Booking)
            .HasForeignKey<BookingPayment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventReview>()
            .HasOne(r => r.Customer)
            .WithMany(c => c.EventReviews)
            .HasForeignKey(r => r.CustomerID)
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



            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Blacklist> Blacklists { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingPayment> BookingPayments { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventLocation> EventLocations { get; set; }
        public DbSet<EventPrice> EventPrices { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<EventReview> EventReviews { get; set; }
        public DbSet<EarlyBird> EarlyBird { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStatus> OrderStatus { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<WineOrderItem> OrderItems { get; set; }
        public DbSet<OrderPayment> OrderPayments { get; set; }
        public DbSet<RefundResponse> RefundResponses { get; set; }
        public DbSet<RefundType> RefundTypes { get; set; }
        public DbSet<ShippingDetails> ShippingDetails { get; set; }

        public DbSet<StockTake_Item> StockTakeItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierOrder> SupplierOrders { get; set; }
        public DbSet<SupplierPayment> SupplierPayments { get; set; }
        public DbSet<SystemPrivilege> SystemPrivileges { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        //public DbSet<User> Users { get; set; }
        public DbSet<VAT> VATs { get; set; }
        public DbSet<Varietal> Varietals { get; set; }
        public DbSet<Wine> Wines { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<WineType> WineTypes { get; set; }
        public DbSet<WriteOff> WriteOffs { get; set; }
        public DbSet<WriteOffItem> WriteOffItems { get; set; }
        public DbSet<WriteOff_Reason> WriteOffReasons { get; set; }
        public DbSet<SuperUser> SuperUser { get; set; }
        public DbSet<EventPayments> EventsPayments { get; set; }
        public DbSet<TicketPurchase> TicketPurchases { get; set; }

        //carts

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        //orderHistory

        public DbSet<WineOrder> WineOrders { get; set; }
        public DbSet<WineOrderItem> WineOrderItems { get; set; }

        //Refunds
        public DbSet<RefundRequest> RefundRequests { get; set; }


    }
}
