using ExpressionEngine.Functions.Base;
using EAVFW.ExpressionEngine.Functions;
using Microsoft.Extensions.DependencyInjection;

namespace EAVFW.ExpressionEngine.Auxiliary
{
    public static class ExpressionEngineExtension
    {
        public static void AddCustomFunctions(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<FormDataAccessor>();
            serviceCollection.AddScoped<IFunction, FormDataAccessor>(x => x.GetService<FormDataAccessor>());
            serviceCollection.AddScoped<LookUpResolver>();
            serviceCollection.AddScoped<IFunction, LookUpResolver>(x => x.GetRequiredService<LookUpResolver>());
            serviceCollection.AddScoped<IFunction, Coalesce>();
        }
    }
}