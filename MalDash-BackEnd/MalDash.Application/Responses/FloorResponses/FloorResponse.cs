using MalDash.Application.Responses.ApartmentResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalDash.Application.Responses.FloorResponses
{
    public class FloorResponse
    {
        public int Id { get; set; }
        public int FloorNumber { get; set; }
        public List<ApartmentResponse> Apartments { get; set; } = new();
    }
}
