using Microsoft.Extensions.Hosting;
using Serilog;

namespace rsb.HostBuilders.Utils;

internal static class ConfigureHost
{
    internal static void Logger(HostBuilderContext hostBuilderContext,
        LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration.WriteTo.Console();
    } 
}