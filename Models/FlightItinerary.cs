using System;
using System.Collections.Generic;
using System.Text;

namespace AeroSync.Models
{
    public class FlightItinerary
    {
        public string FlightNumber { get; set; } = "";
        public string DepartureTerminal { get; set; } = "";
        public string Gate { get; set; } = "";
        public string DepartureTime { get; set; } = "";
        public string Date { get; set; } = "";
        public string EmailHash { get; set; } = string.Empty;

    }
}
