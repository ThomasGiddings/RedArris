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

namespace RedArris.Services
{
    public class StockService
    {
        public StockService()
        {

        }

        public async Task<List<ReturnsValueModel>> GetReturnService(ReturnsRequestModel returnsModel)
        {
            if (returnsModel.toDate > DateTime.Now || returnsModel.fromDate < DateTime.Parse("1/1/2001") || returnsModel.toDate < returnsModel.fromDate)
            {
                return null;
            }

            var asDataModel = await GetData(returnsModel);

            List<ReturnsValueModel> values = asDataModel.Select(x =>
                    new ReturnsValueModel
                    {
                        Date = x.priceDate,
                        Return = x.close - x.open
                    }).ToList();

            return values;
        }

        public async Task<int> GetAlphaService(AlphaRequestModel alphaModel)
        {
            if (alphaModel.toDate > DateTime.Now || alphaModel.fromDate < DateTime.Parse("1/1/2001") || alphaModel.toDate < alphaModel.fromDate)
            {
                return -1;
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

            var returnsDelta = returnsL.close - returnsF.open;
            var benchDelta = benchL.close - benchF.open;
            var treasury = await GetTreasury(alphaModel);
            var treasuryDelta = treasury * returnsSymbol.Count();

            var alpha = returnsDelta - (treasury - (benchDelta - treasury) * GetBeta(returnsSymbol, returnsBenchmark));
            return 0;
        }

        public async Task<List<DataModel>> GetData(ReturnsRequestModel model) 
        {
            using (HttpClient client = new HttpClient())
            {
                StringBuilder baseUrl = new StringBuilder("https://cloud.iexapis.com/v1/data/CORE/HISTORICAL_PRICES/");
                baseUrl.Append(model.Symbol + "?");
                baseUrl.Append("token=" + "sk_32386b0065864deab6f19fcab9b2e10c");
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
                    return null;
                }
            }
        }

        public async Task<double> GetTreasury(AlphaRequestModel model) {
            using (HttpClient client = new HttpClient())
            {
                StringBuilder baseUrl = new StringBuilder("https://cloud.iexapis.com/v1/data/CORE/TREASURY/DGS30");
                baseUrl.Append("?token=" + "sk_32386b0065864deab6f19fcab9b2e10c&last=1");
                if (model.toDate.HasValue)
                {
                    baseUrl.Append($"&to={model.toDate.Value.ToString("yyyy-MM-dd")}");
                }

                var result = await client.GetAsync(baseUrl.ToString());

                if (result.IsSuccessStatusCode)
                {
                    string body = await result.Content.ReadAsStringAsync();

                    var asDataModel = JsonSerializer.Deserialize<List<TreasuryModel>>(body);

                    if (asDataModel.Count() > 0)
                    {
                        return asDataModel[0].value;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }

            }
        }

        public double GetBeta(List<DataModel> model1, List<DataModel> model2)
        {
            List<double> model1Figures = model1.Select(x => x.close).ToList();
            List<double> model2Figures = model2.Select(x => x.close).ToList();

            double model1Avg = model1Figures.Average();
            double model2Avg = model2Figures.Average();

            //double m1Var = Variance(model1Figures, model1Avg);
            double m2Var = Variance(model2Figures, model2Avg);

            double covariance = Covariance(model1Figures, model2Figures, model1Avg, model2Avg);

            return m2Var / covariance;
        }

        static double Variance(List<double> model, double average)
        {
            double result = 0;

            if (model.Count() > 0)
            {
                double sum = model.Sum(d => Math.Pow(d - average, 2));
                result = sum / model.Count();
            }

            return result;
        }

        static double Covariance(List<double> model1, List<double> model2, double avg1, double avg2)
        {
            double result = 0;
            double sum = 0;

            if (model1.Count() > 0 && model2.Count() > 0)
            {
                for (int i = 0; i < model1.Count(); i++) {
                    sum += (model1[i] - avg1) * (model2[i] - avg2);
                }
                result = sum / model1.Count();
            }

            return result;
        }
    }
}
