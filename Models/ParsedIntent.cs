using System;
using System.Collections.Generic;
using System.Text;

namespace AeroSync.Models
{
    public class ParsedIntent
    {
        public List<string> Categories { get; set; } = new();
        public string Duration { get; set; } = "";
    }

}
