using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountModelsLib.Models
{
    [Table("Draws_List")] 
    public class Draws_List
    {
        public long Id { get; set; }
        public string Draw_Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
    public class DrawsReportDTO
    {
        public int Serial { get; set; }
        public long Id { get; set; }
        public string Draw_Name { get; set; }
        public decimal Price { get; set; }
    }
}