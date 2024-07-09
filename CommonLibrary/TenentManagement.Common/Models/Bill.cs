using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenentManagement.Common.Models
{
	public class Bill
	{
		public int Id { get; set; }
		public int TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public decimal MonthlyFee { get; set; }
		public decimal Water { get; set; }
		public decimal Electricity { get; set; }
		public decimal Waste { get; set; }
		public DateTime DueDate { get; set; }
		public bool IsPaid { get; set; }
		public Invoice Invoice { get; set; } // This should exist if Bill has an Invoice
		public List<Payment> Payments { get; set; }
		public List<Service> Services { get; set; }
	}
}
