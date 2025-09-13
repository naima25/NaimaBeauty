using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaimaBeauty.Models;
using NaimaBeauty.Interfaces;
using NaimaBeauty.Services;

namespace NaimaBeauty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerService;
        private readonly ILogger<CustomerController> _logger;  // Injecting logger to log info, warning + error

        public CustomerController(ICustomerRepository customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        // GET: api/Customer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            try
            {
                _logger.LogInformation("Fetching all customers with orders.");
                var customers = await _customerService.GetAllAsync(); // Calls service with Include

                if (customers == null || !customers.Any())
                {
                    _logger.LogWarning("No customers found.");
                    return NotFound("No customers found.");
                }
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching customers.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // GET: api/Customer/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(string id)
        {
            try
            {
                _logger.LogInformation($"Fetching customer with ID {id} and their orders.");
                var customer = await _customerService.GetByIdAsync(id);  // Calls service with Include

                if (customer == null)
                {
                    _logger.LogWarning($"Customer with ID {id} not found.");
                    return NotFound($"Customer with ID {id} not found.");
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the customer.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // POST: api/Customer
        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            try
            {
                if (customer == null)
                {
                    _logger.LogWarning("Received empty customer object.");
                    return BadRequest("Customer data cannot be null.");
                }

                await _customerService.AddAsync(customer);
                _logger.LogInformation($"Customer with ID {customer.Id} created.");
                return CreatedAtAction("GetCustomer", new { id = customer.Id }, customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the customer.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // PUT: api/Customer/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(string id, Customer customer)  // Change int to string
        {
            try
            {
                if (id != customer.Id)
                {
                    _logger.LogWarning("Customer ID mismatch.");
                    return BadRequest("Customer ID mismatch.");
                }

                await _customerService.UpdateAsync(id, customer);
                _logger.LogInformation($"Customer with ID {id} updated.");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _customerService.GetByIdAsync(id) == null)
                {
                    _logger.LogWarning($"Customer with ID {id} not found for update.");
                    return NotFound($"Customer with ID {id} not found.");
                }
                else
                {
                    _logger.LogError("Error updating customer.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the customer.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // DELETE: api/Customer/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)  // Change int to string
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);
                if (customer == null)
                {
                    _logger.LogWarning($"Customer with ID {id} not found.");
                    return NotFound($"Customer with ID {id} not found.");
                }

                await _customerService.DeleteAsync(id);
                _logger.LogInformation($"Customer with ID {id} deleted.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the customer.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
