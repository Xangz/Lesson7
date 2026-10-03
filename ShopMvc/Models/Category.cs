using System.ComponentModel.DataAnnotations;

namespace ShopMvc.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        public List<Product> Products { get; set; } = new();
    }
}
