using Microsoft.Extensions.Hosting;
using Serilog;

namespace Rsb.HostBuilders.Utils;

internal static class ConfigureHost
{
    internal static void Logger(HostBuilderContext hostBuilderContext,
        LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration.WriteTo.Console();
    } 
}