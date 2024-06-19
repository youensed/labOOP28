using System.IO;
using System.Text;
using System.Windows;

namespace ExplorerApp
{
    public partial class EditWindow : Window
    {
        private FileInfo _file;

        public EditWindow(FileInfo file)
        {
            InitializeComponent();
            _file = file;
            LoadFileContent();
        }

        private void LoadFileContent()
        {
            EditorTextBox.Text = File.ReadAllText(_file.FullName, Encoding.UTF8);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(_file.FullName, EditorTextBox.Text, Encoding.UTF8);
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
