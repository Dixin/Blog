namespace EntityFramework.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static partial class Function
    {
        private static IEnumerable<Type> GetStoredProcedureReturnTypes(this MethodInfo methodInfo)
        {
            ParameterInfo returnParameterInfo = methodInfo.ReturnParameter;
            if (returnParameterInfo.ParameterType == typeof(int))
            {
                return Enumerable.Empty<Type>();
            }

            // returnParameterInfo.ParameterType is ObjectResult<T>.
            Type returnParameterClrType = returnParameterInfo.ParameterType.GetGenericArguments().Single();
            List<Type> returnParameterClrTypes = methodInfo
                .GetCustomAttributes<ResultTypeAttribute>()
                .Select(returnTypeAttribute => returnTypeAttribute.Type) // TODO: Duplication of Type.
                .ToList();
            returnParameterClrTypes.RemoveAll(clrType => clrType == returnParameterClrType);
            returnParameterClrTypes.Insert(0, returnParameterClrType);
            return returnParameterClrTypes;
        }

        private static string GetStoreCommandText(this MethodInfo methodInfo, FunctionAttribute functionAttribute, string functionName)
        {
            if (functionAttribute.Type == FunctionType.NonComposableScalarValuedFunction)
            {
                string schema = functionAttribute.Schema;
                schema = string.IsNullOrWhiteSpace(schema) ? string.Empty : $"[{schema}].";
                IEnumerable<string> parameterNames = methodInfo
                    .GetParameters()
                    .Select(parameterInfo =>
                    {
                        ParameterAttribute parameterAttribute = parameterInfo.GetCustomAttribute<ParameterAttribute>();
                        string parameterName = parameterAttribute?.Name;
                        return string.IsNullOrWhiteSpace(parameterName) ? parameterInfo.Name : parameterName;
                    })
                    .Select(parameterName => $"@{parameterName}");
                return $"SELECT {schema}[{functionName}]({string.Join(", ", parameterNames)})";
            }

            return null;
        }
    }
}