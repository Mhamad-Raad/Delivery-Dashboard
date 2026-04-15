namespace MalDash.Application.Responses.DashboardResponses
{
    public class AdminDashboardResponse
    {
        public int TotalUsers { get; set; }
        public int ActiveVendors { get; set; }
        public int TotalBuildings { get; set; }
        public int TotalApartments { get; set; }
        public int OccupiedApartments { get; set; }
        public int TotalProducts { get; set; }
        public int PendingRequests { get; set; }
    }
}