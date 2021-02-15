using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools
{
    internal class TestTask
    {
        private readonly ITestExecutionContext m_Context;
        private static Task m_TestTask;

        public static Task Task
        {
            get { return m_TestTask; }
        }

        public static void Reset()
        {
            m_TestTask = null;
        }

        public TestTask(ITestExecutionContext context, Task testTask)
        {
            m_Context = context;
            m_TestTask = testTask;
        }

        public IEnumerator Execute()
        {
            m_Context.CurrentResult.SetResult(ResultState.Success);

            while (true)
            {
                if (!m_Context.CurrentResult.ResultState.Equals(ResultState.Success))
                {
                    yield break;
                }

                if (m_TestTask.IsFaulted)
                {
                    m_Context.CurrentResult.RecordException(m_TestTask.Exception!.InnerException);
                    yield break;
                }

                if (m_TestTask.IsCanceled)
                {
                    yield break;
                }

                if (m_TestTask.IsCompleted)
                {
                    yield break;
                }

                yield return null;
            }
        }
    }
}