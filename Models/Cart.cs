using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NaimaBeauty.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }  

        [Required]
        public string CustomerId { get; set; }  

        // Navigation property for the related customer
        public Customer ? Customer { get; set; }  

        public List<CartItem> ? CartItems { get; set; } 

        public decimal Price { get; set; }  
    }
}
