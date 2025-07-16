using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NaimaBeauty.DTO
{
    public class ProductDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

         public string ImageUrl { get; set; }

        public List<int> CategoryIds { get; set; } = new();
        public List<string> CategoryNames { get; set; } = new();
    }
}
