using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace archivsoftware.DataAccess.Repositories
{
    public class FolderRepository : IRepository<Folder>
    {
        private readonly ArchiveContext _context;

        // Konstruktor - bekommt den Context übergeben
        public FolderRepository(ArchiveContext context)
        {
            _context = context;
        }

        // Alle Ordner mit ihren Beziehungen laden
        public List<Folder> GetAll()
        {
            return _context.Folders
                .Include(f => f.ParentFolder)      // Lade Parent-Ordner mit
                .Include(f => f.SubFolders)        // Lade Unterordner mit
                .Include(f => f.Documents)         // Lade Dokumente mit
                .ToList();
        }

        // Ordner nach ID laden
        public Folder GetById(int id)
        {
            return _context.Folders
                .Include(f => f.ParentFolder)
                .Include(f => f.SubFolders)
                .Include(f => f.Documents)
                .FirstOrDefault(f => f.Id == id);
        }

        // Neuen Ordner hinzufügen
        public void Add(Folder folder)
        {
            _context.Folders.Add(folder);
        }

        // Ordner aktualisieren
        public void Update(Folder folder)
        {
            _context.Folders.Update(folder);
        }

        // Ordner löschen
        public void Delete(int id)
        {
            var folder = GetById(id);
            if (folder != null)
            {
                _context.Folders.Remove(folder);
            }
        }

        // Änderungen speichern
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        // BONUS: Spezielle Methoden nur für Folders

        // Root-Ordner laden (die ohne Parent)
        public List<Folder> GetRootFolders()
        {
            return _context.Folders
                .Where(f => f.ParentFolderId == null)
                .Include(f => f.SubFolders)
                .ToList();
        }

        // Unterordner eines bestimmten Ordners laden
        public List<Folder> GetSubFolders(int parentId)
        {
            return _context.Folders
                .Where(f => f.ParentFolderId == parentId)
                .ToList();
        }
    }
}