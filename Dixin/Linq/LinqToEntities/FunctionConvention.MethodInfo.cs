namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Reflection;

    public static partial class FunctionConvention
    {
        private static bool IsFunction
            (this MethodInfo methodInfo) => methodInfo.IsScalarValuedFunction() || methodInfo.IsTableValuedFunction();

        internal static bool IsScalarValuedFunction(this MethodInfo methodInfo)
        {
            Type returnType = methodInfo.ReturnType;
            if (returnType.IsGenericType)
            {
                Type genericTypeDefinition = returnType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(ObjectResult<>) // Stored procedure.
                    || genericTypeDefinition == typeof(IQueryable<>)) // Table-valued function.
                {
                    return false;
                }
            }
            else if (returnType == typeof(int))
            {
                return methodInfo.ReturnParameter.GetCustomAttribute<ParameterAttribute>() != null 
                    || methodInfo.GetCustomAttribute<FunctionAttribute>()?.IsComposable == true;
            }

            return true;
        }

        private static bool IsTableValuedFunction(this MethodInfo methodInfo)
        {
            Type returnType = methodInfo.ReturnType;
            return returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(IQueryable<>);
        }

        private static IEnumerable<Type> GetStoredProcedureReturnTypes(this MethodInfo methodInfo)
        {
            ParameterInfo returnParameterInfo = methodInfo.ReturnParameter;
            if (returnParameterInfo.ParameterType == typeof(int))
            {
                return Enumerable.Empty<Type>();
            }

            // returnParameterInfo.ParameterType is ObjectResult<T>.
            Type returnParameterClrType = returnParameterInfo.ParameterType.GenericTypeArguments.Single();
            List<Type> returnParameterClrTypes = methodInfo
                .GetCustomAttributes<ResultTypeAttribute>()
                .Select(returnTypeAttribute => returnTypeAttribute.Type) // TODO: Duplication of Type.
                .ToList();
            returnParameterClrTypes.RemoveAll(clrType => clrType == returnParameterClrType);
            returnParameterClrTypes.Insert(0, returnParameterClrType);
            return returnParameterClrTypes;
        }

        private static string GetStoreCommandText(this MethodInfo methodInfo, FunctionAttribute functionAttribute)
        {
            if (methodInfo.IsScalarValuedFunction() && functionAttribute.IsComposable != true)
            {
                string schema = functionAttribute.Schema;
                schema = string.IsNullOrWhiteSpace(schema) ? string.Empty : $"[{schema}].";
                string name = functionAttribute.FunctionName;
                name = string.IsNullOrWhiteSpace(schema) ? methodInfo.Name : name;
                IEnumerable<string> parameterNames = methodInfo
                    .GetParameters()
                    .Select(parameterInfo =>
                    {
                        ParameterAttribute parameterAttribute = parameterInfo.GetCustomAttribute<ParameterAttribute>();
                        string parameterName = parameterAttribute?.Name;
                        return string.IsNullOrWhiteSpace(parameterName) ? parameterInfo.Name : parameterName;
                    })
                    .Select(parameterName => $"@{parameterName}");
                return $"SELECT {schema}[{name}]({string.Join(", ", parameterNames)})";
            }

            return null;
        }
    }
}