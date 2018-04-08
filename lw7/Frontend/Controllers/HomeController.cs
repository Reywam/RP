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
                
        private async Task<string> Get(string url)
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


        public IActionResult TextDetails(string id)
        {                        
            string value = Get("http://127.0.0.1:5000/api/values/" + id).Result;            
            ViewData["Message"] = value;
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

        private async Task<string> Post(string url, string data)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            data = ("=" + data);            
            var content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await httpClient.PostAsync(url, content);
            var id = await response.Content.ReadAsStringAsync();

            return id;
        }                

        [HttpPost]
        public IActionResult Upload([FromForm] string data)
        {                                
            //TODO: send data in POST request to backend and read returned id value from response            
            string url = "http://127.0.0.1:5000/api/values";
            string res = "";
            if(data != null)
            {
                res = Post(url, data).Result;
            }
                        
            string newUrl = "http://127.0.0.1:5001/Home/TextDetails?=" + res;
            return new RedirectResult(newUrl);
        }
    }
}