using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XaropariaEtiquetas.Configuration;
using XaropariaEtiquetas.Forms;
using XaropariaEtiquetas.Services;

namespace XaropariaEtiquetas
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            using var host = CriarHost();

            var form = host.Services.GetRequiredService<FrmPrincipal>();
            Application.Run(form);
        }

        private static IHost CriarHost()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Configuration
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var services = builder.Services;

            services.Configure<BalancaOptions>(
                builder.Configuration.GetSection(BalancaOptions.SectionName));
            services.Configure<ImpressoraOptions>(
                builder.Configuration.GetSection(ImpressoraOptions.SectionName));

            services.AddSingleton(_ =>
            {
                var connectionString = builder.Configuration.GetConnectionString("Sap")
                    ?? throw new InvalidOperationException(
                        "ConnectionStrings:Sap não configurada.");
                return new SapService(connectionString);
            });

            services.AddSingleton<BalancaService>();
            services.AddSingleton<EtiquetaService>();
            services.AddSingleton<ImpressoraService>();
            services.AddTransient<FrmPrincipal>();

            return builder.Build();
        }
    }
}