using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenentManagement.Common.Models
{
	public class Invoice
	{
		public int Id { get; set; }
		public int BillId { get; set; }
		public Bill Bill { get; set; }
		public DateTime InvoiceDate { get; set; }
		public decimal TotalAmount { get; set; }
		public string Status { get; set; } // e.g., Pending, Paid, Overdue
	}
}
