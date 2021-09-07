using System;
using System.Collections.Generic;
using System.Text;

namespace MinimalApiGenerator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PublishHttpAttribute: Attribute 
    {
    }
}
