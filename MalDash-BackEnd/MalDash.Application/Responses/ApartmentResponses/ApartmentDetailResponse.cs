using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MalDash.Application.Responses.ApartmentResponses
{
    public class ApartmentDetailResponse
    {
        public int Id { get; set; }
        public string ApartmentName { get; set; } = string.Empty;
        public OccupantResponse? Occupant { get; set; }
        public ApartmentLayoutResponse? Layout { get; set; }
    }
}
