using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OrgAPI.ViewModel;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OrgAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        //required for signin and signout
        SignInManager<IdentityUser> signInManager;

        //user manager internal class for creating/updating users
        UserManager<IdentityUser> userManager;

        public AccountController(SignInManager<IdentityUser> _signInManager, UserManager<IdentityUser> _userManager) {
            signInManager = _signInManager;
            userManager = _userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model) {
            if (ModelState.IsValid) {
                var user = new IdentityUser()
                {
                     UserName = model.UserName,
                     Email = model.Email
                };

                var userResult = await userManager.CreateAsync(user, model.Password);

                if (userResult.Succeeded)
                {
                    var roleResult = await userManager.AddToRoleAsync(user, "User");
                    if (roleResult.Succeeded)
                    {
                        return Ok(user);
                    }
                }
                else {
                    foreach (var error in userResult.Errors) {
                        ModelState.AddModelError("", error.Description);
                    }
                    
                }

            }

            return BadRequest(ModelState.Values);
        }

        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn(SignInViewModel model) {
            if (ModelState.IsValid) {
                var userResult = await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

                if (userResult.Succeeded)
                { //when we logged in, we have to generate the claim also for that user that information that you want to access
                  //eg: userid, username etc.
                    var user = await userManager.FindByNameAsync(model.UserName);
                    var roles = await userManager.GetRolesAsync(user);
                    IdentityOptions options = new IdentityOptions();

                    var claims = new Claim[] {
                     //new Claim("userId", user.Id), //similar to below statement
                     new Claim(options.ClaimsIdentity.UserIdClaimType, user.Id),
                     new Claim(options.ClaimsIdentity.UserNameClaimType, user.UserName),
                     new Claim(options.ClaimsIdentity.RoleClaimType, roles[0]) //add roles also in claim, after adding roles, you can add attribute on controller class
                    };

                    //based on cookie based approach we were returning cookie in header
                    //return Ok();
                    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-my-secret-key"));

                    //now generate signin signature
                    var signInCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

                    //in jwt token, after successful login we have to generate jwt token
                    var token = new JwtSecurityToken(signingCredentials: signInCredentials, expires: DateTime.Now.AddMinutes(30),
                        claims: claims);
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPost("signout")]
        public async Task<IActionResult> SignOut() {
            await signInManager.SignOutAsync();

            return NoContent();
        }
    }
}
