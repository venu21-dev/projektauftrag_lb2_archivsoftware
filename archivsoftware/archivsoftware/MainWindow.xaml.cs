using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using archivsoftware.Business.Models;
using archivsoftware.Business.Services;
using archivsoftware.DataAccess;
using archivsoftware.DataAccess.Repositories;
using archivsoftware.ViewModels;


namespace archivsoftware
{

    public partial class MainWindow : Window
    {
        // Repositories
        private readonly ArchiveContext _context;
        private readonly FolderRepository _folderRepository;
        private readonly DocumentRepository _documentRepository;

        // Folder Data für TreeView
        private ObservableCollection<FolderViewModel> _folderTree;

        // FileSystemWatcher Service
        private DocumentWatcherService _watcherService;
        private WatcherSettings _watcherSettings;

        public MainWindow()
        {
            InitializeComponent();

            // Context und Repositories initialisieren
            _context = new ArchiveContext();
            _folderRepository = new FolderRepository(_context);
            _documentRepository = new DocumentRepository(_context);

            // Daten laden
            LoadFolderTree();

            // FileSystemWatcher initialisieren
            InitializeWatcher();
        }

        /// <summary>
        /// Such-Placeholder ausblenden/einblenden
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        /// <summary>
        /// Lädt die Ordnerstruktur aus der Datenbank (hierarchisch für TreeView)
        /// </summary>
        private void LoadFolderTree()
        {
            try
            {
                // Alle Root-Ordner (ohne Parent) aus der DB laden
                var rootFolders = _folderRepository.GetRootFolders();

                // In ViewModels umwandeln
                _folderTree = new ObservableCollection<FolderViewModel>();

                foreach (var folder in rootFolders)
                {
                    var folderVM = new FolderViewModel(folder);
                    LoadSubFolders(folderVM); // Rekursiv Unterordner laden
                    _folderTree.Add(folderVM);
                }

                // An TreeView binden
                FolderTreeView.ItemsSource = _folderTree;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Ordnerstruktur: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Rekursiv Unterordner laden
        /// </summary>
        private void LoadSubFolders(FolderViewModel parentVM)
        {
            var subFolders = _folderRepository.GetSubFolders(parentVM.Id);

            foreach (var subFolder in subFolders)
            {
                var subFolderVM = new FolderViewModel(subFolder);
                LoadSubFolders(subFolderVM); // Rekursion für weitere Ebenen
                parentVM.AddChild(subFolderVM);
            }
        }

        /// <summary>
        /// Window schließen - Context aufräumen
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Watcher stoppen
            _watcherService?.Stop();
            _watcherService?.Dispose();

            // Context aufräumen
            _context?.Dispose();

            base.OnClosed(e);
        }

        /// <summary>
        /// Button: Neuer Ordner erstellen
        /// </summary>
        private void BtnNewFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Dialog öffnen
                var dialog = new CreateFolderDialog();
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    string folderName = dialog.FolderName;

                    // Validierung: Name eindeutig auf Root-Ebene?
                    var existingFolder = _folderRepository.GetAll()
                        .FirstOrDefault(f => f.Name == folderName && f.ParentFolderId == null);

                    if (existingFolder != null)
                    {
                        MessageBox.Show($"Ein Ordner mit dem Namen '{folderName}' existiert bereits auf der Root-Ebene.",
                            "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Neuen Ordner erstellen
                    var newFolder = new Folder
                    {
                        Name = folderName,
                        ParentFolderId = null, // Root-Ordner
                        CreatedAt = DateTime.Now
                    };

                    _folderRepository.Add(newFolder);
                    _folderRepository.SaveChanges();

                    // TreeView aktualisieren
                    LoadFolderTree();

                    MessageBox.Show($"Ordner '{folderName}' wurde erfolgreich erstellt!",
                        "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Erstellen des Ordners: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Speichert den aktuell ausgewählten Ordner für ContextMenu
        private FolderViewModel _selectedFolder;

        /// <summary>
        /// Rechtsklick auf TreeView - Ordner merken für ContextMenu
        /// </summary>
        private void FolderTreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Finde das geklickte TreeViewItem
            var treeViewItem = FindVisualParent<TreeViewItem>((DependencyObject)e.OriginalSource);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                _selectedFolder = treeViewItem.DataContext as FolderViewModel;

                // ContextMenu anzeigen
                if (_selectedFolder != null)
                {
                    var contextMenu = (ContextMenu)FolderTreeView.FindResource("FolderContextMenu");
                    contextMenu.PlacementTarget = treeViewItem;
                    contextMenu.IsOpen = true;
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// TreeView: Ordner wurde ausgewählt (Linksklick)
        /// </summary>
        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FolderViewModel selectedFolder)
            {
                _selectedFolder = selectedFolder;

                // Dokumente des ausgewählten Ordners laden
                LoadDocuments(selectedFolder.Id);
            }
            else
            {
                // Kein Ordner ausgewählt → Liste leeren
                _selectedFolder = null;
                ResultsPanel.Children.Clear();
                ResultCountText.Text = "0 Dokumente";

                // Vorschau leeren
                MetadataPanel.Children.Clear();
                PreviewText.Text = "";
            }
        }

        /// <summary>
        /// ContextMenu: Umbenennen geklickt
        /// </summary>
        private void MenuItemRename_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFolder == null) return;

            try
            {
                // Dialog mit aktuellem Namen öffnen
                var dialog = new RenameFolderDialog(_selectedFolder.DisplayName);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    string newName = dialog.NewFolderName;

                    // Gleicher Name? Nichts tun
                    if (newName == _selectedFolder.DisplayName)
                    {
                        return;
                    }

                    // Validierung: Name eindeutig auf gleicher Ebene?
                    var parentId = _selectedFolder.Folder.ParentFolderId;
                    var existingFolder = _folderRepository.GetAll()
                        .FirstOrDefault(f => f.Name == newName &&
                                             f.ParentFolderId == parentId &&
                                             f.Id != _selectedFolder.Id);

                    if (existingFolder != null)
                    {
                        string level = parentId == null ? "Root-Ebene" : "dieser Ebene";
                        MessageBox.Show($"Ein Ordner mit dem Namen '{newName}' existiert bereits auf {level}.",
                            "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Ordner umbenennen
                    var folder = _folderRepository.GetById(_selectedFolder.Id);
                    folder.Name = newName;
                    _folderRepository.Update(folder);
                    _folderRepository.SaveChanges();

                    // TreeView aktualisieren
                    LoadFolderTree();

                    MessageBox.Show($"Ordner wurde erfolgreich in '{newName}' umbenannt!",
                        "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Umbenennen: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ContextMenu: Löschen geklickt
        /// </summary>
        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFolder == null) return;

            try
            {
                // Folder aus DB laden (mit allen Beziehungen)
                var folder = _folderRepository.GetById(_selectedFolder.Id);

                if (folder == null)
                {
                    MessageBox.Show("Der Ordner konnte nicht gefunden werden.",
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Zähle Unterordner und Dokumente
                int subFolderCount = _folderRepository.GetSubFolders(folder.Id).Count;
                int documentCount = _documentRepository.GetByFolderId(folder.Id).Count;

                // Warnung erstellen
                string warningMessage = $"Möchten Sie den Ordner '{folder.Name}' wirklich löschen?";

                if (subFolderCount > 0 || documentCount > 0)
                {
                    warningMessage += "\n\n⚠️ ACHTUNG:\n";

                    if (subFolderCount > 0)
                        warningMessage += $"• {subFolderCount} Unterordner\n";

                    if (documentCount > 0)
                        warningMessage += $"• {documentCount} Dokument(e)\n";

                    warningMessage += "\nwerden ebenfalls unwiderruflich gelöscht!";
                }

                // Bestätigungs-Dialog
                var result = MessageBox.Show(
                    warningMessage,
                    "Ordner löschen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No
                );

                if (result == MessageBoxResult.Yes)
                {
                    // Rekursiv löschen (Cascade Delete ist in DB konfiguriert)
                    DeleteFolderRecursive(folder.Id);

                    // TreeView aktualisieren
                    LoadFolderTree();

                    MessageBox.Show($"Ordner '{folder.Name}' wurde erfolgreich gelöscht!",
                        "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Löschen des Ordners: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Löscht einen Ordner rekursiv (alle Unterordner und Dokumente)
        /// </summary>
        private void DeleteFolderRecursive(int folderId)
        {
            // 1. Alle Unterordner rekursiv löschen
            var subFolders = _folderRepository.GetSubFolders(folderId);
            foreach (var subFolder in subFolders)
            {
                DeleteFolderRecursive(subFolder.Id);
            }

            // 2. Alle Dokumente in diesem Ordner löschen
            var documents = _documentRepository.GetByFolderId(folderId);
            foreach (var document in documents)
            {
                _documentRepository.Delete(document.Id);
            }

            // 3. Den Ordner selbst löschen
            _folderRepository.Delete(folderId);
            _folderRepository.SaveChanges();
        }

        /// <summary>
        /// Hilfsmethode: Findet Parent-Element im Visual Tree
        /// </summary>
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;

            return FindVisualParent<T>(parentObject);
        }

        /// <summary>
        /// ContextMenu: Verschieben geklickt
        /// </summary>
        private void MenuItemMove_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFolder == null) return;

            try
            {
                // Dialog öffnen mit allen verfügbaren Ordnern
                var dialog = new MoveFolderDialog(_folderTree, _selectedFolder.Id, _selectedFolder.Folder.ParentFolderId);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    int? newParentId = dialog.SelectedParentId;

                    // Gleicher Parent? Nichts tun
                    if (newParentId == _selectedFolder.Folder.ParentFolderId)
                    {
                        MessageBox.Show("Der Ordner befindet sich bereits an diesem Ort.",
                            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Validierung: Name eindeutig in Ziel-Ordner?
                    var existingFolder = _folderRepository.GetAll()
                        .FirstOrDefault(f => f.Name == _selectedFolder.DisplayName &&
                                             f.ParentFolderId == newParentId &&
                                             f.Id != _selectedFolder.Id);

                    if (existingFolder != null)
                    {
                        string location = newParentId == null ? "Root-Ebene" : "diesem Ordner";
                        MessageBox.Show($"Ein Ordner mit dem Namen '{_selectedFolder.DisplayName}' existiert bereits in {location}.",
                            "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Ordner verschieben
                    var folder = _folderRepository.GetById(_selectedFolder.Id);
                    folder.ParentFolderId = newParentId;
                    _folderRepository.Update(folder);
                    _folderRepository.SaveChanges();

                    // TreeView aktualisieren
                    LoadFolderTree();

                    string targetName = newParentId == null ? "Root-Ebene" :
                        _folderRepository.GetById(newParentId.Value).Name;

                    MessageBox.Show($"Ordner '{_selectedFolder.DisplayName}' wurde erfolgreich nach '{targetName}' verschoben!",
                        "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Verschieben: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ContextMenu: Neuer Unterordner erstellen
        /// </summary>
        private void MenuItemNewSubFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFolder == null) return;

            try
            {
                // Dialog öffnen
                var dialog = new CreateFolderDialog();
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    string folderName = dialog.FolderName;

                    // Validierung: Name eindeutig in diesem Ordner?
                    var existingFolder = _folderRepository.GetAll()
                        .FirstOrDefault(f => f.Name == folderName && f.ParentFolderId == _selectedFolder.Id);

                    if (existingFolder != null)
                    {
                        MessageBox.Show($"Ein Ordner mit dem Namen '{folderName}' existiert bereits in '{_selectedFolder.DisplayName}'.",
                            "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Neuen Unterordner erstellen
                    var newFolder = new Folder
                    {
                        Name = folderName,
                        ParentFolderId = _selectedFolder.Id, // Als Unterordner
                        CreatedAt = DateTime.Now
                    };

                    _folderRepository.Add(newFolder);
                    _folderRepository.SaveChanges();

                    // TreeView aktualisieren
                    LoadFolderTree();

                    MessageBox.Show($"Unterordner '{folderName}' wurde erfolgreich erstellt!",
                        "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Erstellen des Unterordners: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== TEST-METHODEN (temporär) ==========

        /// <summary>
        /// Test-Button Click Handler
        /// </summary>
        private void TestPdfButton_Click(object sender, RoutedEventArgs e)
        {
            TestPdfExtraction();
        }

        /// <summary>
        /// Test-Methode für PDF Text-Extraktion
        /// </summary>
        private void TestPdfExtraction()
        {
            try
            {
                var extractor = new archivsoftware.Business.Services.PdfTextExtractor();

                // Datei-Dialog öffnen
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "PDF Dateien (*.pdf)|*.pdf",
                    Title = "PDF zum Testen auswählen"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    // Text extrahieren
                    string extractedText = extractor.ExtractTextFromFile(filePath);

                    // Vorschau-Länge
                    int previewLength = Math.Min(500, extractedText.Length);
                    string preview = extractedText.Substring(0, previewLength);

                    // Ergebnis anzeigen
                    MessageBox.Show(
                        $"✅ PDF Text erfolgreich extrahiert!\n\n" +
                        $"Gesamtlänge: {extractedText.Length} Zeichen\n\n" +
                        $"Vorschau (erste {previewLength} Zeichen):\n" +
                        $"─────────────────────────────\n" +
                        $"{preview}...",
                        "PDF Text-Extraktion",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Fehler beim Extrahieren:\n\n{ex.Message}",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Test-Methode für DOCX Text-Extraktion
        /// </summary>
        private void TestDocxExtraction()
        {
            try
            {
                var extractor = new archivsoftware.Business.Services.DocxTextExtractor();

                // Datei-Dialog öffnen
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Word Dokumente (*.docx)|*.docx",
                    Title = "DOCX zum Testen auswählen"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    // Text extrahieren
                    string extractedText = extractor.ExtractTextFromFile(filePath);

                    // Vorschau-Länge
                    int previewLength = Math.Min(500, extractedText.Length);
                    string preview = extractedText.Substring(0, previewLength);

                    // Ergebnis anzeigen
                    MessageBox.Show(
                        $"✅ DOCX Text erfolgreich extrahiert!\n\n" +
                        $"Gesamtlänge: {extractedText.Length} Zeichen\n\n" +
                        $"Vorschau (erste {previewLength} Zeichen):\n" +
                        $"─────────────────────────────\n" +
                        $"{preview}...",
                        "DOCX Text-Extraktion",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Fehler beim Extrahieren:\n\n{ex.Message}",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void TestDocxButton_Click(object sender, RoutedEventArgs e)
        {
            TestDocxExtraction();
        }

        /// <summary>
        /// Button: Dokument importieren
        /// </summary>
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Prüfen ob ein Ordner ausgewählt ist
                if (_selectedFolder == null)
                {
                    MessageBox.Show(
                        "Bitte wählen Sie zuerst einen Ziel-Ordner aus, in den das Dokument importiert werden soll.\n\n" +
                        "Klicken Sie dazu auf einen Ordner im Ordnerbaum links.",
                        "Kein Ordner ausgewählt",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    return;
                }

                // Datei-Dialog öffnen
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Unterstützte Dokumente (*.pdf;*.docx)|*.pdf;*.docx|PDF Dateien (*.pdf)|*.pdf|Word Dokumente (*.docx)|*.docx",
                    Title = "Dokument zum Importieren auswählen",
                    Multiselect = true // Mehrere Dateien erlauben
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    int successCount = 0;
                    int errorCount = 0;
                    var errors = new StringBuilder();

                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        try
                        {
                            ImportDocument(filePath, _selectedFolder.Id);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errors.AppendLine($"• {Path.GetFileName(filePath)}: {ex.Message}");
                        }
                    }

                    // Ergebnis anzeigen
                    string message = $"Import abgeschlossen!\n\n" +
                                   $"✅ Erfolgreich: {successCount}\n";

                    if (errorCount > 0)
                    {
                        message += $"❌ Fehler: {errorCount}\n\n" +
                                  $"Details:\n{errors}";
                    }

                    MessageBox.Show(
                        message,
                        "Import-Ergebnis",
                        MessageBoxButton.OK,
                        errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information
                    );

                    // GUI aktualisieren - Dokumente neu laden
                    if (_selectedFolder != null)
                    {
                        LoadDocuments(_selectedFolder.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Fehler beim Import:\n\n{ex.Message}",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Importiert ein einzelnes Dokument in die Datenbank
        /// </summary>
        private void ImportDocument(string filePath, int folderId)
        {
            // 1. Datei-Informationen
            string fileName = Path.GetFileName(filePath);
            string fileExtension = Path.GetExtension(filePath).ToLower();
            byte[] fileData = File.ReadAllBytes(filePath);
            long fileSize = fileData.Length;

            // 2. Text extrahieren (je nach Dateityp)
            string plainText = string.Empty;

            if (fileExtension == ".pdf")
            {
                var pdfExtractor = new archivsoftware.Business.Services.PdfTextExtractor();

                // Validierung
                if (!pdfExtractor.IsValidPdf(fileData))
                {
                    throw new InvalidOperationException("Die Datei ist kein gültiges PDF-Dokument.");
                }

                plainText = pdfExtractor.ExtractText(fileData);
            }
            else if (fileExtension == ".docx")
            {
                var docxExtractor = new archivsoftware.Business.Services.DocxTextExtractor();

                // Validierung
                if (!docxExtractor.IsValidDocx(fileData))
                {
                    throw new InvalidOperationException("Die Datei ist kein gültiges DOCX-Dokument.");
                }

                plainText = docxExtractor.ExtractText(fileData);
            }
            else
            {
                throw new InvalidOperationException($"Dateityp '{fileExtension}' wird nicht unterstützt. Nur PDF und DOCX sind erlaubt.");
            }

            // 3. Prüfen ob Dokument bereits existiert (gleicher Name im gleichen Ordner)
            var existingDoc = _documentRepository.GetAll()
                .FirstOrDefault(d => d.FileName == fileName && d.FolderId == folderId);

            if (existingDoc != null)
            {
                var result = MessageBox.Show(
                    $"Ein Dokument mit dem Namen '{fileName}' existiert bereits in diesem Ordner.\n\n" +
                    "Möchten Sie es überschreiben?",
                    "Dokument existiert bereits",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    // Überschreiben
                    existingDoc.FileData = fileData;
                    existingDoc.PlainText = plainText;
                    existingDoc.FileSize = fileSize;
                    existingDoc.ImportedAt = DateTime.Now;

                    _documentRepository.Update(existingDoc);
                    _documentRepository.SaveChanges();
                    return;
                }
                else
                {
                    throw new InvalidOperationException("Import abgebrochen.");
                }
            }

            // 4. Neues Dokument erstellen
            var document = new Document
            {
                FileName = fileName,
                FileType = fileExtension,
                FileData = fileData,
                PlainText = plainText,
                FileSize = fileSize,
                FolderId = folderId,
                ImportedAt = DateTime.Now
            };

            // 5. In Datenbank speichern
            _documentRepository.Add(document);
            _documentRepository.SaveChanges();
        }

        /// <summary>
        /// Lädt und zeigt die Dokumente eines Ordners an - MIT KONTEXTMENÜ
        /// </summary>
        private void LoadDocuments(int folderId)
        {
            try
            {
                // Dokumente aus DB laden
                var documents = _documentRepository.GetByFolderId(folderId);

                // ResultsPanel leeren
                ResultsPanel.Children.Clear();

                // Treffer-Anzahl aktualisieren
                ResultCountText.Text = $"{documents.Count} Dokument(e)";

                if (documents.Count == 0)
                {
                    // Keine Dokumente vorhanden
                    var emptyText = new TextBlock
                    {
                        Text = "Keine Dokumente in diesem Ordner",
                        Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                        FontSize = 14,
                        Margin = new Thickness(20, 40, 20, 20),
                        TextAlignment = TextAlignment.Center
                    };
                    ResultsPanel.Children.Add(emptyText);
                    return;
                }

                // Dokumente anzeigen
                foreach (var document in documents.OrderByDescending(d => d.ImportedAt))
                {
                    var documentCard = CreateDocumentCard(document);
                    ResultsPanel.Children.Add(documentCard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Dokumente: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Erstellt eine Card für ein Dokument - MIT KONTEXTMENÜ
        /// </summary>
        private Border CreateDocumentCard(Document document)
        {
            // Haupt-Container
            var card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(42, 42, 42)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(10, 5, 10, 5),
                Cursor = Cursors.Hand,
                Tag = document  // ← WICHTIG: Dokument speichern für Kontextmenü!
            };

            // ← NEU: Kontextmenü zuweisen!
            card.ContextMenu = (ContextMenu)this.FindResource("DocumentContextMenu");

            // Layout
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            // Icon (basierend auf Dateityp)
            string icon = document.FileType.ToLower() == ".pdf" ? "📄" : "📝";
            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 24,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(iconText, 0);

            // Dokumenten-Info
            var infoStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };

            // Dateiname
            var nameText = new TextBlock
            {
                Text = document.FileName,
                Foreground = new SolidColorBrush(Color.FromRgb(232, 232, 232)),
                FontSize = 14,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(0, 0, 0, 4)
            };
            infoStack.Children.Add(nameText);

            // Metadaten (Größe und Datum)
            string fileSize = FormatFileSize(document.FileSize);
            string importDate = document.ImportedAt.ToString("dd.MM.yyyy HH:mm");

            var metaText = new TextBlock
            {
                Text = $"{fileSize} • {importDate}",
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                FontSize = 12
            };
            infoStack.Children.Add(metaText);

            Grid.SetColumn(infoStack, 1);

            // Badge: Dateityp
            var badge = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(13, 110, 253)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                VerticalAlignment = VerticalAlignment.Center
            };

            var badgeText = new TextBlock
            {
                Text = document.FileType.ToUpper().Replace(".", ""),
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
            badge.Child = badgeText;
            Grid.SetColumn(badge, 2);

            // Alles zusammenfügen
            grid.Children.Add(iconText);
            grid.Children.Add(infoStack);
            grid.Children.Add(badge);
            card.Child = grid;

            // Click-Event: Dokument anzeigen
            card.MouseLeftButtonDown += (s, e) => ShowDocumentPreview(document);

            // Hover-Effekt
            card.MouseEnter += (s, e) =>
            {
                card.Background = new SolidColorBrush(Color.FromRgb(36, 36, 36));
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(58, 58, 58));
            };
            card.MouseLeave += (s, e) =>
            {
                card.Background = new SolidColorBrush(Color.FromRgb(26, 26, 26));
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(42, 42, 42));
            };

            return card;
        }

        /// <summary>
        /// Formatiert Dateigröße (Bytes -> KB/MB)
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024} KB";
            else
                return $"{bytes / (1024 * 1024)} MB";
        }

        /// <summary>
        /// Zeigt Dokumenten-Vorschau an (rechte Spalte)
        /// </summary>
        private void ShowDocumentPreview(Document document)
        {
            try
            {
                // Metadaten anzeigen
                MetadataPanel.Children.Clear();

                AddMetadataRow("Dateiname:", document.FileName);
                AddMetadataRow("Typ:", document.FileType.ToUpper());
                AddMetadataRow("Größe:", FormatFileSize(document.FileSize));
                AddMetadataRow("Importiert:", document.ImportedAt.ToString("dd.MM.yyyy HH:mm"));

                // Text-Vorschau
                int previewLength = Math.Min(2000, document.PlainText?.Length ?? 0);
                PreviewText.Text = previewLength > 0
                    ? document.PlainText.Substring(0, previewLength) + (document.PlainText.Length > previewLength ? "..." : "")
                    : "Keine Textvorschau verfügbar.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Anzeigen der Vorschau: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Fügt eine Metadaten-Zeile hinzu
        /// </summary>
        private void AddMetadataRow(string label, string value)
        {
            var stack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var labelText = new TextBlock
            {
                Text = label,
                Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                FontSize = 12,
                Width = 80
            };

            var valueText = new TextBlock
            {
                Text = value,
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                FontSize = 12,
                FontWeight = FontWeights.Normal
            };

            stack.Children.Add(labelText);
            stack.Children.Add(valueText);
            MetadataPanel.Children.Add(stack);
        }

        /// <summary>
        /// Initialisiert den FileSystemWatcher
        /// </summary>
        private void InitializeWatcher()
        {
            _watcherService = new archivsoftware.Business.Services.DocumentWatcherService();

            // Events abonnieren
            _watcherService.DocumentImported += WatcherService_DocumentImported;
            _watcherService.ImportError += WatcherService_ImportError;

            // Einstellungen laden (später aus Datei/DB)
            _watcherSettings = new archivsoftware.Business.Models.WatcherSettings
            {
                IsEnabled = false, // Erstmal deaktiviert
                WatchPath = @"C:\ArchivImport", // Standard-Pfad
                TargetFolderId = 0, // Muss später gesetzt werden
                AfterImport = archivsoftware.Business.Models.ImportAction.Delete
            };
        }

        /// <summary>
        /// Event: Dokument wurde vom Watcher erkannt
        /// </summary>
        private void WatcherService_DocumentImported(object sender, DocumentImportedEventArgs e)
        {
            // Muss auf UI-Thread ausgeführt werden
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // Dokument importieren
                    ImportDocument(e.FilePath, e.TargetFolderId);

                    // Nach-Import-Aktion
                    HandleAfterImport(e.FilePath, e.AfterImport);

                    // GUI aktualisieren
                    if (_selectedFolder?.Id == e.TargetFolderId)
                    {
                        LoadDocuments(e.TargetFolderId);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Fehler beim automatischen Import:\n{Path.GetFileName(e.FilePath)}\n\n{ex.Message}",
                        "Import-Fehler",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            });
        }

        /// <summary>
        /// Event: Import-Fehler vom Watcher
        /// </summary>
        private void WatcherService_ImportError(object sender, ImportErrorEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"Fehler beim Überwachen:\n{e.FilePath}\n\n{e.ErrorMessage}",
                    "Watcher-Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            });
        }

        /// <summary>
        /// Behandelt Datei nach erfolgreichem Import
        /// </summary>
        private void HandleAfterImport(string filePath, archivsoftware.Business.Models.ImportAction action)
        {
            try
            {
                switch (action)
                {
                    case archivsoftware.Business.Models.ImportAction.Delete:
                        File.Delete(filePath);
                        break;

                    case archivsoftware.Business.Models.ImportAction.MoveToImported:
                        string importedFolder = Path.Combine(Path.GetDirectoryName(filePath), "Imported");
                        Directory.CreateDirectory(importedFolder);
                        string newPath = Path.Combine(importedFolder, Path.GetFileName(filePath));
                        File.Move(filePath, newPath);
                        break;

                    case archivsoftware.Business.Models.ImportAction.KeepInPlace:
                        // Nichts tun
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Verarbeiten der Datei nach Import:\n{ex.Message}",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        /// <summary>
        /// Button: Watcher Ein/Aus
        /// </summary>
        private void WatcherButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_watcherService.IsRunning)
                {
                    // Watcher stoppen
                    _watcherService.Stop();

                    // Button-Text & Farbe ändern
                    WatcherButton.Content = "Auto-Imp: AUS";
                    WatcherButton.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Rot
                    WatcherButton.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 53, 69));

                    MessageBox.Show("Auto-Import wurde gestoppt.",
                        "Watcher gestoppt",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    // Einstellungs-Dialog öffnen
                    var dialog = new WatcherSettingsDialog(_watcherSettings, _context);
                    dialog.Owner = this;

                    if (dialog.ShowDialog() == true)
                    {
                        // Einstellungen übernehmen
                        _watcherSettings = dialog.Settings;

                        // Ordner erstellen falls nicht vorhanden
                        if (!Directory.Exists(_watcherSettings.WatchPath))
                        {
                            Directory.CreateDirectory(_watcherSettings.WatchPath);
                        }

                        // Watcher starten
                        _watcherService.Start(_watcherSettings);

                        // Button-Text & Farbe ändern
                        WatcherButton.Content = "Auto-Imp: AN";
                        WatcherButton.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69)); // Grün
                        WatcherButton.BorderBrush = new SolidColorBrush(Color.FromRgb(40, 167, 69));

                        var folderName = _folderRepository.GetById(_watcherSettings.TargetFolderId).Name;

                        MessageBox.Show(
                            $"Auto-Import wurde gestartet!\n\n" +
                            $"📁 Überwacht: {_watcherSettings.WatchPath}\n" +
                            $"📂 Importiert nach: {folderName}\n\n" +
                            $"Legen Sie PDF oder DOCX Dateien in den überwachten Ordner,\n" +
                            $"sie werden automatisch importiert!",
                            "Watcher gestartet",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Steuern des Watchers:\n\n{ex.Message}",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // ========== DOKUMENT VERWALTEN: KONTEXTMENÜ-HANDLER ========== 

        /// <summary>
        /// Kontextmenü: Dokument umbenennen
        /// </summary>
        private void RenameDocument_Click(object sender, RoutedEventArgs e)
        {
            // Welches Dokument wurde rechts-geklickt?
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var card = contextMenu?.PlacementTarget as Border;
            var document = card?.Tag as Document;

            if (document == null)
            {
                MessageBox.Show("Kein Dokument ausgewählt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Dialog öffnen
            var dialog = new RenameDocumentDialog(document, _context);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                // Dokument in DB umbenennen
                document.FileName = dialog.NewFileName;
                _context.SaveChanges();

                // GUI aktualisieren
                if (_selectedFolder != null)
                {
                    LoadDocuments(_selectedFolder.Id);
                }

                MessageBox.Show(
                    $"Dokument wurde erfolgreich umbenannt zu:\n{dialog.NewFileName}",
                    "Erfolgreich",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        /// <summary>
        /// Kontextmenü: Dokument verschieben
        /// </summary>
        private void MoveDocument_Click(object sender, RoutedEventArgs e)
        {
            // Welches Dokument wurde rechts-geklickt?
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var card = contextMenu?.PlacementTarget as Border;
            var document = card?.Tag as Document;

            if (document == null)
            {
                MessageBox.Show("Kein Dokument ausgewählt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Dialog öffnen
            var dialog = new MoveDocumentDialog(document, _context);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                // Dokument in DB verschieben
                var targetFolder = _context.Folders.FirstOrDefault(f => f.FolderId == dialog.TargetFolderId);
                document.FolderId = dialog.TargetFolderId;
                _context.SaveChanges();

                // GUI aktualisieren
                if (_selectedFolder != null)
                {
                    LoadDocuments(_selectedFolder.Id);
                }

                MessageBox.Show(
                    $"Dokument wurde erfolgreich verschoben nach:\n{targetFolder?.FolderName}",
                    "Erfolgreich",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        /// <summary>
        /// Kontextmenü: Dokument löschen
        /// </summary>
        private void DeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            // Welches Dokument wurde rechts-geklickt?
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var card = contextMenu?.PlacementTarget as Border;
            var document = card?.Tag as Document;

            if (document == null)
            {
                MessageBox.Show("Kein Dokument ausgewählt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Bestätigung
            MessageBoxResult result = MessageBox.Show(
                $"Möchten Sie das Dokument '{document.FileName}' wirklich löschen?\n\nDiese Aktion kann nicht rückgängig gemacht werden.",
                "Dokument löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                // Aus DB löschen
                _context.Documents.Remove(document);
                _context.SaveChanges();

                // GUI aktualisieren
                if (_selectedFolder != null)
                {
                    LoadDocuments(_selectedFolder.Id);
                }

                // Vorschau leeren
                MetadataPanel.Children.Clear();
                PreviewText.Text = "";

                MessageBox.Show(
                    "Dokument wurde erfolgreich gelöscht.",
                    "Erfolgreich",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}