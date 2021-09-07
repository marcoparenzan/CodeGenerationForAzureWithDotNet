
using Bogus;
using Contract;
using MinimalApiGenerator;
using System;
using System.Linq;

namespace MinimalApiCore;

[PublishHttp]
public class ServiceImpl : IService
{
    public string DoSomething(string a, int b)
    {
        var lorem = new Bogus.DataSets.Lorem("en");
        return a + "//" + string.Join("//", lorem.Words(b).ToArray());
    }

    public string ElseSomething(string a, int b)
    {
        var lorem = new Bogus.DataSets.Lorem("en");
        return a + "//" + string.Join("//", lorem.Words(b).ToArray());
    }

}
