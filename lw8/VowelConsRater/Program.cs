using System;
using StackExchange.Redis;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.RegularExpressions;

namespace VowelConsRater
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

        private static void SetRankInDbById(string id, float value) 
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");

            int dbNum = CalculateHash(id);
            IDatabase db = redis.GetDatabase(dbNum);

            Console.WriteLine(id + " rank send to db #" + dbNum);

            db.StringSet("rank_" + id, value);
        }
        
        private static float CalculateRank(string vowels, string cons)
        {
            float vowelsCount = float.Parse(vowels);
            float consCount = float.Parse(cons);
            float result = vowelsCount;
            if(consCount != 0)
            {
                result = vowelsCount / consCount;
            }
            return result;
        }

        private static void SendRankToExchange(string contextId
                                                , float rank
                                                , string exchangeName
                                                , IModel channel)
        {                    
            
            channel.ExchangeDeclare(exchange: exchangeName, 
                                        type: "fanout");            
            string message = "TextRankCalculated:" + contextId + ":" + rank;
            Console.WriteLine("SEND: " + message + " to exch" + exchangeName);
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchangeName, 
                                    routingKey: "",
                                    basicProperties: null,
                                    body: body);            
        }

        static void Main(string[] args)
        {            
            Console.WriteLine("VowelConsRater");
            const string exchange = "vowel-cons-counter";
            const string outputExchange = "text-rank-calc";
            const string key = "vowel-cons-task";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchange, 
                                        type: "direct");                
                var queueName = "rank-task";
                channel.QueueDeclare(queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.QueueBind(queue: queueName,
                                exchange: exchange,
                                routingKey: key);                            

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);                    

                    message = message.Replace("text_", "");
                    
                    var msgArgs = Regex.Split(message, ":");                    
                    if(msgArgs.Length == 4 && msgArgs[0] == "VowelConsCounted")
                    {                                        
                        VowelConsCounted data = new VowelConsCounted(msgArgs[1], msgArgs[2], msgArgs[3]);
                        float rank = CalculateRank(data.Vowels, data.Cons);
                        SetRankInDbById(data.Id, rank);                        
                        SendRankToExchange(data.Id, rank, outputExchange, channel);
                    }

                };
                channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);                
                Console.ReadLine();
            }
        }
    }
}