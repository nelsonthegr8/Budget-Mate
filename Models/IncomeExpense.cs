using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Financial_ForeCast.Models
{
    [Table("IncomeExpense")]
    public class IncomeExpense
    {
        
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Column("RoadMapID")]
        public int RoadMapID { get; set; }

        [Column("Name")]
        public string Name { get; set; }
        [Column("Amount")]
        public double Amount { get; set; }
        [Column("Type")]
        public string Type { get; set; }
    }
}
