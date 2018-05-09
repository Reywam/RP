using System;
using StackExchange.Redis;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.RegularExpressions;

namespace TextStatistics
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

        static float GetRankById(string id)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            int dbNum = CalculateHash(id);
            IDatabase db = redis.GetDatabase(dbNum);

            Console.WriteLine(id + " rank got from #" + dbNum);

            string value = null;
            value = db.StringGet("rank_" + id);
            return float.Parse(value);
        }
        
        static void UpdateStatistics(int textCount, float avgRank, int highRankPart, float ranksSum)
        {            
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            string statistics = textCount + ":" + avgRank + ":" + highRankPart + ":" + ranksSum;
            Console.WriteLine(statistics);
            for(int i = 0; i < 16; i++)
            {
                IDatabase db = redis.GetDatabase(i);
                db.StringSet("statistics", statistics);
            }
        }

        static void InitStartData(ref int textCount, ref int highRankPart, ref float avgRank, ref float ranksSum)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");                        
            IDatabase db = redis.GetDatabase();
            try
            {
                string msg = db.StringGet("statistics");
                var data = Regex.Split(msg, ":");

                textCount = int.Parse(data[0]);
                avgRank = float.Parse(data[1]);
                highRankPart = int.Parse(data[2]);            
                ranksSum = float.Parse(data[3]);
            }
            catch(Exception ex)
            {

            }
        }

        static void Main(string[] args)
        {
            int textCount = 0;
            int highRankPart = 0;
            float avgRank = 0;
            float ranksSum = 0;

            InitStartData(ref textCount, ref highRankPart, ref avgRank, ref ranksSum);

            Console.WriteLine("TextStatistics: {0} {1} {2}", textCount, highRankPart, avgRank);
            const string exchange = "text-success-marker";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchange, 
                                        type: "fanout");
                var queueName = "text-rank-calc";
                channel.QueueDeclare(queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.QueueBind(queue: queueName,
                                exchange: exchange,
                                routingKey: "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);                    
                    
                    var msgArgs = Regex.Split(message, ":");                    
                    if(msgArgs.Length == 3 && msgArgs[0] == "TextSuccessMarked")
                    {                                        
                        Console.WriteLine("Received: " + message);
                        textCount++;                      
                        if(msgArgs[2] == "true") 
                        {
                            highRankPart++;
                        }
                        float rank = GetRankById(msgArgs[1]);
                        ranksSum += rank;
                        avgRank = ranksSum / textCount;
                        UpdateStatistics(textCount, avgRank, highRankPart, ranksSum);
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