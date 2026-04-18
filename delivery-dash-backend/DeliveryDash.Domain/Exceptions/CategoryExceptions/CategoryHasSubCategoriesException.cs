namespace DeliveryDash.Domain.Exceptions.CategoryExceptions
{
    public class CategoryHasSubCategoriesException : Exception
    {
        public CategoryHasSubCategoriesException(int categoryId) 
            : base($"Cannot delete category with ID {categoryId} because it has subcategories.")
        {
        }
    }
}