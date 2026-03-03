using Library.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace Library.MVC.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

        
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // 1 Customer -> Many Invoices
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1 Invoice -> Many Lines
        modelBuilder.Entity<InvoiceLine>()
            .HasOne(l => l.Invoice)
            .WithMany(i => i.Lines)
            .HasForeignKey(l => l.InvoiceId);

        // 1 Product -> Many Lines
        modelBuilder.Entity<InvoiceLine>()
            .HasOne(l => l.Product)
            .WithMany(p => p.InvoiceLines)
            .HasForeignKey(l => l.ProductId);

        // money columns: be explicit
        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<InvoiceLine>()
            .Property(l => l.UnitPrice)
            .HasPrecision(10, 2);
    }
}
