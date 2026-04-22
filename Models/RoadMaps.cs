using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Financial_ForeCast.Models
{

    [Table("RoadMaps")]
    public class RoadMaps
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("RoadMapID")]
        public int Id { get; set; }
        [Column("RoadMapName")]
        public string RoadMapName { get; set; }
        [Column("RoadMapSavingAmount")]
        public double RoadMapSavingAmount { get; set; }
        [Column("RoadMapPrevSavingAmount")]
        public double RoadMapPrevSavingAmount { get; set; }
        [Column("NetWorth")]
        public double NetWorth { get; set; }
        [Column("PrevNetWorth")]
        public double PrevNetWorth { get; set; }
        public bool isSelected { get; set; } = false;
    }

}
