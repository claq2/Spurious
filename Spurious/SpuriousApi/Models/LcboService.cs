using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SpuriousApi.Models
{
    
    public class LcboService
    {
        public async Task<string> GetLcboStores()
        {
            var token = ConfigurationManager.AppSettings["LcboToken"];
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", String.Format("Token {0}", token));
            var resp = await httpClient.GetAsync("https://lcboapi.com/stores");
            if (resp.IsSuccessStatusCode)
            {
                var y = resp.Content;
                return await y.ReadAsStringAsync();
            }
            else
            {
                return "blah";
            }
        }
    }
}