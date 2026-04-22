using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Financial_ForeCast.Models
{
    [Table("Accounts")]
    public class Accnts
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("RoadMapID")]
        public int RoadMapID { get; set; }

        [Column("AccountName")]
        public string AccountName { get; set; }
        [Column("Amount")]
        public double Amount { get; set; }
        [Column("Type")]
        public string Type { get; set; }
    }
}
