using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
 
namespace NaimaBeauty.Models

{
    public class Product

    {
        public int Id { get; set; }
 
        [Required]

        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }
 
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }
        public bool Featured { get; set; } = false;
         public string ? ImageUrl { get; set; }

        public List<ProductCategory>? ProductCategories { get; set; }     // Many-to-many relationship with Category through ProductCategory

        public List<ProductCustomer>? ProductCustomers { get; set; }

    }

}
