using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionGenerator
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SkipGenerationAttribute: Attribute 
    {
    }
}
