using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenentManagement.Common.Models;

namespace TenantManagement.DataAccessLibrary.Data
{
	public class ApplicationDbContext :DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		public DbSet<Bill> Bills { get; set; }
		public DbSet<Invoice> Invoices { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<Service> Services { get; set; }
		public DbSet<Tenant> Tenants { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Tenant>()
				.HasMany(t => t.Bills)
				.WithOne(b => b.Tenant)
				.HasForeignKey(b => b.TenantId);

			modelBuilder.Entity<Bill>()
				.HasMany(b => b.Payments)
				.WithOne(p => p.Bill)
				.HasForeignKey(p => p.BillId);

			modelBuilder.Entity<Bill>()
				.HasMany(b => b.Services)
				.WithOne(s => s.Bill)
				.HasForeignKey(s => s.BillId);

			modelBuilder.Entity<Bill>()
				.HasOne(b => b.Invoice)
				.WithOne(i => i.Bill)
				.HasForeignKey<Invoice>(i => i.BillId);
		}
	}
}
