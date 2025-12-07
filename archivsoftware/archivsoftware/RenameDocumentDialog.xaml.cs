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
using System.IO;
using archivsoftware.DataAccess;

namespace archivsoftware
{
    public partial class RenameDocumentDialog : Window
    {
        private readonly Document _document;
        private readonly ArchiveContext _context;

        public string NewFileName { get; private set; }

        public RenameDocumentDialog(Document document, ArchiveContext context)
        {
            InitializeComponent();

            _document = document;
            _context = context;

            // Aktuellen Namen anzeigen
            CurrentNameText.Text = document.FileName;

            // Nur den Namen ohne Endung ins TextBox setzen
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(document.FileName);
            NewNameTextBox.Text = nameWithoutExtension;
            NewNameTextBox.SelectAll();
            NewNameTextBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string newName = NewNameTextBox.Text.Trim();

            // Validierung: Nicht leer
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show(
                    "Bitte geben Sie einen Namen ein.",
                    "Ungültige Eingabe",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Dateiendung behalten
            string extension = Path.GetExtension(_document.FileName);
            NewFileName = newName + extension;

            // Validierung: Kein Duplikat im selben Ordner
            bool isDuplicate = _context.Documents.Any(d =>
                d.FolderId == _document.FolderId &&
                d.FileName == NewFileName &&
                d.DocumentId != _document.DocumentId
            );

            if (isDuplicate)
            {
                MessageBox.Show(
                    $"Ein Dokument mit dem Namen '{NewFileName}' existiert bereits in diesem Ordner.",
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
    }
}