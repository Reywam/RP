using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
namespace TextRankCalc
{
    class Program
    {        
        private static void InitializeVowels(HashSet<char> vowels) 
        {            
            vowels.Add('a');
            vowels.Add('e');
            vowels.Add('i');
            vowels.Add('o');
            vowels.Add('u');
            vowels.Add('y');
        }

        private static void InitializeConsonants(HashSet<char> consonants) 
        {
            consonants.Add('b'); consonants.Add('c');consonants.Add('d');
            consonants.Add('f'); consonants.Add('g');consonants.Add('h');
            consonants.Add('j'); consonants.Add('k');consonants.Add('l');
            consonants.Add('m'); consonants.Add('n');consonants.Add('p');
            consonants.Add('q'); consonants.Add('r');consonants.Add('s');
            consonants.Add('t'); consonants.Add('v');consonants.Add('w');
            consonants.Add('x');consonants.Add('z');
        }

        private static string GetValueById(string id) 
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            IDatabase db = redis.GetDatabase();
                        
            string value = null;
            value = db.StringGet(id);
            return value;
        }

        private static void SetRankInDbById(string id, float value) 
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost, abortConnect=false");
            IDatabase db = redis.GetDatabase();
                                    
            db.StringSet(id, value);            
        }

        private static float CalculateRank(string text, HashSet<char> vowels, HashSet<char> consonants)
        {
            float vowelsCount = 0;
            float consonantsCount = 0;
            for(int i = 0; i < text.Length; i++)
            {
                if(vowels.Contains(text[i]))
                {
                    vowelsCount++;
                }
                else 
                {
                    consonantsCount++;
                }
            }
            float result = vowelsCount;
            if(consonantsCount != 0)
            {
                result = vowelsCount / consonantsCount;
            }
            return result;
        }

        static void Main(string[] args)
        {
            HashSet<char> vowels = new HashSet<char>();
            HashSet<char> consonants = new HashSet<char>();
            InitializeConsonants(consonants);
            InitializeVowels(vowels);

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {   
                while(true)
                {             
                    channel.QueueDeclare(queue: "backend-api",
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);
                
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);                        
                        
                        string id = Regex.Split(message, ":")[1];
                        string text = GetValueById(id);

                        var rank = CalculateRank(text, vowels, consonants);
                        SetRankInDbById(id, rank);
                        text = GetValueById(id);
                        Console.WriteLine(id + " : " + text);
                    };
                    channel.BasicConsume(queue: "backend-api",
                                        autoAck: true,
                                        consumer: consumer);                            
                }
            }
        }
    }
}