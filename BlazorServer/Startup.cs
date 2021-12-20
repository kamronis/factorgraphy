using BlazorServer.Data;
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
using System.Xml.Linq;

namespace BlazorServer
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

            Infobase.engine = new RDFEngine.REngine();

            // ============= Загрузка базы данных из фототеки
            Infobase.engine.Load(RDFEngine.PhototekaGenerator.Generate(100));

            // ============= Загрузка базы данных из текста модели
            if (true)
            {
                Infobase.engine.Clear();
                //                string graphModelText = @"<?xml version='1.0' encoding='utf-8'?>
                //<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>
                //  <person rdf:about='p3817'>
                //    <name xml:lang='ru'>Иванов</name>
                //    <from-date>1988</from-date>
                //  </person>
                //  <person rdf:about='p3818'>
                //    <from-date>1999</from-date>
                //    <name xml:lang='ru'>Петров</name>
                //  </person>
                //  <org-sys rdf:about='o19302'>
                //    <from-date>1959</from-date>
                //    <name>НГУ</name>
                //  </org-sys>
                //  <org-sys rdf:about='o19305'>
                //    <from-date>1959</from-date>
                //    <name>ИСИ</name>
                //  </org-sys>
                //  <participation rdf:about='r1111'>
                //    <participant rdf:resource='p3817' />
                //    <in-org rdf:resource='o19302' />
                //    <role>профессор</role>
                //  </participation>
                //  <participation rdf:about='r1112'>
                //    <participant rdf:resource='p3818' />
                //    <in-org rdf:resource='o19302' />
                //    <from-date>2008</from-date>
                //    <role>ассистент</role>
                //  </participation>
                //</rdf:RDF>";
                //System.Xml.Linq.XElement graphModelXml = System.Xml.Linq.XElement.Parse(graphModelText);
                XElement graphModelXml = XElement.Load("C:\\Users\\Kamroni\\Desktop\\SypCassete\\meta\\SypCassete_current_20110112.fog");
                var flow = graphModelXml.Elements()
                    .Select(el =>
                    {
                        if (el.Name.LocalName == "photo-doc")
                        {
                            XElement iisStore = el.Element("iisstore");
                            if (iisStore != null)
                            {
                                var kk =  new XElement(el.Name, 
                                    new XAttribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about", el.Attribute("{http://www.w3.org/1999/02/22-rdf-syntax-ns#}about").Value),
                                    el.Elements().Where(e => e.Name.LocalName != "iisstore")
                                    .Concat(iisStore.Attributes().Where(a => a.Name.LocalName == "uri" || a.Name.LocalName == "documenttype")
                                    .Select(at => new XElement(at.Name, at.Value))));
                                return kk;
                            }
                            else
                            {
                                return el;
                            }
                        }
                        else
                        {
                            return el;
                        }
                    });
                Infobase.engine.Load(flow);
                Infobase.cassPath = "C:\\Users\\Kamroni\\Desktop\\SypCassete";

                Infobase.engine.Build();
                Infobase.ront = new RDFEngine.ROntology("C:\\Users\\Kamroni\\Desktop\\SypCassete\\meta\\ontology_iis-v12-doc_ruen.xml"); // тестовая онтология
            }


            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
