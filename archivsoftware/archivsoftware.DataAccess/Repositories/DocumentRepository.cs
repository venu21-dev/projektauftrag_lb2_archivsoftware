using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace archivsoftware.DataAccess.Repositories
{
    public class DocumentRepository : IRepository<Document>
    {
        private readonly ArchiveContext _context;

        // Konstruktor
        public DocumentRepository(ArchiveContext context)
        {
            _context = context;
        }

        // Alle Dokumente laden
        public List<Document> GetAll()
        {
            return _context.Documents
                .Include(d => d.Folder)  // Lade den zugehörigen Ordner mit
                .ToList();
        }

        // Dokument nach ID laden
        public Document GetById(int id)
        {
            return _context.Documents
                .Include(d => d.Folder)
                .FirstOrDefault(d => d.Id == id);
        }

        // Neues Dokument hinzufügen
        public void Add(Document document)
        {
            _context.Documents.Add(document);
        }

        // Dokument aktualisieren
        public void Update(Document document)
        {
            _context.Documents.Update(document);
        }

        // Dokument löschen
        public void Delete(int id)
        {
            var document = GetById(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
            }
        }

        // Änderungen speichern
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        // BONUS: Spezielle Methoden nur für Documents

        // Alle Dokumente eines bestimmten Ordners
        public List<Document> GetByFolderId(int folderId)
        {
            return _context.Documents
                .Where(d => d.FolderId == folderId)
                .ToList();
        }

        // Volltextsuche über PlainText
        public List<Document> Search(string searchTerm)
        {
            return _context.Documents
                .Include(d => d.Folder)
                .Where(d => d.PlainText.Contains(searchTerm) ||
                           d.FileName.Contains(searchTerm))
                .ToList();
        }
    }
}