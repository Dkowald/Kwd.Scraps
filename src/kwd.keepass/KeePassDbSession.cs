using System;
using KeePassLib;
using KeePassLib.Interfaces;

namespace kwd.keepass
{
    /// <summary>
    /// Provide a using-wrapper to auto-close and optionally auto-save a keepass db.
    /// </summary>
    /// <remarks>
    /// Use cast to access Db.
    /// </remarks>
    public class KeePassDbSession : IDisposable
    {
        private readonly PwDatabase _db;
        private readonly IStatusLogger _logger;

        public KeePassDbSession(IStatusLogger logger, PwDatabase db, bool saveOnClose = true)
        {
            if (logger == null) { throw new ArgumentNullException(nameof(logger));}
            if (db == null) { throw new ArgumentNullException(nameof(db));}

            _db = db;
            _logger = logger;
            SaveOnClose = saveOnClose;
        }

        public bool SaveOnClose { get; private set; }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_db.IsOpen)
            {
                if (SaveOnClose) { _db.Save(_logger); }

                _db.Close();

                _logger.SetText("Database closed", LogStatusType.AdditionalInfo);
            }
        }

        #endregion

        public PwDatabase Db => _db;

        public static explicit operator PwDatabase(KeePassDbSession self) { return self._db; }
    }
}
