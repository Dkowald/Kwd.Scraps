using System;
using KeePassLib.Interfaces;
using Microsoft.Extensions.Logging;

namespace kwd.keepass
{   
    public class KeepassStatusLogger : IStatusLogger
    {
        private readonly ILogger _logger;

        public static IStatusLogger SelectLoggerStrategy<T>(KeepassConfigurationOptions options) =>
            SelectLoggerStrategy<T>(options?.Logger);

        public static IStatusLogger SelectLoggerStrategy<T>(ILoggerFactory loggerFactory) =>
            SelectLoggerStrategy(loggerFactory?.CreateLogger(typeof(T).FullName));

        public static IStatusLogger SelectLoggerStrategy(ILogger logger) => 
            logger != null ? (IStatusLogger)new KeepassStatusLogger(logger) : new NullStatusLogger();
        
        public KeepassStatusLogger(ILogger logger)
        {
            if (logger == null) { throw new ArgumentNullException(nameof(logger));}

            _logger = logger;
        }

        #region Implementation of IStatusLogger

        public void StartLogging(string strOperation, bool bWriteOperationToLog)
        {
            if(bWriteOperationToLog)
            { _logger?.LogInformation($"Starting {strOperation}");}
        }

        public void EndLogging()
        {
        }

        public bool SetProgress(uint uPercent)
        {
            _logger.LogInformation($"Progress : {uPercent}%");
            return true;
        }

        public bool SetText(string strNewText, LogStatusType lsType)
        {
            switch (lsType)
            {
                case LogStatusType.Error:
                _logger?.LogError(strNewText);
                break;
                    
                case LogStatusType.Warning:
                _logger?.LogWarning(strNewText);
                break;

                case LogStatusType.Info:
                _logger?.LogInformation(strNewText);
                break;

                case LogStatusType.AdditionalInfo:
                _logger?.LogDebug(strNewText);
                break;
            }

            return true;
        }

        public bool ContinueWork() => true;

        #endregion
    }
}
