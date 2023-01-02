using RedArris.Services.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace RedArris.Services
{
    public class StockService
    {
        public string Token { get; set; }
        public StockService()
        {
            Token = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json")
                 .AddUserSecrets<StockService>()
                 .Build().GetSection("Token").Value;
        }

        public async Task<List<ReturnsValueModel>> GetReturnService(ReturnsRequestModel returnsModel)
        {
            if (returnsModel.toDate > DateTime.Now || returnsModel.fromDate < DateTime.Parse("1/1/2001") || returnsModel.toDate < returnsModel.fromDate)
            {
                throw new Exception("Invalid date arguments.");
            }

            var asDataModel = await GetData(returnsModel);

            List<ReturnsValueModel> values = asDataModel.Select(x =>
                    new ReturnsValueModel
                    {
                        Date = x.priceDate,
                        Return = (x.close - x.open) / x.open
                    }).ToList();

            return values;
        }

        public async Task<double> GetAlphaService(AlphaRequestModel alphaModel)
        {
            if (alphaModel.toDate > DateTime.Now || alphaModel.fromDate < DateTime.Parse("1/1/2001") || alphaModel.toDate < alphaModel.fromDate)
            {
                throw new Exception("Invalid date arguments.");
            }

            var returnsSymbol = await GetData(new ReturnsRequestModel
            {
                toDate = alphaModel.toDate,
                fromDate = alphaModel.fromDate,
                Symbol = alphaModel.Symbol,
            });

            var returnsBenchmark = await GetData(new ReturnsRequestModel
            {
                toDate = alphaModel.toDate,
                fromDate = alphaModel.fromDate,
                Symbol = alphaModel.Benchmark,
            });

            var returnsF = returnsSymbol.FirstOrDefault();
            var returnsL = returnsSymbol.LastOrDefault();
            var benchF = returnsBenchmark.FirstOrDefault();
            var benchL = returnsBenchmark.LastOrDefault();

            if (returnsF == null || returnsL == null || benchF == null || benchL == null) {
                throw new Exception("Error retrieving values from source, please try again later.");
            }

            var returnsDelta = (returnsL.close - returnsF.open) / returnsF.open;
            var benchDelta = (benchL.close - benchF.open) / benchF.open;

            return returnsDelta - benchDelta;
        }

        public async Task<List<DataModel>> GetData(ReturnsRequestModel model) 
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    StringBuilder baseUrl = new StringBuilder("https://cloud.iexapis.com/v1/data/CORE/HISTORICAL_PRICES/");
                    baseUrl.Append(model.Symbol + "?");
                    baseUrl.Append("token=" + this.Token);

                    if (model.toDate == null && model.fromDate == null)
                    {
                        baseUrl.Append("&range=ytd");
                    }
                    else
                    {
                        if (model.fromDate != null)
                        {

                            baseUrl.Append($"&from={model.fromDate.Value.ToString("yyyy-MM-dd")}");
                        }
                        if (model.toDate != null)
                        {
                            baseUrl.Append($"&to={model.toDate.Value.ToString("yyyy-MM-dd")}");
                        }
                    }

                    var result = await client.GetAsync(baseUrl.ToString());

                    if (result.IsSuccessStatusCode)
                    {
                        string body = await result.Content.ReadAsStringAsync();

                        var asDataModel = JsonSerializer.Deserialize<List<DataModel>>(body);

                        return asDataModel;
                    }
                    else
                    {
                        throw new Exception("Request to IEX failed, please try again later.");
                    }
                }
            }
            catch {
                throw new Exception("There was an error retrieving data from IEX, please try again later.");
            }
        }
    }
}
