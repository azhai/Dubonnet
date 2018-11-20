using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dubonnet.QueryBuilder.Compilers
{
    internal class ConditionProvider<Q> where Q : QueryFactory<Q>
    {
        private readonly Type compilerType;
        private readonly Dictionary<string, MethodInfo> methodsCache = new Dictionary<string, MethodInfo>();
        private readonly object syncRoot = new object();

        public ConditionProvider(Compiler<Q> compiler)
        {
            this.compilerType = compiler.GetType();
        }

        public MethodInfo GetMethodInfo(Type clauseType, string methodName)
        {
            // The cache key should take the type and the method name into consideration
            var cacheKey = methodName + "::" + clauseType.FullName;

            if (methodsCache.ContainsKey(cacheKey))
            {
                return methodsCache[cacheKey];
            }

            lock (syncRoot)
            {
                if (methodsCache.ContainsKey(cacheKey))
                {
                    return methodsCache[cacheKey];
                }

                return methodsCache[cacheKey] = FindMethodInfo(clauseType, methodName);
            }
        }

        private MethodInfo FindMethodInfo(Type clauseType, string methodName)
        {
            MethodInfo methodInfo = compilerType
                .GetRuntimeMethods()
                .FirstOrDefault(x => x.Name == methodName);

            if (methodInfo == null)
            {
                throw new Exception($"Failed to locate a compiler for {methodName}.");
            }

            if (clauseType.IsConstructedGenericType && methodInfo.GetGenericArguments().Any())
            {
                methodInfo = methodInfo.MakeGenericMethod(clauseType.GenericTypeArguments);
            }

            return methodInfo;
        }
    }
}