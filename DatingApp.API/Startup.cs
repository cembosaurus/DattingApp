using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }



        // This method gets called by the runtime. Use this method to add services to the container.
        // ************************************************************************************************


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(d => d.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(opt => {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddCors();
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            services.AddAutoMapper();
            services.AddTransient<Seed>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IDatingRepository, DatingRepository>();
            services.AddScoped<LogUserActivity>();
            
            //............................................. JWT ..........................................
            // ... add authentication as a service:
            // ... specifying authentication scheme ...
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // ... get my SECRET string from 'APPSETTINGS.JSON' and convert it into ByteArray ...
                    var JWT_secret = Configuration.GetSection("Cembo_Settings:Token").Value;
                    var JWT_byteArray = Encoding.ASCII.GetBytes(JWT_secret);

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // ... options server wants to validate against ...
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(JWT_byteArray), // ... KEY from my configuration ...
                        ValidateIssuer = false,
                        ValidateAudience = false

                    };
                });
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ************************************************************************************************


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
        {
            //// ... for testing purpose, adding extra header into request ....
            //app.Use(async (context, next) =>
            //{
            //    context.Request.Headers.Add("Test Key", "Request - Test Value");
            //    await next();
            //    context.Response.Headers.Add("Test Key", "Response - Test Value");
            //    context.Response.Headers["Test Key"] = "Modified Respoinse Value in pipeline";
            //});

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //... intercepting EXCEPTION ...
                app.UseExceptionHandler(builder =>
                {

                    //... CREATING new response to user on different PIPELINE ...
                    builder.Run(async context =>
                    {

                        //... grabbing SPECIFIC '500' exception from pipeline and passing it into HttpContext collection ...
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                        //... retrieving exception data ...
                        var err = context.Features.Get<IExceptionHandlerFeature>();

                        if (err != null)
                        {
                            //... EITHER writing exception into response header ...
                            //context.Response.Headers.Add("Shit-Happens", "this SHIT is getting on my  nerves !");
                            //context.Response.Headers.Add("Access-Control-Expose-Headers", "Shit-Happens");
                            //context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                            //... OR ...
                            //... using my extension method ...
                            context.Response.AddExceptionIntoResponseHeader(err.Error.Message);

                            //... writting into new response and sending EX MESSAGE to user, or my custom message f.e.: "chodte do zadku !" ...
                            await context.Response.WriteAsync(err.Error.Message);
                        }
                    });

                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    //... UseHsts() is to force SSL ...
                    //app.UseHsts();
                });

            }

            //... one time seeding users, then comment it out
            //seeder.SeedUsers();
            //app.UseHttpsRedirection();
            app.UseCors(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            // ... my authentication inserted into HTTP request pipeline. Order matters ...
            app.UseAuthentication();

            // ... to serve static files from my 'wwwroot'
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc( routes => {
                routes.MapSpaFallbackRoute( name: "spa-fallback", defaults: new { controller = "Fallback", action = "Index"});
            });

        }
    }
}

