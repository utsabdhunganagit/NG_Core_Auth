using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NG_Core_Auth.Data;
using NG_Core_Auth.Models;

namespace NG_Core_Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }


        [HttpGet("[action]")]
        [Authorize(Policy = "RequiredLoggedIn")]
        public IActionResult GetProduct()
        {
            return Ok(_db.Products.ToList());
        }
        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel formdata)
        {
            var newproduct = new ProductModel
            {
                Name = formdata.Name,
                ImageUrl = formdata.ImageUrl,
                Description = formdata.Description,
                OutOfStock = formdata.OutOfStock,
                Price = formdata.Price
            };
            await _db.Products.AddAsync(newproduct);
            await _db.SaveChangesAsync();
            return Ok(new JsonResult("The Product was added sucessfully"));
        }

        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] ProductModel formdata)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var findProduct = _db.Products.FirstOrDefault(p => p.ProductId == id);
            if (findProduct == null)
            {
                return NotFound();
            }
            else
            {
                findProduct.Name = formdata.Name;
                findProduct.ImageUrl = formdata.ImageUrl;
                findProduct.Description = formdata.Description;
                findProduct.OutOfStock = formdata.OutOfStock;
                findProduct.Price = formdata.Price;

                _db.Entry(findProduct).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return Ok(new JsonResult("The Product With Id"+ id +"Is Updated"));
            }
        }

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            //find the product
            var findProduct = await _db.Products.FindAsync(id);
            if (findProduct == null)
            {
                return NotFound();
            }
            else
            {
                _db.Products.Remove(findProduct);
                await _db.SaveChangesAsync();
                return Ok(new JsonResult("The Product with id" + id + "is deleted"));
            }
        }
    }
}