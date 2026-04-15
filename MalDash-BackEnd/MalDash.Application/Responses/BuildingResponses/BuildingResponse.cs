using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalDash.Application.Responses.BuildingResponses
{
    public class BuildingResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberOfFloors { get; set; }
        public int TotalApartments { get; set; }
        public int Occupants { get; set; }
    }
}
