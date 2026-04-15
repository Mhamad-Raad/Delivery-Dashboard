namespace MalDash.Application.Abstracts.IService
{
    public interface INotificationHubService
    {
        Task SendNotificationToUserAsync(Guid userId, object notification);
    }
}