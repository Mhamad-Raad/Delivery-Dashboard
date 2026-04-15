using MalDash.Application.Responses.ApartmentResponses;

namespace MalDash.Application.Responses.FloorResponses
{
    public class FloorDetailResponse
    {
        public int Id { get; set; }
        public int FloorNumber { get; set; }
        public List<ApartmentDetailResponse?> Apartments { get; set; } = [];
    }
}
