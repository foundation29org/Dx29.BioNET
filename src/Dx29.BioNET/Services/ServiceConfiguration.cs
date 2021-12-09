using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dx29.Services
{
    static public class ServiceConfiguration
    {
        static public void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<BioEntityService>();
            //services.AddSingleton<OrphaNET>();
            //services.AddSingleton<Omim>();
            services.AddSingleton<EnsembleService>();
        }

        static public void ConfigureServices(this IServiceProvider services, IConfiguration configuration)
        {
            var bioEntity = services.GetService<BioEntityService>();
            bioEntity.Initialize();
            //var orpha = services.GetService<OrphaNET>();
            //orpha.Initialize();
            //var diagnosis = services.GetService<DiagnosisService>();
            //diagnosis.Initialize();
        }
    }
}
