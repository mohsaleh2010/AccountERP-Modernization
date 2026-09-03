using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
	public class Financial_Period
	{
		public long Id { get; set; }
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
		public int Done { get; set; }
		public int FirstPeriod { get; set; }
		public int IS_Active { get; set; }
		public int IS_Finished_Posted { get; set; }
	}
}
