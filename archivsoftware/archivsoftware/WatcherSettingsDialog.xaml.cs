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
using archivsoftware.Business.Models;
using archivsoftware.DataAccess;
using archivsoftware.DataAccess.Repositories;

namespace archivsoftware
{
    public partial class WatcherSettingsDialog : Window
    {
        public WatcherSettings Settings { get; private set; }
        private FolderRepository _folderRepository;

        public WatcherSettingsDialog(WatcherSettings currentSettings, ArchiveContext context)
        {
            InitializeComponent();

            _folderRepository = new FolderRepository(context);
            Settings = currentSettings;

            // Ordner-Liste laden
            LoadFolders();

            // Aktuelle Einstellungen anzeigen
            WatchPathTextBox.Text = Settings.WatchPath;
            AfterImportComboBox.SelectedIndex = (int)Settings.AfterImport;

            // Ziel-Ordner vorauswählen
            if (Settings.TargetFolderId > 0)
            {
                var item = TargetFolderComboBox.Items
                    .Cast<FolderComboBoxItem>()
                    .FirstOrDefault(i => i.FolderId == Settings.TargetFolderId);

                if (item != null)
                    TargetFolderComboBox.SelectedItem = item;
            }
        }

        /// <summary>
        /// Lädt alle Ordner in die ComboBox
        /// </summary>
        private void LoadFolders()
        {
            try
            {
                var allFolders = _folderRepository.GetAll();

                // ComboBox leeren
                TargetFolderComboBox.Items.Clear();

                // Alle Ordner hinzufügen (flache Liste)
                foreach (var folder in allFolders.OrderBy(f => f.Name))
                {
                    TargetFolderComboBox.Items.Add(new FolderComboBoxItem
                    {
                        FolderId = folder.Id,
                        FolderName = folder.Name
                    });
                }

                if (TargetFolderComboBox.Items.Count > 0)
                    TargetFolderComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Ordner: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Button: Ordner durchsuchen
        /// </summary>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Wählen Sie einen Ordner aus",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Ordner auswählen"
            };

            if (dialog.ShowDialog() == true)
            {
                // Extrahiere den Ordner-Pfad aus dem ausgewählten Pfad
                string selectedPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    WatchPathTextBox.Text = selectedPath;
                }
            }
        }

        /// <summary>
        /// Button: Speichern
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validierung
            if (string.IsNullOrWhiteSpace(WatchPathTextBox.Text))
            {
                MessageBox.Show("Bitte wählen Sie einen Überwachungs-Ordner aus.",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!System.IO.Directory.Exists(WatchPathTextBox.Text))
            {
                MessageBox.Show("Der ausgewählte Ordner existiert nicht.",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TargetFolderComboBox.SelectedItem == null)
            {
                MessageBox.Show("Bitte wählen Sie einen Ziel-Ordner aus.",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Einstellungen speichern
            Settings.WatchPath = WatchPathTextBox.Text;
            Settings.TargetFolderId = ((FolderComboBoxItem)TargetFolderComboBox.SelectedItem).FolderId;
            Settings.AfterImport = (ImportAction)AfterImportComboBox.SelectedIndex;
            Settings.IsEnabled = true;

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Button: Abbrechen
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AfterImportComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    /// <summary>
    /// Helper-Klasse für ComboBox-Items
    /// </summary>
    public class FolderComboBoxItem
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; }

        public override string ToString()
        {
            return FolderName;
        }
    }
}