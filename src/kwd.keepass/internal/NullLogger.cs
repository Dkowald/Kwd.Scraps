using System;
using Microsoft.Extensions.Logging;

namespace kwd.keepass
{
    /// <summary>
    /// No-op logger.
    /// </summary>
    /// <remarks>
    /// todo: next version of dotnet has inbuilt NullLogger.
    /// </remarks>
    internal class NullLogger : ILogger
    {
        #region Implementation of ILogger

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        { }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Scope();
        }

        #endregion

        private class Scope : IDisposable
        {
            public void Dispose() { }
        }
    }
}
