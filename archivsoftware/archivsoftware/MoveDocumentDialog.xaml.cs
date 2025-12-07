using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using archivsoftware.DataAccess;


namespace archivsoftware
{
    public partial class MoveDocumentDialog : Window
    {
        private readonly Document _document;
        private readonly ArchiveContext _context;

        public int TargetFolderId { get; private set; }

        public MoveDocumentDialog(Document document, ArchiveContext context)
        {
            InitializeComponent();

            _document = document;
            _context = context;

            // Dokument-Name anzeigen
            DocumentNameText.Text = document.FileName;

            // Aktuellen Ordner anzeigen
            var currentFolder = _context.Folders.FirstOrDefault(f => f.Id == document.FolderId);
            CurrentFolderText.Text = currentFolder?.Name ?? "Unbekannt";

            // Alle Ordner laden (außer aktuellem)
            LoadFolders();
        }

        private void LoadFolders()
        {
            // Alle Ordner aus DB holen
            var allFolders = _context.Folders
                .OrderBy(f => f.Name)
                .ToList();

            // Helper-Klasse für ComboBox Items
            var folderItems = allFolders
                .Where(f => f.Id != _document.FolderId) // Aktuellen Ordner ausschließen
                .Select(f => new FolderComboBoxItem
                {
                    FolderId = f.Id,
                    FolderName = GetFolderPath(f.Id, allFolders)
                })
                .OrderBy(f => f.FolderName)
                .ToList();

            TargetFolderComboBox.ItemsSource = folderItems;
            TargetFolderComboBox.DisplayMemberPath = "FolderName";

            if (folderItems.Any())
            {
                TargetFolderComboBox.SelectedIndex = 0;
            }
        }

        private string GetFolderPath(int folderId, List<Folder> allFolders)
        {
            var folder = allFolders.FirstOrDefault(f => f.Id == folderId);
            if (folder == null) return "";

            // Hierarchischen Pfad aufbauen
            var pathParts = new List<string>();
            var current = folder;

            while (current != null)
            {
                pathParts.Insert(0, current.Name);
                current = allFolders.FirstOrDefault(f => f.Id == current.ParentFolderId);
            }

            return string.Join(" / ", pathParts);
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validierung: Ziel-Ordner ausgewählt?
            if (TargetFolderComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Bitte wählen Sie einen Ziel-Ordner aus.",
                    "Kein Ordner ausgewählt",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var selectedItem = (FolderComboBoxItem)TargetFolderComboBox.SelectedItem;
            TargetFolderId = selectedItem.FolderId;

            // Validierung: Duplikat im Zielordner?
            bool isDuplicate = _context.Documents.Any(d =>
                d.FolderId == TargetFolderId &&
                d.FileName == _document.FileName &&
                d.Id != _document.Id  // ← Id statt DocumentId!
            );

            if (isDuplicate)
            {
                MessageBox.Show(
                    $"Ein Dokument mit dem Namen '{_document.FileName}' existiert bereits im Zielordner.",
                    "Duplikat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Alles OK → Dialog schließen
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Helper-Klasse für ComboBox Items
        private class FolderComboBoxItem
        {
            public int FolderId { get; set; }
            public string FolderName { get; set; }
        }
    }
}