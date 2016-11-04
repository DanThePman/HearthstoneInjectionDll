using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Reflection;

namespace HearthstoneInjectionDll.SandBox
{
    /// <summary>
    /// Tries to forbid the use of System.Linq and AddRange calls
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class CallInspector : Attribute
    {
        public static void Monitor(Type t)
        {
            t.GetCustomAttributes(typeof(CallInspector), true);
        }

        public CallInspector()
        {
            foreach (var assemblyType in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!assemblyType.IsClass)
                    continue;

                var classMethods = assemblyType.GetMethods();
                var classProperties = assemblyType.GetProperties();
                CheckClassMethods(classMethods);
                CheckClassProperties(classProperties);
            }
        }

        void CheckClassMethods(MethodInfo[] classMethods)
        {
            foreach (var classMethod in classMethods)
            {
                if ((object)classMethod == null)
                    continue;

                var instructions = MethodBodyReader.GetInstructions(classMethod);
                foreach (Instruction instruction in instructions)
                {
                    MethodInfo methodInfo = instruction.Operand as MethodInfo;

                    if ((object)methodInfo != null)
                    {
                        Type type = methodInfo.DeclaringType;
                        string FullMethodName = type.FullName + "." + methodInfo.Name;
                        if (FullMethodName.Equals("System.Collections.Generic.List<T>.AddRange"))
                            Debug.AppendDesktopLog("CodePermissionsExceptionA", "Unhandled use of System.Collections.Generic.List<T>.AddRange", false);

                        if (FullMethodName.Equals("System.Collections.ArrayList.AddRange"))
                            Debug.AppendDesktopLog("CodePermissionsExceptionB", "Unhandled use of System.Collections.ArrayList.AddRange", false);

                        if (FullMethodName.Contains("System.Linq"))
                            Debug.AppendDesktopLog("CodePermissionsExceptionC", "Unhandled use of System.Linq", false);

                        Debug.AppendDesktopLog(FullMethodName, append: false);
                    }
                }
            }
        }

        void CheckClassProperties(PropertyInfo[] classProperties)
        {
            List<MethodInfo> methodInfos = new List<MethodInfo>();
            foreach (var classProperty in classProperties)
            {
                methodInfos.Add(classProperty.GetGetMethod());
            }

            MethodInfo[] methodInfosArray = new MethodInfo[methodInfos.Count];
            for (int i = 0; i < methodInfos.Count; i++)
            {
                methodInfosArray[i] = methodInfos[i];
            }

            CheckClassMethods(methodInfosArray);
        }
    }
}
