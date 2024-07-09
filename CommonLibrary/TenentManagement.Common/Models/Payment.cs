using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenentManagement.Common.Models
{
	public class Payment
	{
		public int Id { get; set; }
		public int BillId { get; set; }
		public Bill Bill { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public string PaymentMethod { get; set; } // e.g., Cash, Credit Card, Bank Transfer
	}
}
