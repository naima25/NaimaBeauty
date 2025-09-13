using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NaimaBeauty.DTO;
using NaimaBeauty.Interfaces;
using NaimaBeauty.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaimaBeauty.Data;
using NaimaBeauty.Services;

namespace NaimaBeauty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly AppDbContext _context;

        // Updated constructor to inject BOTH services + context
        public ProductController(
            IProductRepository productService,
            ILogger<ProductController> logger,
            AppDbContext context
        )
        {
            _productService = productService;
            _logger = logger;
            _context = context;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            try
            {
                _logger.LogInformation("Fetching all products.");
                var products = await _productService.GetAllAsync();

                if (products == null || !products.Any())
                {
                    _logger.LogWarning("No products found.");
                    return NotFound("No products found.");
                }

                var productDTOs = products.Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    CategoryIds = p.ProductCategories?
                        .Select(pc => pc.CategoryId)
                        .Distinct()
                        .ToList() ?? new List<int>(),

                    CategoryNames = p.ProductCategories?
                        .Select(pc => pc.Category?.Name)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Distinct()
                        .ToList() ?? new List<string>()
                }).ToList();

                return Ok(productDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching products.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // GET: api/Product/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching product with ID {id}");
                var product = await _productService.GetByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found.");
                    return NotFound($"Product with ID {id} not found.");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the product.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // GET: api/Product/byCategory?categoryName=Electronics
        [HttpGet("byCategory")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory([FromQuery] string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return BadRequest("Category name is required.");
            }

            var products = await _context.Products
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Where(p => p.ProductCategories.Any(pc => pc.Category.Name == categoryName))
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound("No products found for the given category.");
            }

            return Ok(products);
        }

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    _logger.LogWarning("Received empty product object.");
                    return BadRequest("Product data cannot be null.");
                }

                await _productService.AddAsync(product);
                _logger.LogInformation($"Product with ID {product.Id} created.");
                return CreatedAtAction("GetProduct", new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the product.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // PUT: api/Product/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    _logger.LogWarning("Product ID mismatch.");
                    return BadRequest("Product ID mismatch.");
                }

                await _productService.UpdateAsync(id, product);
                _logger.LogInformation($"Product with ID {id} updated.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _productService.GetByIdAsync(id) == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found for update.");
                    return NotFound($"Product with ID {id} not found.");
                }
                else
                {
                    _logger.LogError("Error updating product.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the product.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // DELETE: api/Product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found.");
                    return NotFound($"Product with ID {id} not found.");
                }

                await _productService.DeleteAsync(id);
                _logger.LogInformation($"Product with ID {id} deleted.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the product.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
