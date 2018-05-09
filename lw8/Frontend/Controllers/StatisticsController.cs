using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Frontend.Controllers
{
    public class StatisticsController : Controller
    {
        public IActionResult Statistics()
        {
            var value = GetStatistics("http://127.0.0.1:5000/api/values/statistics").Result;
            var data = Regex.Split(value, ":");
            string msg = "Text count: " + data[0] + " Avg. rank: " + data[1] + " High rank parts: " + data[2];
            ViewData["Message"] = msg;
            return View();
        }        

        private async Task<string> GetStatistics(string url)
        {            
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));                        

            var response = await httpClient.GetAsync(url);            
            string value = "";
            if (response.IsSuccessStatusCode)
            {
                value = await response.Content.ReadAsStringAsync();
            }
            else
            {
                value = response.StatusCode.ToString();
            }            

            return value;
        }
    }
}