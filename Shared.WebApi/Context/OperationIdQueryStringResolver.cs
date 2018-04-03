namespace Shared.WebApi.Context
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web.Http.Controllers;

    public class OperationIdQueryStringResolver : IOperationIdResolver
    {
        private static IList<OperationIdMapping> mappingList = new List<OperationIdMapping>();

        public static void Initialize(string controllerNamespace)
        {
            var mappings = new List<OperationIdMapping>();

            Type type = typeof(IHttpController);
            IEnumerable<Type> controllerTypeList = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.GetName().Name.StartsWith(controllerNamespace))
                .SelectMany(s => s.GetTypes().Where(x => x.Namespace != null && x.Namespace.StartsWith(controllerNamespace)))
                .Where(p => p.GetInterfaces().Contains(type) && !p.IsAbstract);

            foreach (Type controllerType in controllerTypeList)
            {
                MethodInfo[] controllerMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (MethodInfo controllerMethod in controllerMethods)
                {
                    OperationIdQueryStringAttribute operationIdAttribute = controllerMethod.GetCustomAttribute<OperationIdQueryStringAttribute>();
                    if (operationIdAttribute != null)
                    {
                        mappings.Add(new OperationIdMapping
                        {
                            ControllerType = controllerType,
                            MethodName = controllerMethod.Name,
                            QueryPath = operationIdAttribute.QueryPath,
                            QueryParameter = operationIdAttribute.QueryParameter,
                        });
                    }
                }
            }

            mappingList = mappings;
        }

        public bool TryResolve(HttpRequestMessage request, out string operationId)
        {
            if(mappingList == null)
            {
                throw new ConfigurationErrorsException($"Class {typeof(OperationIdQueryStringResolver)} was not initialized");
            }

            // todo: ideally to use routing here but not regex
            OperationIdMapping mapping = mappingList.FirstOrDefault(x => Regex.IsMatch(request.RequestUri.PathAndQuery, x.QueryPath));
            if(mapping != null)
            {
                var parameterValue = request.GetQueryNameValuePairs().FirstOrDefault(x => x.Key == mapping.QueryParameter).Value;
                if(!string.IsNullOrEmpty(parameterValue))
                {
                    operationId = parameterValue;
                    return true;
                }
            }

            operationId = null;
            return false;
        }

        private class OperationIdMapping
        {
            public Type ControllerType { get; set; }
            public string MethodName { get; set; }
            public string QueryPath { get; set; }
            public string QueryParameter { get; set; }
        }
    }
}