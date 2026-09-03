using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    [NotMapped]
    public class BanksListViewModel : Bank
    {
        public BanksListViewModel() { }
        //public long Id { get; set; }
        public long OrderNo { get; set; }
        public string BankName { get; set; }
        public long BankId { get; set; }
        public decimal Cach_Value { get; set; }
        public decimal Factor { get; set; }
        public string Notes { get; set; }
        //public long SubAccount_Id { get; set; }
        //public int Currency_Id { get; set; }
    }
}
