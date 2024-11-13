namespace ASureBus.ConfigurationObjects.Exceptions;

public class ConfigurationNullException(string configName)
    : Exception($"Configuration object '{configName}' cannot be null.");