using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Financial_ForeCast.Models
{
    
    [Table("RoadMaps")]
    public class RoadMaps
    {

        [PrimaryKey, AutoIncrement, Column("RoadMapID")]
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
