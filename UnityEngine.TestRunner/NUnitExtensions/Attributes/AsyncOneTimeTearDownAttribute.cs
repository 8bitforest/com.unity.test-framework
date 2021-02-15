using System;
using NUnit.Framework;

namespace UnityEngine.TestTools
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AsyncOneTimeTearDownAttribute : NUnitAttribute
    {
    }
}