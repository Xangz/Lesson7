using System.ComponentModel.DataAnnotations;

namespace ShopMvc.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Display(Name = "Hình ảnh")]
        public string Image { get; set; }

        [Display(Name = "Giá chuẩn")]
        public float Price { get; set; }

        [Display(Name = "Giá khuyến mãi")]
        public float SalePrice { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
