using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;
using System.Text.RegularExpressions;

namespace TextListener
{
    class Program
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

        private static string GetValueById(string id) 
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            int dbNum = CalculateHash(id);
            Console.WriteLine("From db #" + dbNum);
            IDatabase db = redis.GetDatabase(dbNum);            

            string value = null;
            value = db.StringGet("text_" + id);            
            return value;
        }

        static void Main(string[] args)
        {
            const string exchange = "backend-api";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {  
                channel.ExchangeDeclare(exchange: exchange, type: "fanout");             

                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                                exchange: exchange,
                                routingKey: "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);                    
                    
                    var msgArgs = Regex.Split(message, ":");                    
                    if(msgArgs.Length == 2 && msgArgs[0] == "TextCreated")
                    {
                        string id = msgArgs[1];
                        string val = GetValueById(id);	
                        Console.WriteLine(id + " : " + val);
                    }
                };
                channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);

                Console.WriteLine("Listening messages.");
                Console.ReadLine();
            }
        }
    }
}