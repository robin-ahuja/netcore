using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OrgDAL;
using System.Text;
using System.Threading.Tasks;

namespace OrgAPI
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<OrganizationDbContext>();
            ////APPROACH 1: add your custom exception class
            //services.AddMvc(config => config.Filters.Add(new ExceptionBuilder()));

            //COOKIE BASED : it uses AuthorizeFilter when we use cookie based authorization
            //services.AddMvc(x => x.Filters.Add(new AuthorizeFilter())).AddXmlSerializerFormatters().AddXmlDataContractSerializerFormatters();

            //JWT TOKEN BASED: AuthorizeFilter not required for Jwt token based approach
            services.AddControllers().AddXmlSerializerFormatters().AddXmlDataContractSerializerFormatters();

            //while working with any client like angular, react, javascript, need to pass cors info like
            //cookie based authentication only
            //services.AddCors(x => x.AddPolicy("tms", p => 
            //p.AllowAnyHeader()
            //.AllowAnyMethod()
            //.WithOrigins("")
            //.AllowCredentials()));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<OrganizationDbContext>()
                .AddDefaultTokenProviders();

            //implmenet JWt token
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this-is-my-secret-key"));
            var tokenValidationParameter = new TokenValidationParameters()
            {
                IssuerSigningKey = signingKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                //ValidateLifetime=false,
                //ClockSkew=TimeSpan.Zero
            };

            ////APPROACH: JWT token based implementation
            services.AddAuthentication(x => x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(jwt =>
                    {
                        //jwt.SaveToken = true;
                        //jwt.RequireHttpsMetadata = true;
                        jwt.TokenValidationParameters = tokenValidationParameter;
                    }
                    );


            ////APPROACH: Cookie based authentication
            //services.ConfigureApplicationCookie(opt => {
            //    opt.Events = new CookieAuthenticationEvents
            //    {
            //        OnRedirectToLogin = redirectContext =>
            //        {
            //            redirectContext.HttpContext.Response.StatusCode = 401;
            //            return Task.CompletedTask;
            //        },
            //        OnRedirectToAccessDenied = redirectContext =>
            //        {
            //            redirectContext.HttpContext.Response.StatusCode = 401;
            //            return Task.CompletedTask;
            //        }
            //    };
            //});


            ////add Swagger in your application in .netcore 2.1
            //services.AddSwaggerDocument();

            ////add Swagger in your application in .netcore 2.1
            services.AddOpenApiDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseDeveloperExceptionPage();
            //app.UseSwagger(); //OLD way
            app.UseOpenApi(); //New way for swagger
            app.UseSwaggerUi3();

            //APPROACH 2: no need to add your custom exception class, just check in context if there are any exception do whatever you want to do
            app.UseExceptionHandler(options => {
                options.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();

                    if (ex != null)
                    {
                        await context.Response.WriteAsync(ex.Error.Message);
                    }
                });
            });
            //For cookie, cors, pass tms
           // app.UseCors("tms");
            app.UseAuthentication();

            ////in .net core 2.1 we are using app.UseMvc() for routing and endpoints
            //app.UseMvc();

            //// in .net core 3.1, we are defining differently end point and routing
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
