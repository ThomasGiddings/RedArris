using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedArris.Services.Models
{
    public class DataModel
    {
        public double close { get; set; }
        public double fclose { get; set; }
        public double fhigh { get; set; }
        public double flow { get; set; }
        public double fopen { get; set; }
        public double fvolume { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double open { get; set; }
        public DateTime priceDate { get; set; }
        public string symbol { get; set; }
        public double uclose { get; set; }
        public double uhigh { get; set; }
        public double ulow { get; set; }
        public double uopen { get; set; }
        public double uvolume { get; set; }
        public double volume { get; set; }
        public string id { get; set; }
        public string key { get; set; }
        public string subkey { get; set; }
        public double date { get; set; }
        public double updated { get; set;  }
    }
}
