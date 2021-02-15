using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools.TestRunner
{
    internal class TestTaskWrapper
    {
        private readonly TestMethod m_TestMethod;

        public TestTaskWrapper(TestMethod testMethod)
        {
            m_TestMethod = testMethod;
        }

        public Task GetTask(ITestExecutionContext context)
        {
            if (m_TestMethod.Method.ReturnType.Type == typeof(Task))
            {
                return HandleAsyncTest(context);
            }
            var message = string.Format("Return type {0} of {1} in {2} is not supported.",
                m_TestMethod.Method.ReturnType, m_TestMethod.Method.Name, m_TestMethod.Method.TypeInfo.FullName);
            throw new InvalidSignatureException(message);
        }

        private Task HandleAsyncTest(ITestExecutionContext context)
        {
            try
            {
                return m_TestMethod.Method.MethodInfo.Invoke(context.TestObject, m_TestMethod.parms != null ? m_TestMethod.parms.OriginalArguments : null) as Task;
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is IgnoreException)
                {
                    context.CurrentResult.SetResult(ResultState.Ignored, e.InnerException.Message);
                    return null;
                }
                throw;
            }
        }
    }
}
