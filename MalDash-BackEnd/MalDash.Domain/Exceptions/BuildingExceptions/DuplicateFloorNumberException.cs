namespace MalDash.Domain.Exceptions.BuildingExceptions
{
    public class DuplicateFloorNumberException : Exception
    {
        public DuplicateFloorNumberException(int floorNumber)
            : base($"Floor number {floorNumber} appears more than once in the building request.")
        {
        }
    }
}