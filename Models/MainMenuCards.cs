using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Financial_ForeCast.Models
{
    [Table("MainMenuCards")]
    public class MainMenuCards
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("CardName")]
        public string Name { get; set; }

        [Column("Link")]
        public string Link { get; set; }
        [Column("Amount")]
        public double Amount { get; set; }
        [Column("Passthrough")]
        public string Passthrough { get; set; }
    }
}
