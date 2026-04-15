namespace MalDash.Domain.Exceptions.BuildingExceptions
{
    public class BuildingNotFoundException : Exception
    {
        public BuildingNotFoundException(string message) 
            : base(message)
        {
        }

        public BuildingNotFoundException(int id) 
            : base($"Building with ID {id} not found.")
        {
        }
    }
}
