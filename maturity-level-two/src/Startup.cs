﻿using System.Linq;
using Codit.LevelTwo.DB;
using Codit.LevelTwo.Entities;
using Codit.LevelTwo.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Codit.LevelTwo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            AutoMapperConfig.Initialize();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure database
            services.ConfigureDatabase(Configuration);

            // Configure API
            services.ConfigureMvc();
            services.ConfigureOpenApiGeneration();
            services.ConfigureRouting();
            services.ConfigureInvalidStateHandling();
            services.AddMvc(options =>
            {
                var jsonInputFormatters = options.InputFormatters.OfType<JsonInputFormatter>();
                var jsonInputFormatter = jsonInputFormatters.First();
                options.InputFormatters.Clear();
                options.InputFormatters.Add(jsonInputFormatter);
                var jsonOutputFormatters = options.OutputFormatters.OfType<JsonOutputFormatter>();
                var jsonOutputFormatter = jsonOutputFormatters.First();
                options.OutputFormatters.Clear();
                options.OutputFormatters.Add(jsonOutputFormatter);
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, CoditoContext coditoContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Seed DB
            coditoContext.DataSeed();
            
            // Configure API
            app.UseHttpsRedirection();
            app.UseExceptionHandlerWithProblemJson();
            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseMvc();
            app.UseOpenApi();
            // Configure Automapper
            AutoMapperConfig.Initialize();
        }
    }
}
