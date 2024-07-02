using Microsoft.Win32;

public static class ProtocolManager
{
    public static bool IsProtocolRegistered(string protocolName)
    {
        if (string.IsNullOrWhiteSpace(protocolName))
        {
            throw new ArgumentException("Protocol name cannot be null or whitespace.", nameof(protocolName));
        }

        string protocolPath = $@"HKEY_CLASSES_ROOT\{protocolName}";
        var result = Registry.GetValue(protocolPath, "", null);
        return result != null;
    }

    public static void RegisterProtocol(string protocolName, string applicationPath)
    {
        if (string.IsNullOrWhiteSpace(protocolName))
        {
            throw new ArgumentException("Protocol name cannot be null or whitespace.", nameof(protocolName));
        }

        if (string.IsNullOrWhiteSpace(applicationPath))
        {
            throw new ArgumentException("Application path cannot be null or whitespace.", nameof(applicationPath));
        }

        string protocolPath = $@"SOFTWARE\Classes\{protocolName}";
        string commandPath = $@"{protocolPath}\shell\open\command";

        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(protocolPath))
        {
            if (key != null)
            {
                key.SetValue("", $"URL:{protocolName} Protocol");
                key.SetValue("URL Protocol", "");
            }
        }

        using (RegistryKey commandKey = Registry.CurrentUser.CreateSubKey(commandPath))
        {
            if (commandKey != null)
            {
                commandKey.SetValue("", $"\"{applicationPath}\" \"%1\"");
            }
        }
    }

    public static bool IsProtocolUpToDate(string protocolName, string applicationPath)
   {
        if (string.IsNullOrWhiteSpace(protocolName))
        {
            throw new ArgumentException("Protocol name cannot be null or whitespace.", nameof(protocolName));
        }

        if (string.IsNullOrWhiteSpace(applicationPath))
        {
            throw new ArgumentException("Application path cannot be null or whitespace.", nameof(applicationPath));
        }

        string commandPath = $@"HKEY_CURRENT_USER\SOFTWARE\Classes\{protocolName}\shell\open\command";
        var result = Registry.GetValue(commandPath, "", null);
        return result != null && result.ToString() == $"\"{applicationPath}\" \"%1\"";
    }
}
