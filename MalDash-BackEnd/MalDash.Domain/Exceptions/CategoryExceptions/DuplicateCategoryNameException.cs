namespace MalDash.Domain.Exceptions.CategoryExceptions
{
    public class DuplicateCategoryNameException : Exception
    {
        public DuplicateCategoryNameException(string categoryName) 
            : base($"A category with the name '{categoryName}' already exists.")
        {
        }
    }
}