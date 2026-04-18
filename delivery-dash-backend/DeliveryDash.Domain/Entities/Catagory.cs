namespace DeliveryDash.Domain.Entities
{
    public class Catagory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        // Navigation properties
        public Catagory? ParentCategory { get; set; }
        public ICollection<Catagory> SubCategories { get; set; } = new List<Catagory>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
