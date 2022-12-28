using Microsoft.AspNetCore.Mvc;
using RedArris.Services.Models;
using RedArris.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedArris.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : Controller
    {
        [HttpPost("getReturn")]
        public async Task<IActionResult> GetReturns(ReturnsRequestModel model)
        {
            StockService service = new StockService();
            return Ok(await service.GetReturnService(model));
        }


        [HttpPost("getAlpha")]
        public async Task<IActionResult> GetAlpha(AlphaRequestModel model)
        {
            StockService service = new StockService();
            return Ok(await service.GetAlphaService(model));
        }
    }
}
