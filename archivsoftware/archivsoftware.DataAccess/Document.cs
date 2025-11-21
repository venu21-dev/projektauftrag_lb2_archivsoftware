using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archivsoftware.DataAccess
{
    public class Document
    {
        // Primärschlüssel
        public int Id { get; set; }

        // Dateiname (z.B. "Rechnung.pdf")
        public string FileName { get; set; } = string.Empty;

        // Dateityp ("PDF" oder "DOCX")
        public string FileType { get; set; } = string.Empty;

        // BLOB - Binärdaten der Datei
        public byte[] FileData { get; set; } = Array.Empty<byte>(); // ← Heißt "FileData" statt "FileContent"

        // Extrahierter Text für Volltextsuche
        public string PlainText { get; set; } = string.Empty;

        // Dateigröße in Bytes
        public long FileSize { get; set; }

        // Import-Zeitpunkt
        public DateTime ImportedAt { get; set; } = DateTime.Now; // ← Heißt "ImportedAt" statt "UploadedAt"

        // Fremdschlüssel: Zu welchem Ordner gehört das Dokument?
        public int FolderId { get; set; }

        // Navigation Property: Verweis auf den Ordner
        public Folder Folder { get; set; } = null!;
    }
}