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
        static int CalculateHash(string value)
        {
            int hash = 0;

            for(int i = 0; i < value.Length; i++)
            {
                if(Char.IsLetter(value[i])) 
                {
                    hash++;
                }
            }

            return hash % 10;
        }

        private IActionResult GetRankFromDbById(string id)
        {            
            int tryCount = 5;
            int sleepTime = 500;
            
            id = id.Replace("text_", "");            

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            int dbNum = CalculateHash(id);
            IDatabase db = redis.GetDatabase(dbNum);

            Console.WriteLine(id + " rank got from #" + dbNum);

            string value = null;
            for(int i = 0; i < tryCount; i++)
            {   
                value = db.StringGet("rank_" + id);
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
            id = id.Replace("text_", "");            
            int dbNum = CalculateHash(id);
            Console.WriteLine(id + " get from #" + dbNum);
            return GetRankFromDbById("text_" + id);
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

            var id = Guid.NewGuid().ToString();                        
            var data = "text_" + id;    
            
            int dbNum = CalculateHash(id);
            IDatabase db = redis.GetDatabase(dbNum);

            db.StringSet(data, value);
            Console.WriteLine(data + " send to #" + dbNum);

            try {
                Send(id);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            return data;
        }
    }
}