using System.ComponentModel.DataAnnotations;

namespace ShopMvc.ViewModels
{
    // ViewModel dùng cho form Thêm / Sửa. Tất cả thuộc tính bắt buộc nhập (trừ Image khi sửa).
    public class ProductViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(150, MinimumLength = 6, ErrorMessage = "Tên sản phẩm phải từ 6 đến 150 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        // Ảnh hiện tại (dùng khi sửa)
        public string Image { get; set; }

        [Display(Name = "Hình ảnh")]
        public IFormFile ImageFile { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá chuẩn")]
        [Range(100000, float.MaxValue, ErrorMessage = "Giá chuẩn phải nhỏ nhất là 100000")]
        [Display(Name = "Giá chuẩn")]
        public float? Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá khuyến mãi")]
        [Range(0, float.MaxValue, ErrorMessage = "Giá khuyến mãi không được âm")]
        [Display(Name = "Giá khuyến mãi")]
        public float? SalePrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [StringLength(1500, ErrorMessage = "Mô tả không được vượt quá 1500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        private static readonly string[] BannedWords = { "die", "admin", "fack" };
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // SalePrice phải nhỏ hơn giá chuẩn 10% (tức SalePrice <= 90% Price)
            if (Price.HasValue && SalePrice.HasValue && SalePrice.Value > Price.Value * 0.9f)
            {
                yield return new ValidationResult(
                    "Giá khuyến mãi phải nhỏ hơn giá chuẩn ít nhất 10% (tối đa " + (Price.Value * 0.9f).ToString("0.##") + ")",
                    new[] { nameof(SalePrice) });
            }

            // Từ nhạy cảm trong Description
            if (!string.IsNullOrWhiteSpace(Description))
            {
                var lower = Description.ToLowerInvariant();
                var found = BannedWords.Where(w => lower.Contains(w)).ToList();
                if (found.Any())
                {
                    yield return new ValidationResult(
                        "Mô tả chứa từ nhạy cảm không được phép: " + string.Join(", ", found),
                        new[] { nameof(Description) });
                }
            }

            // Ảnh: bắt buộc chọn khi thêm mới (Id == 0) hoặc khi sửa mà chưa có ảnh
            if (ImageFile == null || ImageFile.Length == 0)
            {
                if (string.IsNullOrEmpty(Image))
                    yield return new ValidationResult("Vui lòng chọn hình ảnh sản phẩm", new[] { nameof(ImageFile) });
            }
            else
            {
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                    yield return new ValidationResult("Chỉ chấp nhận ảnh định dạng jpg, jpeg, png, gif, webp", new[] { nameof(ImageFile) });
                else if (ImageFile.Length > 5 * 1024 * 1024)
                    yield return new ValidationResult("Dung lượng ảnh không được vượt quá 5MB", new[] { nameof(ImageFile) });
            }
        }
    }
}
