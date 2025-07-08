using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NaimaBeauty.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; }

        [JsonIgnore]
        public Customer ? Customer { get; set; }

        public decimal Price { get; set; } 

        [Required]
        public DateTime OrderDate { get; set; }

         //[JsonIgnore]
        public List<OrderItem> ? OrderItems { get; set; } 
    }
}
