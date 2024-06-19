using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO.Compression;
using Microsoft.Win32;

namespace ExplorerApp
{
    public partial class MainWindow : Window
    {
        private string currentPath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            LoadDrives();
            AddPlaceholderText(FilterTextBox, null);
        }

        private void LoadDrives()
        {
            try
            {
                DriveTree.Items.Clear();
                foreach (var drive in DriveInfo.GetDrives())
                {
                    var driveItem = new TreeViewItem { Header = drive.Name, Tag = drive };
                    driveItem.Items.Add(null);
                    driveItem.Expanded += DriveItem_Expanded;
                    DriveTree.Items.Add(driveItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні дисків: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDirectory(TreeViewItem item)
        {
            try
            {
                if (item.Items.Count == 1 && item.Items[0] == null)
                {
                    item.Items.Clear();
                    var dir = item.Tag as DirectoryInfo ?? new DirectoryInfo((item.Tag as DriveInfo).RootDirectory.FullName);
                    try
                    {
                        foreach (var subDir in dir.GetDirectories())
                        {
                            var subItem = new TreeViewItem { Header = subDir.Name, Tag = subDir };
                            subItem.Items.Add(null);
                            subItem.Expanded += DriveItem_Expanded;
                            item.Items.Add(subItem);
                        }
                        foreach (var file in dir.GetFiles())
                        {
                            var subItem = new TreeViewItem { Header = file.Name, Tag = file };
                            item.Items.Add(subItem);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні каталогу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DriveItem_Expanded(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (TreeViewItem)sender;
                LoadDirectory(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при розширенні елементу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DriveTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var selectedItem = DriveTree.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    if (selectedItem.Tag is DriveInfo drive)
                    {
                        PropertiesTextBlock.Text = $"Диск: {drive.Name}\nОбсяг: {drive.TotalSize}\nВільне місце: {drive.AvailableFreeSpace}";
                        currentPath = drive.RootDirectory.FullName;
                        LoadDirectoryContent(drive.RootDirectory);
                    }
                    else if (selectedItem.Tag is DirectoryInfo dir)
                    {
                        PropertiesTextBlock.Text = $"Каталог: {dir.FullName}\nДата створення: {dir.CreationTime}";
                        currentPath = dir.FullName;
                        LoadDirectoryContent(dir);
                    }
                    else if (selectedItem.Tag is FileInfo file)
                    {
                        PropertiesTextBlock.Text = $"Файл: {file.FullName}\nРозмір: {file.Length}\nДата створення: {file.CreationTime}\nОстання модифікація: {file.LastWriteTime}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при виборі елементу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDirectoryContent(DirectoryInfo dir)
        {
            try
            {
                DirectoryContentListBox.Items.Clear();
                foreach (var subDir in dir.GetDirectories())
                {
                    DirectoryContentListBox.Items.Add(new ListBoxItem { Content = subDir.Name, Tag = subDir });
                }
                foreach (var file in dir.GetFiles())
                {
                    DirectoryContentListBox.Items.Add(new ListBoxItem { Content = file.Name, Tag = file });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні вмісту каталогу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshTreeView()
        {
            try
            {
                var selectedItem = DriveTree.SelectedItem as TreeViewItem;
                if (selectedItem != null)
                {
                    if (selectedItem.Tag is DirectoryInfo dir)
                    {
                        selectedItem.Items.Clear();
                        selectedItem.Items.Add(null);
                        LoadDirectory(selectedItem);
                    }
                    else if (selectedItem.Tag is DriveInfo drive)
                    {
                        selectedItem.Items.Clear();
                        selectedItem.Items.Add(null);
                        LoadDirectory(selectedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні дерева: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DirectoryContentListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedItem = DirectoryContentListBox.SelectedItem as ListBoxItem;
                if (selectedItem != null)
                {
                    if (selectedItem.Tag is FileInfo file)
                    {
                        if (IsImageFile(file))
                        {
                            DisplayImage(file);
                        }
                        else if (IsTextFile(file))
                        {
                            DisplayTextFile(file);
                        }
                        DisplaySecurityAttributes(file);
                    }
                    else if (selectedItem.Tag is DirectoryInfo dir)
                    {
                        DisplaySecurityAttributes(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при зміні вибору вмісту каталогу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                var filterText = FilterTextBox.Text.ToLower();
                for (int i = 0; i < DirectoryContentListBox.Items.Count; i++)
                {
                    var item = (ListBoxItem)DirectoryContentListBox.Items[i];
                    var itemName = item.Content.ToString().ToLower();
                    item.Visibility = itemName.Contains(filterText) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при фільтрації вмісту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsImageFile(FileInfo file)
        {
            try
            {
                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                return imageExtensions.Contains(file.Extension.ToLower());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при визначенні типу зображення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool IsTextFile(FileInfo file)
        {
            try
            {
                string[] textExtensions = { ".txt", ".log", ".xml", ".json" };
                return textExtensions.Contains(file.Extension.ToLower());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при визначенні типу текстового файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void DisplayImage(FileInfo file)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(file.FullName);
                bitmap.EndInit();

                var image = new Image { Source = bitmap, Width = 750, Height = 450 };
                var imageWindow = new Window
                {
                    Title = file.Name,
                    Content = image,
                    Width = 800,
                    Height = 500
                };
                imageWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при відображенні зображення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayTextFile(FileInfo file)
        {
            try
            {
                string text = File.ReadAllText(file.FullName, Encoding.UTF8);
                var textBox = new TextBox { Text = text, TextWrapping = TextWrapping.Wrap, IsReadOnly = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                var textWindow = new Window
                {
                    Title = file.Name,
                    Content = textBox,
                    Width = 800,
                    Height = 500
                };
                textWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при відображенні текстового файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplaySecurityAttributes(FileSystemInfo fileSystemInfo)
        {
            try
            {
                FileSecurity fileSecurity = null;
                DirectorySecurity directorySecurity = null;

                if (fileSystemInfo is FileInfo fileInfo)
                {
                    fileSecurity = fileInfo.GetAccessControl();
                }
                else if (fileSystemInfo is DirectoryInfo directoryInfo)
                {
                    directorySecurity = directoryInfo.GetAccessControl();
                }

                var rules = (fileSecurity != null ? fileSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)) :
                              directorySecurity != null ? directorySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)) :
                              null);

                if (rules != null)
                {
                    var securityInfo = new StringBuilder();
                    foreach (FileSystemAccessRule rule in rules)
                    {
                        securityInfo.AppendLine(rule.IdentityReference.ToString());
                        securityInfo.AppendLine(rule.AccessControlType.ToString());
                    }
                    PropertiesTextBlock.Text += "\nАтрибути безпеки:\n" + securityInfo.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при відображенні атрибутів безпеки: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemovePlaceholderText(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FilterTextBox.Text == "Фільтр...")
                {
                    FilterTextBox.Text = "";
                    FilterTextBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при видаленні тексту заповнювача: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPlaceholderText(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FilterTextBox.Text))
                {
                    FilterTextBox.Text = "Фільтр...";
                    FilterTextBox.Foreground = System.Windows.Media.Brushes.Gray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні тексту заповнювача: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newDirName = Prompt("Введіть назву нового каталогу:");
                if (!string.IsNullOrEmpty(newDirName))
                {
                    string newPath = Path.Combine(currentPath, newDirName);
                    Directory.CreateDirectory(newPath);
                    LoadDirectoryContent(new DirectoryInfo(currentPath));
                    RefreshTreeView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при створенні каталогу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newFileName = Prompt("Введіть назву нового файлу:");
                if (!string.IsNullOrEmpty(newFileName))
                {
                    string newPath = Path.Combine(currentPath, newFileName);
                    File.Create(newPath).Close();
                    LoadDirectoryContent(new DirectoryInfo(currentPath));
                    RefreshTreeView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при створенні файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DirectoryContentListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                try
                {
                    if (selectedItem.Tag is FileInfo file)
                    {
                        file.Delete();
                    }
                    else if (selectedItem.Tag is DirectoryInfo dir)
                    {
                        dir.Delete(true);
                    }
                    LoadDirectoryContent(new DirectoryInfo(currentPath));
                    RefreshTreeView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при видаленні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MoveItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DirectoryContentListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                string destinationPath = Prompt("Введіть шлях призначення:");
                if (!string.IsNullOrEmpty(destinationPath))
                {
                    try
                    {
                        if (selectedItem.Tag is FileInfo file)
                        {
                            string newFilePath = Path.Combine(destinationPath, file.Name);
                            File.Move(file.FullName, newFilePath);
                        }
                        else if (selectedItem.Tag is DirectoryInfo dir)
                        {
                            string newDirPath = Path.Combine(destinationPath, dir.Name);
                            Directory.Move(dir.FullName, newDirPath);
                        }
                        LoadDirectoryContent(new DirectoryInfo(currentPath));
                        RefreshTreeView();
                        currentPath = destinationPath;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при переміщенні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DirectoryContentListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                string destinationPath = Prompt("Введіть шлях призначення:");
                if (!string.IsNullOrEmpty(destinationPath))
                {
                    try
                    {
                        if (selectedItem.Tag is FileInfo file)
                        {
                            string newFilePath = Path.Combine(destinationPath, file.Name);
                            File.Copy(file.FullName, newFilePath);
                        }
                        else if (selectedItem.Tag is DirectoryInfo dir)
                        {
                            string newDirPath = Path.Combine(destinationPath, dir.Name);
                            CopyDirectory(dir.FullName, newDirPath);
                        }
                        LoadDirectoryContent(new DirectoryInfo(currentPath));
                        RefreshTreeView();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при копіюванні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EditAttributes_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DirectoryContentListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                FileSystemInfo fileSystemInfo = selectedItem.Tag as FileSystemInfo;
                if (fileSystemInfo != null)
                {
                    string attributes = Prompt("Введіть нові атрибути (ReadOnly, Hidden):");
                    if (!string.IsNullOrEmpty(attributes))
                    {
                        try
                        {
                            fileSystemInfo.Attributes = (FileAttributes)Enum.Parse(typeof(FileAttributes), attributes, true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Помилка при зміні атрибутів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void EditTextFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DirectoryContentListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null && selectedItem.Tag is FileInfo file)
            {
                EditTextFile(file);
            }
        }

        private void EditTextFile(FileInfo file)
        {
            try
            {
                var editWindow = new EditWindow(file);
                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при редагуванні текстового файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ZipItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DirectoryContentListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null)
            {
                string zipPath = Prompt("Введіть шлях для збереження ZIP файлу:");
                if (!string.IsNullOrEmpty(zipPath))
                {
                    try
                    {
                        if (selectedItem.Tag is FileInfo file)
                        {
                            if (!zipPath.EndsWith(".zip"))
                            {
                                zipPath += ".zip";
                            }

                            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                            {
                                archive.CreateEntryFromFile(file.FullName, file.Name);
                            }
                        }
                        else if (selectedItem.Tag is DirectoryInfo dir)
                        {
                            if (!zipPath.EndsWith(".zip"))
                            {
                                zipPath += ".zip";
                            }

                            ZipFile.CreateFromDirectory(dir.FullName, zipPath);
                        }

                        RefreshTreeView();
                        MessageBox.Show("Архівація завершена успішно!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при архівації: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void UnzipItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DirectoryContentListBox.SelectedItem as ListBoxItem;
            if (selectedItem != null && selectedItem.Tag is FileInfo file && file.Extension == ".zip")
            {
                string extractPath = Prompt("Введіть шлях для розпакування ZIP файлу:");
                if (!string.IsNullOrEmpty(extractPath))
                {
                    try
                    {
                        ZipFile.ExtractToDirectory(file.FullName, extractPath);
                        MessageBox.Show("Розпакування завершено успішно!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshTreeView();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при розпакуванні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private string Prompt(string message)
        {
            try
            {
                var prompt = new PromptWindow(message);
                if (prompt.ShowDialog() == true)
                {
                    return prompt.ResponseText;
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при введенні даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            try
            {
                Directory.CreateDirectory(destinationDir);
                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                    File.Copy(file, destFile);
                }
                foreach (var directory in Directory.GetDirectories(sourceDir))
                {
                    var destDir = Path.Combine(destinationDir, Path.GetFileName(directory));
                    CopyDirectory(directory, destDir);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при копіюванні каталогу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
