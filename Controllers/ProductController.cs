using Microsoft.AspNetCore.Mvc;
using Pharmacy_API.Dtos.Product;
using Pharmacy_API.Models.Product;
using Pharmacy_API.Services.Product;

namespace Pharmacy_API.Controllers
{
    [Route("api/Account/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetListProductAsync([FromQuery] ProductFilterDto filter)
        {
            var result = await _productService.GetListProductsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductRequestDto dto)
        {
            var result = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ProductId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductRequestDto dto)
        {
            var result = await _productService.UpdateProductAsync(id, dto);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            return success ? NoContent() : NotFound();
        }

        [HttpGet("alias/{nameAlias}")]
        public async Task<IActionResult> GetByAlias(string nameAlias)
        {
            var result = await _productService.GetProductByAliasAsync(nameAlias);
            return result == null ? NotFound() : Ok(result);
        }
    }
}