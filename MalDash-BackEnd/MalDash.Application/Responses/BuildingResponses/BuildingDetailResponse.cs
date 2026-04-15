using MalDash.Application.Responses.FloorResponses;

namespace MalDash.Application.Responses.BuildingResponses
{
    public class BuildingDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<FloorDetailResponse> Floors { get; set; } = [];
    }
}
