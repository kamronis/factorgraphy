using BlazorKonstantinLearn.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDFEngine;
using System.Xml;
using System.Xml.Linq;

namespace BlazorKonstantinLearn
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            // Инициирование объектов Infobase

            XElement xml = XElement.Load(@"C:\Home\Konstantin2\family1234.xml");
            XElement xml_led = XElement.Load(@"C:\Users\shish\Downloads\hellenic-police.rdf");
            XElement xml_rdf = XElement.Load(@"C:\Home\Data\SypCassete\meta\SypCassete_current_new.rdf");
        
            Infobase.engine = new RDFEngine.REngine();
            //((RDFEngine.REngine)Infobase.engine).Load();
            ((RDFEngine.REngine)Infobase.engine).Load(xml_rdf.Elements());
            Infobase.engine.Build();
            Infobase.rontology = new RDFEngine.ROntology();

            
            
            
   
        }
    }
}
