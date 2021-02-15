using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace UnityEngine.TestTools
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AsyncTestAttribute : CombiningStrategyAttribute, ISimpleTestBuilder, IImplyFixture
    {
        public AsyncTestAttribute() : base(new UnityCombinatorialStrategy(), new ParameterDataSourceProvider()) {}
    
        private readonly NUnitTestCaseBuilder _builder = new NUnitTestCaseBuilder();
        
        public TestMethod BuildFrom(IMethodInfo method, Test suite)
        {
            TestCaseParameters parms = new TestCaseParameters
            {
                ExpectedResult = new object(),
                HasExpectedResult = true
            };
    
            var t = _builder.BuildTestMethod(method, suite, parms);
    
            if (t.parms != null)
                t.parms.HasExpectedResult = false;
    
            if (!method.ReturnType.IsType(typeof(Task)))
            {
                t.RunState = RunState.NotRunnable;
                t.Properties.Set(PropertyNames.SkipReason, "Method marked with AsyncTest must return Task.");
            }
    
            return t;
        }
    }
}