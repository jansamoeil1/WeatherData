using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program.WeatherDataMap;

namespace EFCore.Models
{

    public class WeatherData
    {
        public int Id { get; set; }  // Primärnyckel
        public DateTime Date { get; set; }
        public string Place { get; set; }
        public double? Temperature { get; set; }
        public double? Humidity { get; set; }
        public double? MoldRisk { get; set; }
        public string Location { get; set; }
    }
}


