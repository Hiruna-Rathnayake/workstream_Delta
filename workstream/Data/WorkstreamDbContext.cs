using Microsoft.EntityFrameworkCore;
using workstream.Model;
using System.Data;
using System.Security;

namespace workstream.Data
{
    public class WorkstreamDbContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Constructor to pass options to the base DbContext
        public WorkstreamDbContext(DbContextOptions<WorkstreamDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tenant Configuration
            modelBuilder.Entity<Tenant>()
                .HasKey(t => t.TenantId); // Primary Key for Tenant

            // Tenant to User Configuration (1:N relationship)
            modelBuilder.Entity<Tenant>()
                .HasMany(t => t.Users)  // One Tenant can have many Users
                .WithOne()  // Each User is associated with one Tenant (no navigation on User side)
                .HasForeignKey(u => u.TenantId)  // Foreign Key in User table
                .OnDelete(DeleteBehavior.Restrict); // If a Tenant is deleted, all related Users are deleted

            modelBuilder.Entity<User>()
               .HasOne(u => u.Role)
               .WithMany(r => r.Users) //if you have a navigation property
               .HasForeignKey(u => u.RoleId)
               .OnDelete(DeleteBehavior.Restrict); // Ensure delete behavior is correct
            
            // Role Configuration
            modelBuilder.Entity<Role>()
                .HasKey(r => r.RoleId); // Primary Key for Role

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users) // A Role can have many Users
                .WithOne(u => u.Role) // Each User has one Role
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull); // Deleting a Role sets RoleId to null for related Users

            modelBuilder.Entity<Role>()
                .HasMany(r => r.RolePermissions) // A Role can have many RolePermissions
                .WithOne(rp => rp.Role) // Each RolePermission is associated with a Role
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade); // If a Role is deleted, delete all related RolePermissions

            // Tenant Configuration for Role
            modelBuilder.Entity<Role>()
                .HasOne(r => r.Tenant) // Each Role is associated with one Tenant
                .WithMany(t => t.Roles) // A Tenant can have many Roles
                .HasForeignKey(r => r.TenantId) // Foreign Key for TenantId in Role
                .OnDelete(DeleteBehavior.Restrict); // Deleting a Tenant deletes related Roles

            // Permission Configuration
            modelBuilder.Entity<Permission>()
                .HasKey(p => p.PermissionId); // Primary Key for Permission

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Name)
                .IsUnique(); // Make Name unique in the database

            // Since you removed the navigation property, don't configure relationships from Permission side

            // RolePermission Configuration (Many-to-Many relationship)
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId, rp.TenantId }); // Composite key for RolePermission with TenantId

            // Role -> RolePermission relationship 
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete on Role deletion, deleting all RolePermissions

            // Configure the Permission foreign key without navigation property
            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => rp.PermissionId); // Index the foreign key

            // Configure the cascade behavior using just the foreign key
            modelBuilder.Entity<RolePermission>()
                .Property(rp => rp.PermissionId)
                .IsRequired(); // The foreign key is required

            // InventoryItem Configuration
            modelBuilder.Entity<InventoryItem>()
                .HasKey(i => i.InventoryItemId); // Primary Key for InventoryItem

            modelBuilder.Entity<InventoryItem>()
                .HasMany(i => i.Stocks) // One InventoryItem can have many Stocks
                .WithOne() // No navigation property on Stock anymore, so just leave this empty
                .HasForeignKey(s => s.InventoryItemId) // Foreign Key in Stock to InventoryItem
                .OnDelete(DeleteBehavior.Cascade); // If an InventoryItem is deleted, all related Stocks are deleted


            // Customer Configuration
            modelBuilder.Entity<Customer>()
                .HasKey(c => c.CustomerId); // Primary Key for Customer
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders) // One Customer can have many Orders
                .WithOne(o => o.Customer) // Each Order is associated with one Customer
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // If a Customer is deleted, all related Orders are deleted

            // Order Configuration
            modelBuilder.Entity<Order>()
                .HasKey(o => o.OrderId); // Primary Key for Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer) // Each Order is associated with a Customer
                .WithMany(c => c.Orders) // One Customer can have many Orders
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // If a Customer is deleted, all related Orders are deleted

            // Configure Decimal Precision for financial data (e.g., Price in InventoryItem)
            modelBuilder.Entity<InventoryItem>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)"); // Precision: 18 digits, Scale: 2 digits after decimal

            // Configure Decimal Precision for Quantity in Stock (if necessary)
            modelBuilder.Entity<Stock>()
                .Property(s => s.Quantity)
                .HasColumnType("decimal(18,2)"); // Adjust this precision if needed

            // OrderItem Configuration (New entity to track items in an order)
            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.OrderItemId); // Primary Key for OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order) // Each OrderItem is associated with one Order
                .WithMany(o => o.OrderItems) // One Order can have many OrderItems
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // If an Order is deleted, delete all related OrderItems

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.InventoryItem)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(oi => oi.InventoryItemId)
                .OnDelete(DeleteBehavior.NoAction); // Don't modify OrderItem when InventoryItem is deleted

            // Price locking: ensure that price in OrderItem is fixed at the time of order
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)"); // Ensure precision for price in OrderItem
        }

    }
}
