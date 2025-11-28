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

namespace archivsoftware
{
    public partial class RenameFolderDialog : Window
    {
        public string NewFolderName { get; private set; }

        public RenameFolderDialog(string currentName)
        {
            InitializeComponent();

            // Aktuellen Namen vorausfüllen
            FolderNameTextBox.Text = currentName;
            FolderNameTextBox.SelectAll();
            FolderNameTextBox.Focus();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            // Validierung
            if (string.IsNullOrWhiteSpace(FolderNameTextBox.Text))
            {
                MessageBox.Show("Bitte geben Sie einen Ordnernamen ein.",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewFolderName = FolderNameTextBox.Text.Trim();
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