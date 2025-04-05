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
        [Column("CardName")]
        public string CardName { get; set; }

        [Column("MenuLink")]
        public string MenuLink { get; set; }
        [Column("TotalAmount")]
        public double TotalAmount { get; set; }
    }
}
