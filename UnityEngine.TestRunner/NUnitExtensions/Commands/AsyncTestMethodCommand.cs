using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using NUnit.Framework.Internal.Execution;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestTools
{
    internal class AsyncTestMethodCommand : TestCommand, IEnumerableTestMethodCommand
    {
        private readonly TestMethod testMethod;

        public AsyncTestMethodCommand(TestMethod testMethod)
            : base(testMethod)
        {
            this.testMethod = testMethod;
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            yield return null;

            Task currentExecutingTestTask;
            try
            {
                currentExecutingTestTask = new TestTaskWrapper(testMethod).GetTask(context);
            }
            catch (Exception ex)
            {
                context.CurrentResult.RecordException(ex);
                yield break;
            }
            
            if (currentExecutingTestTask != null)
            {
                var testTaskYieldInstruction = new TestTask(context, currentExecutingTestTask);

                yield return testTaskYieldInstruction;

                var enumerator = testTaskYieldInstruction.Execute();

                var executingEnumerator = ExecuteEnumerableAndRecordExceptions(enumerator, context);
                while (executingEnumerator.MoveNext())
                {
                    yield return executingEnumerator.Current;
                }
            }
            else
            {
                if (context.CurrentResult.ResultState != ResultState.Ignored)
                {
                    context.CurrentResult.SetResult(ResultState.Success);
                }
            }
        }

        private static IEnumerator ExecuteEnumerableAndRecordExceptions(IEnumerator enumerator, ITestExecutionContext context)
        {
            while (true)
            {
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    context.CurrentResult.RecordException(ex);
                    break;
                }

                if (enumerator.Current is IEnumerator)
                {
                    var current = (IEnumerator)enumerator.Current; 
                    yield return ExecuteEnumerableAndRecordExceptions(current, context);
                }
                else
                {
                    yield return enumerator.Current;
                }
            }
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException("Use ExecuteEnumerable");
        }
    }
}
