using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs;
using Xunit;

namespace Belial.Tests
{
    public class FunctionValidationTests
    {
        [Fact]
        public void AllPublicStaticMethodsOnFunctionClassHaveValidFunctionNameAttribute()
        {
            var functions = typeof(Functions).GetMethods(BindingFlags.Public | BindingFlags.Static);

            var resultsExpectToBeEmpty = functions.Select(x =>
                {
                    var functionNameAttribute = x.GetCustomAttribute<FunctionNameAttribute>();
                    if (functionNameAttribute == null || string.IsNullOrWhiteSpace(functionNameAttribute.Name))
                        return x.Name;

                    return null;
                })
                .Where(x => x != null);

            Assert.Empty(resultsExpectToBeEmpty);
        }

        [Fact]
        public void AllFunctionsHaveATrigger()
        {
            var functions = typeof(Functions).GetMethods(BindingFlags.Public | BindingFlags.Static);

            var resultsExpectToBeEmpty = functions.Select(x =>
                {
                    var parameters = x.GetParameters();

                    if (parameters.Any(p =>
                        p.GetCustomAttribute<HttpTriggerAttribute>() != null ||
                        p.GetCustomAttribute<QueueTriggerAttribute>() != null))
                        return null;

                    return x.Name;
                })
                .Where(x => x != null);

            Assert.Empty(resultsExpectToBeEmpty);
        }
    }
}