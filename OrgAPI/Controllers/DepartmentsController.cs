using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrgDAL;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace OrgAPI.Controllers
{
   // [Authorize(Roles = "Admin")] 
   [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : Controller
    {
        OrganizationDbContext context;
        //get logged in user from UserManager
        UserManager<IdentityUser> _userManager;
        public DepartmentsController(OrganizationDbContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            _userManager = userManager;
        }

        //SOLUTION1: Remove circular reference by using Lambda Expression
        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    //Solution1: Solving circular referencing problem
        //    var list = await context.Departments.Include(x =>x.Employees)
        //        .Select(z => new Department { 
        //        DId = z.DId,
        //        DName = z.DName,
        //        Description = z.Description,
        //        Employees = z.Employees.Select(y => new Employee {
        //            EId = y.EId,
        //            DId = y.DId,
        //            Name = y.Name
        //        })
        //        }).ToListAsync();
        //    if (list.Count == 0)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(list);
        //}

        //SOLUTION2: Using Netwon soft json convertor, remove circular references
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                //Solution1: Solving circular referencing problem
                var list = await context.Departments.Include(x => x.Employees).ToListAsync();
                if (list.Count == 0)
                {
                    return NotFound();
                }

                var jsonResult = JsonConvert.SerializeObject(list, Formatting.None,
                    new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.DId == id);
            if (dept == null)
            {
                return NotFound();
            }

            return Ok(dept);
        }
        [HttpGet("{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.DName == name);
            if (dept == null)
            {
                return NotFound();
            }

            return Ok(dept);
        }

        //[HttpGet("getByIdAndName/{id}/{name}")] //it will also work
        [HttpGet("getByIdAndName")]
        public async Task<IActionResult> GetByIdAndName(int id, string name)
        {
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.DId == id && x.DName == name);
            if (dept == null)
            {
                return NotFound();
            }

            return Ok(dept);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.DId == id);
            if (dept != null)
            {
                context.Remove(dept);
                context.SaveChanges();
                return Ok(dept);
            }
            else {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Department dept) {

            if (ModelState.IsValid)
            {
               
                ////get logged in user based on cookie based approach
                //var loggedInUser = await _userManager.FindByNameAsync(User.Identity.Name);
                //Get logged in user based on jwt token approach
                var listOfClaims = User.Claims;
                //get user id from list of claims
                dept.CreatedBy = listOfClaims.First().Value;
                context.Add(dept);
                await context.SaveChangesAsync();

                return CreatedAtAction("Get", new { id = dept.DId }, dept);
            } else {
                return BadRequest(ModelState);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(Department dept) {

            Department d = await context.Departments.AsNoTracking().FirstOrDefaultAsync(f => f.DId == dept.DId);

            if (d != null)
            {
                if (ModelState.IsValid)
                {
                    context.Update(dept);
                    await context.SaveChangesAsync();

                    return NoContent(); // when you don't' wanto send any content
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            else {
                return NotFound();
            }
        }
    }
}
