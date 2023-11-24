using IsometricShooterWebApp.Utils.Providers;
using System.Text;

namespace IsometricShooterWebApp.Utils
{
    public class MemoryLogger : ILogger
    {
        private readonly string _name;
        private readonly MemoryLoggerProvider loggerProvider;

        public MemoryLogger(string name, MemoryLoggerProvider loggerProvider)
        {
            _name = name;
            this.loggerProvider = loggerProvider;
        }

        public IDisposable? BeginScope<TState>(TState state) 
            where TState : notnull 
            => default!;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            StringBuilder sb = new StringBuilder();


            sb.AppendLine($"{DateTime.UtcNow} = [{eventId.Id,2}: {logLevel,-12}]");
            sb.Append($"     {_name} - ");
            sb.Append($"{formatter(state, exception)}");

            loggerProvider.EnqueueLog(sb.ToString());
        }
    }
}
