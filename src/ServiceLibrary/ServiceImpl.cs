
using Bogus;
using Contract;
using System;
using System.Linq;

namespace ServiceLibrary;
public class ServiceImpl : IService
{
    public string DoSomething(string a, int b)
    {
        var lorem = new Bogus.DataSets.Lorem("en");
        return a + "//" + string.Join("//", lorem.Words(b).ToArray());
    }
}
