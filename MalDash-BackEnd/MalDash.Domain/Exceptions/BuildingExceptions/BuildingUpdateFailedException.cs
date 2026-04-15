namespace MalDash.Domain.Exceptions.BuildingExceptions
{
    public class BuildingUpdateFailedException : Exception
    {
        public BuildingUpdateFailedException(string? message)
            : base(message)
        {
        }

        public BuildingUpdateFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
