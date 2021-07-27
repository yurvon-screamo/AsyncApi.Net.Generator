using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Saunter.Generation;
using Saunter.Serialization;
using Saunter.Utils;

namespace Saunter
{
    public static class AsyncApiServiceCollectionExtensions
    {
        public static IServiceCollection AddAsyncApiSchemaGeneration(this IServiceCollection services, Action<AsyncApiOptions> setupAction)
        {
            services.AddOptions();

            services.TryAddTransient<IAsyncApiDocumentProvider, AsyncApiDocumentProvider>();
            services.TryAddTransient<IDocumentGenerator, DocumentGenerator>();
            services.TryAddTransient<IAsyncApiDocumentSerializer, NewtonsoftAsyncApiDocumentSerializer>();

            if (setupAction != null) services.Configure(setupAction);

            return services;
        }
    }
}