using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangDreamChartsConverter.IServices;
using BangDreamChartsConverter.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

namespace BangDreamChartsConverter
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddScoped<IConvertService, ConvertService>();
            services.AddMvc().AddNewtonsoftJson(options=>{options.SerializerSettings.ContractResolver= new DefaultContractResolver();});
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}");
            });
        }
    }
}
