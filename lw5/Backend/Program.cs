using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using StackExchange.Redis;

namespace Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }
        
        public static IWebHost BuildWebHost(string[] args){        
            var rootFolder = Directory.GetParent(Directory.GetCurrentDirectory());
            var dirs = Directory.GetDirectories(rootFolder.FullName, "config");                        
            DirectoryInfo configDir = new DirectoryInfo(dirs[0]);

            var config = new ConfigurationBuilder()            
            .SetBasePath(configDir.FullName)
            .AddJsonFile("backend_config.json", optional: true)
            .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())                
                .UseStartup<Startup>()
                .Build();

            return host;                        
        }                          
    }
}