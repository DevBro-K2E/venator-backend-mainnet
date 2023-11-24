using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace IsometricShooterWebApp.Utils.Providers
{
    public sealed class MemoryLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, MemoryLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public MemoryLoggerProvider()
        {
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new MemoryLogger(name, this));



        private ConcurrentQueue<string> logs = new ConcurrentQueue<string>();


        public void EnqueueLog(string text)
        {
            while (logs.Count > 1000)
            {
                logs.TryDequeue(out _);
            }

            logs.Enqueue(text);
        }

        public IEnumerable<string> GetLogs()
            => logs.ToArray();


        public void Dispose()
        {
            _loggers.Clear();
        }
    }


    public static class MLPExtensions
    {
        public static ILoggingBuilder AddMemoryLogger(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, MemoryLoggerProvider>());

            //LoggerProviderOptions.RegisterProviderOptions
            //    <ColorConsoleLoggerConfiguration, MemoryLoggerProvider>(builder.Services);

            return builder;
        }
    }
}
