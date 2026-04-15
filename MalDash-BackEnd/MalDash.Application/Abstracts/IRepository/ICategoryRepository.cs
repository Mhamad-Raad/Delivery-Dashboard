using MalDash.Domain.Entities;

namespace MalDash.Application.Abstracts.IRepository
{
    public interface ICategoryRepository
    {
        Task<Catagory?> GetByIdAsync(int id);
        Task<Catagory?> GetByNameAsync(string name);
        Task<IEnumerable<Catagory>> GetAllCategoriesAsync();
        Task<IEnumerable<Catagory>> GetTopLevelCategoriesAsync();
        Task<IEnumerable<Catagory>> GetSubCategoriesAsync(int parentCategoryId);
        Task<(IEnumerable<Catagory> Categories, int Total)> GetCategoriesPagedAsync(
            int page,
            int limit,
            string? searchName = null,
            int? parentCategoryId = null);
        Task<Catagory> CreateAsync(Catagory category);
        Task<Catagory> UpdateAsync(Catagory category);
        Task DeleteAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> HasSubCategoriesAsync(int categoryId);
        Task<bool> HasProductsAsync(int categoryId);
    }
}