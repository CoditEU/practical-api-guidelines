﻿using Codit.LevelOne.DB;
using Codit.LevelOne.Entities;
using Codit.LevelOne.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace Codit
{
    public class Startup
    {
        public static IConfiguration Configuration { get; set; }
      public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();



            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Startup.Configuration.GetConnectionString("WorldCupDB");

            //default scope lifetime
#if DEBUG

            services.AddDbContext<WorldCupContext>(opt => opt.UseInMemoryDatabase("WorldCupDB"));

#else
            services.AddDbContext<WorldCupContext>(o => o.UseSqlServer(connectionString)); 
#endif
            services.AddScoped<IWorldCupRepository, WorldCupRepository>(); //scoped

            // Versioning
            services.AddApiVersioning();

            services
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    //explicit date configuration
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.DateFormatString = "o";
                });

            services.AddSwaggerGen(c =>
            {
                c.DescribeAllEnumsAsStrings();
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "World Cup API",
                    Description = "World cup 2018 Russia",
                    TermsOfService = "None",
                    Contact = new Contact() { Name = "API at Codit", Email = "support@codit.eu", Url = "www.codit.eu" }
                });
            });

            //Problem+Json
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Instance = context.HttpContext.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Type = "https://asp.net/core",
                        Detail = "Please refer to the errors property for additional details."
                    };
                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json", "application/problem+xml" }
                    };
                };
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, WorldCupContext worldCupContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            // seed DB
            worldCupContext.DataSeed();

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "World Cup API.");
            });

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<LevelOne.Entities.Team, LevelOne.Models.TeamDto>();
                cfg.CreateMap<LevelOne.Entities.Team, LevelOne.Models.TeamDetailsDto>();
                cfg.CreateMap<LevelOne.Entities.Player, LevelOne.Models.PlayerDto>();
            });

        }
    }
}
