using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Financial_ForeCast.Models
{
    public class Forecasts
    { 
        public string Month { get; set; }
        public int Year { get; set; }
        public double Income { get; set; }
        public double ExtraIncome { get; set; }
        public double Total { get; set; }
        public double cashStack { get; set; }
    }
}
