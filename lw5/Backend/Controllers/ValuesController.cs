using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Configuration;
using StackExchange.Redis;
using RabbitMQ.Client;
using System.Text;
using System.Threading;

namespace Backend.Controllers
{    
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private IActionResult GetRankFromDbById(string id)
        {
            int tryCount = 5;
            int sleepTime = 500;
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            IDatabase db = redis.GetDatabase();

            string value = null;
            for(int i = 0; i < tryCount; i++)
            {   
                value = db.StringGet(id);
                if(value == null || !float.TryParse(value, out float f))
                {
                    Thread.Sleep(sleepTime);
                } 
                else 
                {
                    break;
                }                
            }   

            IActionResult result = null;
            if(value != null)          
            {
                result = Ok(value);
            } 
            else 
            {
                result = new NotFoundResult();
            }            
            
            return result;
        }

        // GET api/values/<id>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {                        
            return GetRankFromDbById(id);
        }

        private void Send(string id) {
            const string exchange = "backend-api";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange, "fanout");

                string message = "TextCreated:" + id;
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: exchange,
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);                
            }            
        }

        // POST api/values
        [HttpPost]
        public string Post([FromForm] string value)
        {               
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            IDatabase db = redis.GetDatabase();

            var id = Guid.NewGuid().ToString();            
            db.StringSet(id, value);
            try {
                Send(id);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            return id;            
        }
    }
}