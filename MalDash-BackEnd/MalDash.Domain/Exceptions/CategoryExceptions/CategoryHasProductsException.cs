namespace MalDash.Domain.Exceptions.CategoryExceptions
{
    public class CategoryHasProductsException : Exception
    {
        public CategoryHasProductsException(int categoryId) 
            : base($"Cannot delete category with ID {categoryId} because it has products.")
        {
        }
    }
}