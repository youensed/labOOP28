using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.AccessControl;


namespace OOP_27
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            DriveInfo[] Drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in Drives)
            {
                listBox1.Items.Add(drive.Name);
                listBox1.Items.Add("");

            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Виберіть файл для переміщенння!";


            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string sourcePath = openFileDialog.FileName;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Виберіть місце для переміщення файла";



                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string targetPath = saveFileDialog.FileName;

                    try
                    {

                        File.Move(sourcePath, targetPath);
                        MessageBox.Show("Файл переміщено...");
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("Помилка при переміщенні: " + ex.Message);
                    }
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DriveInfo foundDrive = null;


            if (listBox1.SelectedItem != null)
            {
                string foundItem = listBox1.SelectedItem.ToString();



                string selectedDriveName = foundItem.Split(':')[0];


                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.Name.StartsWith(selectedDriveName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundDrive = drive;
                        break;
                    }
                }


                if (foundDrive != null)
                {

                    MessageBox.Show($"Назва диску: {foundDrive.Name}\n" +
                                    $"Тип диску: {foundDrive.DriveType}\n" +
                                    $"Ємність диску: {(foundDrive.TotalSize / (1024 * 1024 * 1024)).ToString()} ГБ\n" +
                                    $"Вільна ємність диску: {(foundDrive.TotalFreeSpace / (1024 * 1024 * 1024)).ToString()} ГБ");
                }
                else
                {
                    MessageBox.Show("Диск не знайдено.");
                }
            }
            else
            {
                MessageBox.Show("Ви нічого не обрали.");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            DirectoryInfo selectedDirectory = null;
            FileInfo selectedFile = null;

            if (listBox2.SelectedItem != null)
            {
                string selectedItem = listBox2.SelectedItem.ToString();
                selectedDirectory = new DirectoryInfo(selectedItem);

                if (selectedDirectory.Exists)
                {
                    MessageBox.Show($"Назва каталогу: {selectedDirectory.FullName}\n" +
                                    $"Дата створення: {selectedDirectory.CreationTime}\n" +
                                    $"Остання зміна: {selectedDirectory.LastWriteTime}\n" +
                                    $"Файли: {selectedDirectory.GetFiles().Length}\n" +
                                    $"Підкаталоги: {selectedDirectory.GetDirectories().Length}");
                }
                else
                {
                    selectedFile = new FileInfo(selectedItem);

                    if (selectedFile.Exists)
                    {
                        MessageBox.Show($"Назва файлу: {selectedFile.Name}\n" +
                                       $"Розмір: {(selectedFile.Length / (1024)).ToString()} KB\n" +
                                       $"Дата створення: {selectedFile.CreationTime}\n" +
                                       $"Остання зміна: {selectedFile.LastWriteTime}");
                    }
                    else
                    {
                        MessageBox.Show("Елемент не знайдено.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Ви нічого не обрали.");
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            DirectoryInfo selectedDirectory = null;

            if (listBox1.SelectedItem != null)
            {
                string selectedItem = listBox1.SelectedItem.ToString();



                selectedDirectory = new DirectoryInfo(selectedItem);

                if (selectedDirectory.Exists)
                {
                    listBox2.Items.Clear();

                    string[] dirs = Directory.GetDirectories(selectedItem);
                    foreach (string s in dirs)
                    {
                        listBox2.Items.Add("Підкаталоги:");
                        listBox2.Items.Add(s);
                    }
                    string[] files = Directory.GetFiles(selectedItem);
                    foreach (string s in files)
                    {
                        listBox2.Items.Add("Файли:");
                        listBox2.Items.Add(s);
                    }
                }
                else
                {
                    listBox2.Items.Clear();
                    listBox2.Items.Add("Каталог не знайдено.");
                }
            }
            else
            {
                listBox2.Items.Clear();
                listBox2.Items.Add("Ви нічого не обрали.");
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedDrive = listBox1.SelectedItem.ToString();
                string rootPath = Path.GetPathRoot(selectedDrive);


                string[] allDirs = Directory.GetDirectories(rootPath);


                var filteredDirs = allDirs.Where(dir => dir.Contains(textBox1.Text));

                listBox5.Items.Clear();

                foreach (string dir in filteredDirs)
                {
                    listBox5.Items.Add(dir);
                }
            }
            else
            {
                MessageBox.Show("Виберіть диск!.");
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            FileInfo selectedFile = null;
            listBox3.Items.Clear();
            if (listBox2.SelectedItem != null)
            {
                string path = listBox2.SelectedItem.ToString();

                try
                {

                    string readText = File.ReadAllText(path);
                    listBox3.Items.Add(readText);
                }
                catch (Exception ex)
                {

                    MessageBox.Show($"Помилка при читанні файлу: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Ви нічого не обрали.");
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            DirectoryInfo selectedDirectory = null;
            FileInfo selectedFile = null;
            listBox4.Items.Clear();

            string selectedPath = listBox2.SelectedItem.ToString();

            try
            {
                FileSystemSecurity security = null;
                if (File.Exists(selectedPath))
                {

                    security = File.GetAccessControl(selectedPath);
                }
                else if (Directory.Exists(selectedPath))
                {

                    security = Directory.GetAccessControl(selectedPath);
                }
                else
                {
                    MessageBox.Show("Файл або каталог не існує.");
                    return;
                }

                AuthorizationRuleCollection rules = security.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

                foreach (FileSystemAccessRule rule in rules)
                {
                    listBox4.Items.Add($"Користувач: {rule.IdentityReference}");
                    listBox4.Items.Add($"Тип доступу: {rule.FileSystemRights}");
                    listBox4.Items.Add($"Дозвіл: {rule.AccessControlType}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Помилка доступу: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }

        }
    }
}