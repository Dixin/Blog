namespace Tutorial.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.Tests.Introduction;

    internal static class MSTest
    {
        internal static void Run(bool stopWhenFail = false)
        {
            new ContactTests().EmailValidationTest();
            List<(MethodInfo, Exception, double)> failed = new List<(MethodInfo, Exception, double)>();
            typeof(MSTest).Assembly.GetTypes()
                .Where(type => Attribute.GetCustomAttribute(type, typeof(TestClassAttribute)) != null)
                .ForEach(type =>
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    object instance = Activator.CreateInstance(type);
                    type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(method => Attribute.GetCustomAttribute(method, typeof(TestMethodAttribute)) != null)
                        .ForEach(method =>
                        {
                            Trace.WriteLine($"Test starts: {type.Name}.{method.Name}".WriteLine());
                            try
                            {
                                method.Invoke(instance, Array.Empty<object>());
                                stopwatch.Stop();
                                Trace.WriteLine($"Test passed: {type.Name}.{method.Name} {stopwatch.Elapsed.TotalSeconds}".WriteLine());
                            }
                            catch (Exception exception)
                            {
                                stopwatch.Stop();
                                failed.Add((method, exception, stopwatch.Elapsed.TotalSeconds));
                                Trace.WriteLine($"Test failed: {type.Name}.{method.Name} {stopwatch.Elapsed.TotalSeconds}: {exception} {exception.GetBaseException()}".WriteLine());
                                if (stopWhenFail)
                                {
                                    throw;
                                }
                            }
                        });
                });
            if (failed.Any())
            {
                $"Failed tests: {failed.Count}.".WriteLine();
                failed.ForEach(test => $"{test.Item1.DeclaringType.Name}.{test.Item1.Name} {test.Item3}: {test.Item2} {test.Item2.GetBaseException()}".WriteLine());
            }
        }
    }
}
