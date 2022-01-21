using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestTools
{
    internal class AsyncSetUpTearDownCommand : BeforeAfterTestCommandBase<MethodInfo>
    {
        public AsyncSetUpTearDownCommand(TestCommand innerCommand)
            : base(innerCommand, "SetUp", "TearDown")
        {
            if (Test.TypeInfo.Type != null)
            {
                BeforeActions = GetMethodsWithAttributeFromFixture(Test.TypeInfo.Type, typeof(AsyncSetUpAttribute));
                AfterActions = GetMethodsWithAttributeFromFixture(Test.TypeInfo.Type, typeof(AsyncTearDownAttribute))
                    .Reverse().ToArray();
            }
        }

        private static MethodInfo[] GetMethodsWithAttributeFromFixture(Type fixtureType, Type setUpType)
        {
            MethodInfo[] methodsWithAttribute = Reflect.GetMethodsWithAttribute(fixtureType, setUpType, true);
            return methodsWithAttribute.Where(x => x.ReturnType == typeof(Task)).ToArray();
        }

        protected override IEnumerator InvokeBefore(MethodInfo action, Test test, UnityTestExecutionContext context)
        {
            var task = Reflect.InvokeMethod(action, context.TestObject) as Task;
            return WaitForTask(task);
        }

        protected override IEnumerator InvokeAfter(MethodInfo action, Test test, UnityTestExecutionContext context)
        {
            var task = Reflect.InvokeMethod(action, context.TestObject) as Task;
            return WaitForTask(task);
        }

        private IEnumerator WaitForTask(Task task)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
                throw task.Exception!;
        }

        protected override BeforeAfterTestCommandState GetState(UnityTestExecutionContext context)
        {
            return context.SetUpTearDownState;
        }
    }
}