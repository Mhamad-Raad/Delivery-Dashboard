using DeliveryDash.Application.Responses.FloorResponses;

namespace DeliveryDash.Application.Responses.BuildingResponses
{
    public class BuildingDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<FloorDetailResponse> Floors { get; set; } = [];
    }
}
