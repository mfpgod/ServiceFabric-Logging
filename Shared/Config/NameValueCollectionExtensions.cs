namespace Shared.Config
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;

    public static class NameValueCollectionExtensions
    {
        public static T GetRefValue<T>(this NameValueCollection collection, string name, T defaultValue = null)
            where T : class
        {
            string value = collection[name];
            if (string.IsNullOrEmpty(value))
            {
                if (defaultValue != null)
                {
                    return defaultValue;
                }

                throw new ConfigurationErrorsException($"AppSetting {name} is null or empty");
            }

            return ConvertValue<T>(value, name);
        }

        public static T GetValue<T>(this NameValueCollection collection, string name, T? defaultValue = null)
            where T : struct
        {
            string value = collection[name];
            if (string.IsNullOrEmpty(value))
            {
                if (defaultValue.HasValue)
                {
                    return defaultValue.Value;
                }

                throw new ConfigurationErrorsException($"AppSetting {name} is null or empty");
            }

            return ConvertValue<T>(value, name);
        }

        public static T ConvertValue<T>(string value, string name)
        {
            Type toType = typeof(T);
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(toType);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    return (T)converter.ConvertFrom(value);
                }

                return (T)Convert.ChangeType(value, toType);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException($"AppSetting {name} could not be parsed to {toType.Name}", ex);
            }
        }
    }
}
