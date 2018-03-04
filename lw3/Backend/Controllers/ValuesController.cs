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

namespace Backend.Controllers
{    
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {        
        // GET api/values/<id>
        [HttpGet("{id}")]
        public string Get(string id)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            IDatabase db = redis.GetDatabase();
                        
            string value = null;            
            value = db.StringGet(id);
            
            return value;
        }

        private void Send(string id) {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "backend-api",
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                string message = "Text created:" + id;
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                    routingKey: "backend-api",
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