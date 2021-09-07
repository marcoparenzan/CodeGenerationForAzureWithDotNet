using Contract;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceLibrary;
using System.Threading.Tasks;

namespace ServiceHostAppGenerated
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services => {

                    services.Add(ServiceDescriptor.Singleton(new ServiceImpl()));
                
                })
                .Build();

            host.Run();
        }
    }
}