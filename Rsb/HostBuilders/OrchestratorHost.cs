using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreLinq;
using Rsb.Configurations;
using Rsb.Services;
using Rsb.Workers;

namespace Rsb.HostBuilders;

public static class OrchestratorHost
{
    public static IHostBuilder Create<TSettings>()
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        return new HostBuilder()
            .ConfigureServices((hostBuilderContest, services) =>
            {
                //TODO 
                services.Configure<TSettings>(
                    hostBuilderContest.Configuration.GetSection("TODO"));

                services.AddMemoryCache();

                services
                    .AddScoped<IMessagingContext, MessagingContext>()
                    .AddSingleton<IAzureServiceBusService,
                        AzureServiceBusService<TSettings>>()
                    .AddSingleton<IMessageEmitter, MessageEmitter>();

                Assembly.GetEntryAssembly()?
                    .GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false })
                    .Where(t =>
                        t.BaseType is not null
                        && t.BaseType.IsGenericType
                        && t.BaseType.GetGenericTypeDefinition() ==
                        typeof(Saga<>)
                    )
                    .ToArray()
                    .ForEach(sagaType =>
                    {
                        var sagaDataType = sagaType
                            .BaseType!
                            .GetGenericArguments()
                            .Single();

                        //add saga as service
                        // services.AddScoped(typeof(IAmSaga), sagaType);
                        services.AddScoped<IAmSaga>(x => new Saga()
                        {
                            Data = (IAmSagaData)Activator.CreateInstance(
                                sagaDataType)
                        });


                        //add saga-data as service
                        services.AddScoped(typeof(IAmSagaData), sagaDataType);

                        var sagaHolderServiceType = typeof(ISagaHolder<,>)
                            .MakeGenericType(sagaType, sagaDataType);

                        var sagaHolderImplType = typeof(SagaHolder<,>)
                            .MakeGenericType(sagaType, sagaDataType);

                        //add saga-holder as service
                        services.AddScoped(sagaHolderServiceType,
                            sagaHolderImplType);

                        /*
                         * add hosted service
                         */
                        var engineImplType = typeof(SagaWorker<,>)
                            .MakeGenericType(sagaType, sagaDataType);

                        services.AddSingleton(typeof(IHostedService),
                            engineImplType);
                    });
            });
    }
}