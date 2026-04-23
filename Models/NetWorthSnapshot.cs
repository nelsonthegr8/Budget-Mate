using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Financial_ForeCast.Models
{
    [Table("NetWorthSnapshots")]
    public class NetWorthSnapshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("RecordedDate")]
        public DateTime RecordedDate { get; set; }

        [Column("Amount")]
        public double Amount { get; set; }
    }
}
