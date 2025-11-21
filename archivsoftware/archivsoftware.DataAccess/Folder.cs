using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace archivsoftware.DataAccess
{
    public class Folder
    {
        // Primärschlüssel
        public int Id { get; set; }

        // Ordnername
        public string Name { get; set; } = string.Empty;

        // Hierarchie: Parent Folder (NULL = Root-Ordner)
        public int? ParentFolderId { get; set; }

        // Erstellungsdatum
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Property: Verweis auf Parent-Ordner
        public Folder? ParentFolder { get; set; }

        // Navigation Property: Liste der Unterordner
        public List<Folder> SubFolders { get; set; } = new List<Folder>();

        // Navigation Property: Liste der Dokumente in diesem Ordner
        public List<Document> Documents { get; set; } = new List<Document>();
    }
}
