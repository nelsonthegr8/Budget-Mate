using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Financial_ForeCast.Models
{
    [Table("Forecasts")]
    public class Forecasts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("ForcastName")]
        public string ForcastName { get; set; }
        [Column("Month")]
        public string Month { get; set; }
        [Column("Year")]
        public int Year { get; set; }
        [Column("Income")]
        public double Income { get; set; }
        [Column("ExtraIncome")]
        public double ExtraIncome { get; set; }
        [Column("Total")]
        public double Total { get; set; }
        [Column("cashStack")]
        public double cashStack { get; set; }
    }
}
