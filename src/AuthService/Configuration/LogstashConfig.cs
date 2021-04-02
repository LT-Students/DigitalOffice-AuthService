using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthService.Configuration
{
    public class LogstashConfig
    {
        public const string LogstashSectionName = "Logstash";

        public string KeyProperty { get; set; }
        public string Url { get; set; }
    }
}
