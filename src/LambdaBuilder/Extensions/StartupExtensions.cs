using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LambdaBuilder
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddLambdaBuilder(this IServiceCollection services, IConfiguration configuration)
        {
            services
                //.AddSingleton<IPredicateLambdaBuilder, PredicateLambdaBuilder>()
                .AddOptions()
                .Configure<LambdaBuilderSettings>(configuration.GetSection("LambdaBuilderSettings"));

            return services;
        }

        /// <summary>
        /// This method already exists in Microsoft.Extensions.Hosting library
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        public static IServiceCollection Configure<TOptions>(this IServiceCollection services,
            IConfigurationSection configurationSection) where TOptions : class
        {
            services.Configure<TOptions>(opt =>
            {
                opt = configurationSection.Get<TOptions>();
            });

            return services;
        }
    }
}
