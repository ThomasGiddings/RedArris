using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedArris.Services.Models
{
    public class ReturnsRequestModel
    {
        public string Symbol { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
    }
}
