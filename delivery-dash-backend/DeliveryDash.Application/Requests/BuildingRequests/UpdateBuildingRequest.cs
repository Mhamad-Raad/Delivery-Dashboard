using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryDash.Application.Requests.BuildingRequests
{
    public record UpdateBuildingRequest
    {
        public required string Name { get; init; }
    }
}
