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
    public partial class CreateFolderDialog : Window
    {
        public string FolderName { get; private set; }

        public CreateFolderDialog()
        {
            InitializeComponent();
            FolderNameTextBox.Focus();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Validierung
            if (string.IsNullOrWhiteSpace(FolderNameTextBox.Text))
            {
                MessageBox.Show("Bitte geben Sie einen Ordnernamen ein.",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FolderName = FolderNameTextBox.Text.Trim();
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