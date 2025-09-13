using Microsoft.AspNetCore.Mvc;
using NaimaBeauty.Dtos;
using NaimaBeauty.Data; // your DbContext namespace
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NaimaBeauty.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminAnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminAnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("orders-overtime")]
        public async Task<ActionResult<List<OrdersOverTimeDto>>> GetOrdersOverTime([FromQuery] int? categoryId = null)
        {
            // Retrieve all orders with related items and categories
            var ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductCategories)
                            .ThenInclude(pc => pc.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                ordersQuery = ordersQuery
                    .Where(o => o.OrderItems
                        .Any(oi => oi.Product.ProductCategories
                            .Any(pc => pc.CategoryId == categoryId.Value)));
            }

            var orders = await ordersQuery.ToListAsync();

            // Flatten orders → order items → categories
            var ordersWithCategory = orders
                .SelectMany(o => o.OrderItems
                    .SelectMany(oi => oi.Product.ProductCategories
                        .Select(pc => new
                        {
                            o.OrderDate,
                            pc.CategoryId,
                            CategoryName = pc.Category.Name
                        })))
                .ToList();

            // Group by date and category
            var grouped = ordersWithCategory
                .GroupBy(x => new { x.OrderDate.Date, x.CategoryId, x.CategoryName })
                .Select(g => new OrdersOverTimeDto
                {
                    Date = g.Key.Date,
                    TotalOrders = g.Count(),
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName
                })
                .OrderBy(dto => dto.Date)
                .ToList();

            return Ok(grouped);
        }
        // GET: api/AdminAnalytics/revenue-overtime
        [HttpGet("revenue-overtime")]
        public async Task<ActionResult<List<RevenueOverTimeDto>>> GetRevenueOverTime([FromQuery] int? categoryId = null)
        {
            // Retrieve all orders with related items and categories
            var ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductCategories)
                            .ThenInclude(pc => pc.Category)
                .AsQueryable();


            if (categoryId.HasValue)
            {
                ordersQuery = ordersQuery
                    .Where(o => o.OrderItems
                        .Any(oi => oi.Product.ProductCategories
                            .Any(pc => pc.CategoryId == categoryId.Value)));
            }

            var orders = await ordersQuery.ToListAsync();

            // Flatten order items to include category info
            var orderItemsWithCategory = orders
                .SelectMany(o => o.OrderItems.SelectMany(oi => oi.Product.ProductCategories.Select(pc => new
                {
                    o.OrderDate,
                    pc.CategoryId,
                    CategoryName = pc.Category.Name,
                    LineTotal = oi.Quantity * oi.Product.Price
                })))
                .ToList();

            // Group by date and category
            var revenueByDate = orderItemsWithCategory
                .GroupBy(x => new { x.OrderDate.Date, x.CategoryId, x.CategoryName })
                .Select(g => new RevenueOverTimeDto
                {
                    Date = g.Key.Date,
                    TotalRevenue = g.Sum(x => x.LineTotal),
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName
                })
                .OrderBy(dto => dto.Date)
                .ToList();

            return Ok(revenueByDate);
        }


        // GET: api/AdminAnalytics/orders-by-category
        [HttpGet("orders-by-category")]
        public async Task<ActionResult<IEnumerable<OrdersByCategoryDto>>> GetOrdersByCategory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            // Query orders with related items, products, and categories
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductCategories)
                            .ThenInclude(pc => pc.Category)
                .AsQueryable();

            // Apply date filters
            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            // Flatten and group by category
            var ordersByCategory = await query
                .SelectMany(o => o.OrderItems.SelectMany(oi => oi.Product.ProductCategories.Select(pc => new
                {
                    pc.CategoryId,
                    CategoryName = pc.Category.Name,
                    Quantity = oi.Quantity
                })))
                .GroupBy(x => new { x.CategoryId, x.CategoryName })
                .Select(g => new OrdersByCategoryDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalQuantitySold = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            return Ok(ordersByCategory);
        }

        // GET: api/AdminAnalytics/aov-by-customer-category
        [HttpGet("aov-by-customer-category")]
        public async Task<ActionResult<List<CustomerAovByCategoryDto>>> GetAovByCustomerCategory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? categoryId = null)
        {
            // Step 1: Retrieve orders with related items and products
            var ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductCategories)
                            .ThenInclude(pc => pc.Category)
                .AsQueryable();

            // Step 2: Apply date range filter if provided
            if (startDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                ordersQuery = ordersQuery.Where(o => o.OrderDate <= endDate.Value);

            var orders = await ordersQuery.ToListAsync();

            // Step 3: Flatten orders → order items → categories
            var categoryOrders = orders
                .SelectMany(o => o.OrderItems.SelectMany(oi => oi.Product.ProductCategories.Select(pc => new
                {
                    pc.CategoryId,
                    CategoryName = pc.Category.Name,
                    OrderDate = o.OrderDate.Date,
                    LineTotal = oi.Quantity * oi.Product.Price
                })))
                .ToList();

            // Step 4: Optionally filter by category
            if (categoryId.HasValue)
                categoryOrders = categoryOrders.Where(x => x.CategoryId == categoryId.Value).ToList();

            // Step 5: Group by Category + Date and calculate AOV
            var result = categoryOrders
                .GroupBy(x => new { x.CategoryId, x.CategoryName, x.OrderDate })
                .Select(g => new CustomerAovByCategoryDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Date = g.Key.OrderDate,
                    AverageOrderValue = g.Average(x => x.LineTotal)
                })
                .OrderBy(x => x.Date)
                .ThenBy(x => x.CategoryName)
                .ToList();

            return Ok(result);
        }


    }


}
