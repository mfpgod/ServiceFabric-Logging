﻿namespace Adapters.DocumentDbDependency
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Shared.Extensions.Logging.ApplicationInsights;
    using Shared.Logging;
    using Shared.ServiceFabric;

    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The call initializes TelemetryConfiguration that will create and Intialize modules.
                TelemetryConfiguration configuration = TelemetryConfiguration.Active;

                // LoggerFactory registration in container
                LoggerFactory loggerFactory = new LoggerFactory();
                loggerFactory.AddApplicationInsights(() => new TelemetryClient(), LogLevel.Trace);
                LoggingContext.Initialize(loggerFactory);

                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync(
                    "Adapters.DocumentDbDependencyType",
                    context => new DocumentDbDependency(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(DocumentDbDependency).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
