namespace Shared.Logging
{
    using Microsoft.Extensions.Logging;

    public class LoggingContext
    {
        private static ILoggerFactory innerFactory = null;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if(innerFactory == null)
                {
                    // _factory = new LoggerFactory();
                    // throw new ConfigurationErrorsException(string.Format($"{typeof(LoggingContext)} class was not initialized"));
                }

                return innerFactory;
            }

            set => innerFactory = value;
        }

        public static void Initialize(ILoggerFactory factory)
        {
            innerFactory = factory;
        }

        public static ILogger CreateLogger<T>()
        {
            if (innerFactory == null)
            {
                // _factory = new LoggerFactory();
                // throw new ConfigurationErrorsException(string.Format($"{typeof(LoggingContext)} class was not initialized"));
            }

            return innerFactory.CreateLogger<T>();
        }

        public static ILogger CreateLogger(string categoryName)
        {
            if (innerFactory == null)
            {
                // _factory = new LoggerFactory();
                // throw new ConfigurationErrorsException(string.Format($"{typeof(LoggingContext)} class was not initialized"));
            }

            return innerFactory.CreateLogger(categoryName);
        }
    }
}
