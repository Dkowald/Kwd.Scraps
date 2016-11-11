using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace kwd_keepass.tests.TestHelpers
{
    public class TestLoggerProvider : ILoggerProvider
    {
        public void Dispose() { }

        public List<Item> Entries = new List<Item>();

        public ILogger CreateLogger(string categoryName) => new TestLogger(this, categoryName);
        
        public class TestLogger : ILogger
        {
            private readonly TestLoggerProvider _provider;
            public readonly string Name;

            public TestLogger(TestLoggerProvider provider, string name)
            {
                if (provider == null) { throw new ArgumentNullException(nameof(provider)); }

                _provider = provider;
                Name = name;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _provider.Entries.Add(new Item
                {
                    Name = Name,
                    logLevel = logLevel,
                    eventId = eventId,
                    state = state,
                    exception = exception
                });
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }
        }

        public class Item
        {
            public string Name;

            public LogLevel logLevel;
            public EventId eventId;
            public object state;
            public Exception exception;

            public string Msg() => state?.ToString();
        }
    }
}
