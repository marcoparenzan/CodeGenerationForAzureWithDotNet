using Contract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceLibrary;

namespace ServiceHostApp
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services => {

                    services.Add(ServiceDescriptor.Singleton<IService, ServiceImpl>());
                
                })
                .Build();

            host.Run();
        }
    }
}