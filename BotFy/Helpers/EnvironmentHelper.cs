namespace BotFy.Helpers;

public class EnvironmentHelper
{
    public static string Get(string key, string? defaultValue = null)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (value is null)
        {
            if (defaultValue is not null)
            {
                return defaultValue;
            }

            throw new ArgumentException($"The value of {key} is not set");
        }

        return value;
    }

    public static T Get<T>(string key, T defaultValue)
    {
        ArgumentNullException.ThrowIfNull(defaultValue);

        var value = Get(key, defaultValue.ToString());

        return (T)Convert.ChangeType(value, typeof(T));
    }
}
