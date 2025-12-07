using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace archivsoftware.Business.Models
{
    /// <summary>
    /// Einstellungen für den FileSystemWatcher
    /// </summary>
    public class WatcherSettings
    {
        /// <summary>
        /// Pfad zum überwachten Ordner
        /// </summary>
        public string WatchPath { get; set; }

        /// <summary>
        /// Ist der Watcher aktiv?
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Ziel-Ordner ID in der Datenbank (wohin importiert wird)
        /// </summary>
        public int TargetFolderId { get; set; }

        /// <summary>
        /// Was passiert nach erfolgreichem Import?
        /// </summary>
        public ImportAction AfterImport { get; set; }

        public WatcherSettings()
        {
            WatchPath = string.Empty;
            IsEnabled = false;
            TargetFolderId = 0;
            AfterImport = ImportAction.Delete;
        }
    }

    /// <summary>
    /// Aktion nach erfolgreichem Import
    /// </summary>
    public enum ImportAction
    {
        /// <summary>
        /// Datei löschen nach Import
        /// </summary>
        Delete,

        /// <summary>
        /// Datei in Unterordner "Imported" verschieben
        /// </summary>
        MoveToImported,

        /// <summary>
        /// Datei im Ordner lassen
        /// </summary>
        KeepInPlace
    }
}