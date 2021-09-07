using Contract;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ServiceHostApp;

public partial class Service: ServiceBase<IService>
{
    public Service(IService service): base(service)
    {
    }
}
