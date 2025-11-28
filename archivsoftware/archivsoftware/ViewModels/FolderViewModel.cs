using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using archivsoftware.DataAccess;

namespace archivsoftware.ViewModels
{
    /// <summary>
    /// ViewModel für die Darstellung von Folders im TreeView
    /// </summary>
    public class FolderViewModel
    {
        // Die eigentliche Folder-Entity aus der Datenbank
        public Folder Folder { get; set; }

        // Name für die Anzeige
        public string DisplayName => Folder.Name;

        // ID für Zugriff
        public int Id => Folder.Id;

        // Unterordner (hierarchisch)
        public ObservableCollection<FolderViewModel> Children { get; set; }

        // Konstruktor
        public FolderViewModel(Folder folder)
        {
            Folder = folder;
            Children = new ObservableCollection<FolderViewModel>();
        }

        // Hilfsmethode: Unterordner hinzufügen
        public void AddChild(FolderViewModel child)
        {
            Children.Add(child);
        }
    }
}