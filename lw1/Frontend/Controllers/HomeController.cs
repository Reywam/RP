using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;
using System.Net;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        private async Task<string> GetResponseString(string url, string data)
        {
            var httpClient = new HttpClient();
            StringContent content = new StringContent(data);

            var response = await httpClient.PostAsync(url, content);
            var contents = await response.Content.ReadAsStringAsync();

            return contents;
        }

        [HttpPost]
        public IActionResult Upload(string data)
        {            
            //HttpContent content = new StringContent(data);
            //TODO: send data in POST request to backend and read returned id value from response
            string url = "http://127.0.0.1:5000/api/values";
            string res = GetResponseString(url, data).Result;            
            return Ok(res);
        }
    }
}