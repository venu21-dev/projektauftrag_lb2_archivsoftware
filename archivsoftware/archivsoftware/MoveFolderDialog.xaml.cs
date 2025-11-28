using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using archivsoftware.ViewModels;

namespace archivsoftware
{
    public partial class MoveFolderDialog : Window
    {
        public int? SelectedParentId { get; private set; }
        private int _currentFolderId;

        public MoveFolderDialog(ObservableCollection<FolderViewModel> allFolders, int currentFolderId, int? currentParentId)
        {
            InitializeComponent();

            _currentFolderId = currentFolderId;
            SelectedParentId = null;

            // Alle Ordner außer dem zu verschiebenden und seinen Unterordnern anzeigen
            var availableFolders = FilterFolders(allFolders, currentFolderId);
            FoldersListView.ItemsSource = availableFolders;
        }

        /// <summary>
        /// Filtert Ordner: Entfernt den zu verschiebenden Ordner und alle seine Unterordner
        /// </summary>
        private ObservableCollection<FolderViewModel> FilterFolders(ObservableCollection<FolderViewModel> folders, int excludeId)
        {
            var filtered = new ObservableCollection<FolderViewModel>();

            foreach (var folder in folders)
            {
                if (folder.Id != excludeId && !IsDescendant(folder, excludeId))
                {
                    var folderCopy = new FolderViewModel(folder.Folder);

                    // Rekursiv Unterordner filtern
                    if (folder.Children.Count > 0)
                    {
                        var filteredChildren = FilterFolders(folder.Children, excludeId);
                        foreach (var child in filteredChildren)
                        {
                            folderCopy.AddChild(child);
                        }
                    }

                    filtered.Add(folderCopy);
                }
            }

            return filtered;
        }

        /// <summary>
        /// Prüft ob ein Ordner ein Nachfahre des zu verschiebenden Ordners ist
        /// </summary>
        private bool IsDescendant(FolderViewModel folder, int ancestorId)
        {
            if (folder.Id == ancestorId)
                return true;

            foreach (var child in folder.Children)
            {
                if (IsDescendant(child, ancestorId))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Root-Ebene ausgewählt
        /// </summary>
        private void RootOption_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedParentId = null;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Ordner Border angeklickt
        /// </summary>
        private void FolderBorder_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag != null)
            {
                SelectedParentId = (int)border.Tag;
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}