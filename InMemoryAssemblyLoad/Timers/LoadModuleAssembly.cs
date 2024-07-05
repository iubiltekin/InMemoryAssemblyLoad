using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace InMemoryAssemblyLoad.Timers
{
    internal class LoadModuleAssembly : MarshalByRefObject
    {
        private Assembly _assembly;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void LoadAssembly(byte[] rawAssembly)
        {
            _assembly = Assembly.Load(rawAssembly, null);
        }

        public object ExecuteStaticMethod(string className, string methodName)
        {
            var moduleType = _assembly.GetTypes().FirstOrDefault(q => q.Name == className);
            if (moduleType != null)
            {
                var moduleInstance = _assembly.CreateInstance(moduleType.FullName);
                if (moduleInstance != null)
                {
                    var executeMethod = moduleType.GetMethod(methodName);
                    if (executeMethod != null)
                        return executeMethod.Invoke(moduleInstance, BindingFlags.InvokeMethod, null, null, CultureInfo.CurrentCulture);
                }
            }

            return null;
        }
    }
}