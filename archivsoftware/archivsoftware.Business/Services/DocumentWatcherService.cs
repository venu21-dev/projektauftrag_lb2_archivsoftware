using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using archivsoftware.Business.Models;

namespace archivsoftware.Business.Services
{
    /// <summary>
    /// Service für automatischen Dokument-Import via FileSystemWatcher
    /// </summary>
    public class DocumentWatcherService : IDisposable
    {
        private FileSystemWatcher _watcher;
        private WatcherSettings _settings;

        // Events für GUI-Feedback
        public event EventHandler<DocumentImportedEventArgs> DocumentImported;
        public event EventHandler<ImportErrorEventArgs> ImportError;

        public bool IsRunning => _watcher?.EnableRaisingEvents ?? false;

        /// <summary>
        /// Startet den Watcher
        /// </summary>
        public void Start(WatcherSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(settings.WatchPath))
                throw new ArgumentException("Watch-Pfad darf nicht leer sein.");

            if (!Directory.Exists(settings.WatchPath))
                throw new DirectoryNotFoundException($"Ordner nicht gefunden: {settings.WatchPath}");

            // Stop falls bereits läuft
            Stop();

            _settings = settings;

            // FileSystemWatcher konfigurieren
            _watcher = new FileSystemWatcher
            {
                Path = settings.WatchPath,
                Filter = "*.*", // Alle Dateien, wir filtern später
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            // Events abonnieren
            _watcher.Created += OnFileCreated;
            _watcher.Error += OnError;
        }

        /// <summary>
        /// Stoppt den Watcher
        /// </summary>
        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Created -= OnFileCreated;
                _watcher.Error -= OnError;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        /// <summary>
        /// Wird aufgerufen wenn eine neue Datei erstellt wurde
        /// </summary>
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            string filePath = e.FullPath;
            string extension = Path.GetExtension(filePath).ToLower();

            // Nur PDF und DOCX
            if (extension != ".pdf" && extension != ".docx")
                return;

            // Warte kurz, damit Datei komplett geschrieben ist
            System.Threading.Thread.Sleep(500);

            // Prüfe ob Datei noch existiert und zugreifbar ist
            if (!IsFileReady(filePath))
            {
                OnImportError(filePath, "Datei ist nicht bereit oder wurde bereits verarbeitet.");
                return;
            }

            try
            {
                // Event auslösen → GUI importiert die Datei
                OnDocumentImported(filePath);
            }
            catch (Exception ex)
            {
                OnImportError(filePath, ex.Message);
            }
        }

        /// <summary>
        /// Prüft ob eine Datei bereit zum Lesen ist
        /// </summary>
        private bool IsFileReady(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                // Versuche Datei zu öffnen (exklusiv)
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return stream.Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Wird aufgerufen bei Watcher-Fehlern
        /// </summary>
        private void OnError(object sender, ErrorEventArgs e)
        {
            OnImportError("FileSystemWatcher", e.GetException()?.Message ?? "Unbekannter Fehler");
        }

        /// <summary>
        /// Löst DocumentImported Event aus
        /// </summary>
        protected virtual void OnDocumentImported(string filePath)
        {
            DocumentImported?.Invoke(this, new DocumentImportedEventArgs
            {
                FilePath = filePath,
                TargetFolderId = _settings.TargetFolderId,
                AfterImport = _settings.AfterImport
            });
        }

        /// <summary>
        /// Löst ImportError Event aus
        /// </summary>
        protected virtual void OnImportError(string filePath, string error)
        {
            ImportError?.Invoke(this, new ImportErrorEventArgs
            {
                FilePath = filePath,
                ErrorMessage = error
            });
        }

        public void Dispose()
        {
            Stop();
        }
    }

    /// <summary>
    /// Event-Args für erfolgreichen Import
    /// </summary>
    public class DocumentImportedEventArgs : EventArgs
    {
        public string FilePath { get; set; }
        public int TargetFolderId { get; set; }
        public ImportAction AfterImport { get; set; }
    }

    /// <summary>
    /// Event-Args für Import-Fehler
    /// </summary>
    public class ImportErrorEventArgs : EventArgs
    {
        public string FilePath { get; set; }
        public string ErrorMessage { get; set; }
    }
}
