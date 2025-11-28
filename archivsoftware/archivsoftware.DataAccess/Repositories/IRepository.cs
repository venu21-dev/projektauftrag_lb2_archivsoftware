using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archivsoftware.DataAccess.Repositories
{
    /// <summary>
    /// Generisches Interface für alle Repositories
    /// T = Der Entity-Typ (z.B. Folder oder Document)
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // Alle Einträge abrufen
        List<T> GetAll();

        // Eintrag nach ID abrufen
        T GetById(int id);

        // Neuen Eintrag hinzufügen
        void Add(T entity);

        // Eintrag aktualisieren
        void Update(T entity);

        // Eintrag löschen
        void Delete(int id);

        // Änderungen in der Datenbank speichern
        void SaveChanges();
    }
}