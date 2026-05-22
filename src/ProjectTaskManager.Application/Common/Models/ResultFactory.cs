using System.Reflection;

namespace ProjectTaskManager.Application.Common.Models;

internal static class ResultFactory
{
    internal static TResponse CreateFailure<TResponse>(Error error)
    {
        var type = typeof(TResponse);

        if (type == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = type.GetGenericArguments()[0];
            var method = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod(nameof(Result.Failure), BindingFlags.Public | BindingFlags.Static)!;

            return (TResponse)method.Invoke(null, [error])!;
        }

        throw new InvalidOperationException($"Cannot create failure result for type {type.Name}.");
    }
}
