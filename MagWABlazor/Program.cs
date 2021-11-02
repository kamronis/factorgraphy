using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MagWABlazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Инициирование объектов Infobase
            Infobase.engine = new RDFEngine.REngine();
            ((RDFEngine.REngine)Infobase.engine).Load();
            Infobase.engine.Build();
            Infobase.rontology = new RDFEngine.ROntology();


            await builder.Build().RunAsync();
        }
    }
}
