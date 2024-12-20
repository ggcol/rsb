﻿using Microsoft.Extensions.Configuration;

namespace ASureBus.Utils;

internal static class ConfigProvider
{
    internal static TSettings LoadSettings<TSettings>(
        IConfiguration configuration)
        where TSettings : class, new()
    {
        var settings = new TSettings();
        configuration.GetSection(typeof(TSettings).Name).Bind(settings);
        return settings;
    }
}