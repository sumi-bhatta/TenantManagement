using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenentManagement.Common.Models
{
	public class Service
	{
		public int Id { get; set; }
		public int BillId { get; set; }
		public Bill Bill { get; set; }
		public string ServiceName { get; set; } // e.g., Water, Electricity, Waste
		public decimal ServiceFee { get; set; }
	}
}
