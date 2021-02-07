# netcore
Understanding implementation of JWT as well as cookie based Authentication &amp; Authorization
JWT (JSON web token) has become more and more popular in web development. It is an open standard which allows transmitting data between parties as a JSON object in a secure and compact way. The data transmitting using JWT between parties are digitally signed so that it can be easily verified and trusted.
 
In this article, we will learn how to setup JWT with ASP.NET core web application. We can create an application using Visual Studio or using CLI (Command Line Interface).
The first step is to configure JWT based authentication in our project. To do this, we need to register a JWT authentication schema by using "AddAuthentication" method and specifying JwtBearerDefaults.AuthenticationScheme. Here, we configure the authentication schema with JWT bearer options.
public void ConfigureServices(IServiceCollection services)    
{    
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)    
    .AddJwtBearer(options =>    
    {    
        options.TokenValidationParameters = new TokenValidationParameters    
        {    
            ValidateIssuer = true,    
            ValidateAudience = true,    
            ValidateLifetime = true,    
            ValidateIssuerSigningKey = true,    
            ValidIssuer = Configuration["Jwt:Issuer"],    
            ValidAudience = Configuration["Jwt:Issuer"],    
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))    
        };    
    });    
    services.AddMvc();    
}   
In this example, we have specified which parameters must be taken into account to consider JWT as valid. As per our code,  the following items consider a token valid:
Validate the server (ValidateIssuer = true) that generates the token.
Validate the recipient of the token is authorized to receive (ValidateAudience = true)
Check if the token is not expired and the signing key of the issuer is valid (ValidateLifetime = true)
Validate signature of the token (ValidateIssuerSigningKey = true)
Additionally, we specify the values for the issuer, audience, signing key. In this example, I have stored these values in appsettings.json file.
AppSetting.Json
{    
  "Jwt": {    
    "Key": "ThisismySecretKey",    
    "Issuer": "Test.com"    
  }    
}   
The above-mentioned steps are used to configure a JWT based authentication service. The next step is to make the authentication service is available to the application. To do this, we need to call app.UseAuthentication() method in the Configure method of startup class. The UseAuthentication method is called before UseMvc method.
public void Configure(IApplicationBuilder app, IHostingEnvironment env)    
{    
    app.UseAuthentication();    
    app.UseMvc();    
} 
Generate JSON Web Token
 
I have created a LoginController and Login method within this controller, which is responsible to generate the JWT. I have marked this method with the AllowAnonymous attribute to bypass the authentication. This method expects the Usermodel object for Username and Password.
 
I have created the "AuthenticateUser" method, which is responsible to validate the user credential and returns to the UserModel. For demo purposes, I have returned the hardcode model if the username is "Jignesh". If the "AuthenticateUser" method returns the user model, API generates the new token by using the "GenerateJSONWebToken" method.
 
Here, I have created a JWT using the JwtSecurityToken class. I have created an object of this class by passing some parameters to the constructor such as issuer, audience, expiration, and signature.
 
Finally, JwtSecurityTokenHandler.WriteToken method is used to generate the JWT. This method expects an object of the JwtSecurityToken class.
using Microsoft.AspNetCore.Authorization;    
using Microsoft.AspNetCore.Mvc;    
using Microsoft.Extensions.Configuration;    
using Microsoft.IdentityModel.Tokens;    
using System;    
using System.IdentityModel.Tokens.Jwt;    
using System.Security.Claims;    
using System.Text;    
    
namespace JWTAuthentication.Controllers    
{    
    [Route("api/[controller]")]    
    [ApiController]    
    public class LoginController : Controller    
    {    
        private IConfiguration _config;    
    
        public LoginController(IConfiguration config)    
        {    
            _config = config;    
        }    
        [AllowAnonymous]    
        [HttpPost]    
        public IActionResult Login([FromBody]UserModel login)    
        {    
            IActionResult response = Unauthorized();    
            var user = AuthenticateUser(login);    
    
            if (user != null)    
            {    
                var tokenString = GenerateJSONWebToken(user);    
                response = Ok(new { token = tokenString });    
            }    
    
            return response;    
        }    
    
        private string GenerateJSONWebToken(UserModel userInfo)    
        {    
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));    
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);    
    
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],    
              _config["Jwt:Issuer"],    
              null,    
              expires: DateTime.Now.AddMinutes(120),    
              signingCredentials: credentials);    
    
            return new JwtSecurityTokenHandler().WriteToken(token);    
        }    
    
        private UserModel AuthenticateUser(UserModel login)    
        {    
            UserModel user = null;    
    
            //Validate the User Credentials    
            //Demo Purpose, I have Passed HardCoded User Information    
            if (login.Username == "Jignesh")    
            {    
                user = new UserModel { Username = "Jignesh Trivedi", EmailAddress = "test.btest@gmail.com" };    
            }    
            return user;    
        }    
    }    
}   
Once, we have enabled the JWT based authentication, I have created a simple Web API method that returns a list of value strings when invoked with an HTTP GET request. Here, I have marked this method with the authorize attribute, so that this endpoint will trigger the validation check of the token passed with an HTTP request.
 
If we call this method without a token, we will get 401 (UnAuthorizedAccess) HTTP status code as a response. If we want to bypass the authentication for any method, we can mark that method with the AllowAnonymous attribute.
 
To test the created Web API, I am Using Fiddler. First, I have requested to "API/login" method to generate the token. I have passed the following JSON in the request body.
{"username": "Jignesh", "password": "password"}  
