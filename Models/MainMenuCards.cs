using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Financial_ForeCast.Models
{
    [Table("MainMenuCards")]
    public class MainMenuCards
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Column("Card Name")]
        public string Name { get; set; }

        [Column("Link")]
        public string Link { get; set; }
        [Column("Amount")]
        public double Amount { get; set; }
        [Column("Passthrough")]
        public string Passthrough { get; set; }
    }
}
