using System;
using System.Collections.Generic;
using System.Text;

namespace Hangfire.Simplify.Entities
{
    public class ConfigureOptions
    {
        public string ConnnectionString { get; set; }
        public string MongoPrefix { get; set; }
        public List<string> HangfireServerName { get; set; }

        public ConfigureOptions()
        {
            this.ConnnectionString = "mongodb://localhost:27017/e-stock?readPreference=primary&appname=MongoDB%20Compass&ssl=false";
            this.MongoPrefix = "hangfire";
            this.HangfireServerName = new List<string>() { "E-Mail Worker" };
        }
    }
}
