using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Backend.Controllers
{
    public class DataObject {
        public string data {get; set;}
    }

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        static readonly ConcurrentDictionary<string, string> _data = new ConcurrentDictionary<string, string>();

        // GET api/values/<id>
        [HttpGet("{id}")]
        public string Get(string id)
        {
            string value = null;
            _data.TryGetValue(id, out value);
            return value;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromForm] string value)
        {            
            if(value == null) {
                Console.WriteLine("Ti obosralsa");
            } else {
                Console.WriteLine(value);
            }
            var id = Guid.NewGuid().ToString();            
            _data[id] = value;
            return id;
        }
    }
}
