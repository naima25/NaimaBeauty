using NaimaBeauty.DTO;
using NaimaBeauty.Models;
using NaimaBeauty.Data;
using NaimaBeauty.Services;
using Microsoft.AspNetCore.Mvc;

namespace NaimaBeauty.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly ProductCategoryService _service;

        public ProductCategoryController(ProductCategoryService service)
        {
            _service = service;
        }

        // GET: api/productcategory
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // POST: api/productcategory
        [HttpPost]
        public async Task<IActionResult> Add(ProductCategoryDto dto)
        {
            var pc = new ProductCategory { ProductId = dto.ProductId, CategoryId = dto.CategoryId };

            await _service.AddAsync(pc);
            return Ok();
        }

        // DELETE: api/productcategory/{productId}/{categoryId}
        [HttpDelete("{productId}/{categoryId}")]
        public async Task<IActionResult> Delete(int productId, int categoryId)
        {
            await _service.DeleteAsync(productId, categoryId);
            return NoContent();
        }
    }
}
