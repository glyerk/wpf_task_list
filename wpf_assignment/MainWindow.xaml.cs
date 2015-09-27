using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace wpf_assignment
{
    public static class Service
    {
        private static int _count;

        public static int Count
        {
            get { return _count++; }
            private set { _count = value; }
        }

        private static int _listcount;

        public static int ListCount
        {
            get { return _listcount++; }
            private set { _listcount = value; }
        }

        public enum TaskPriority
        {
            Normal,
            Critical,
            Postponed
        }

        static Service()
        {
            Count = 0;
            ListCount = 1;
        }
    }

    public partial class MainWindow
    {
        private static int _notesCount;

        private static readonly string ConfigFileName;

        private static string _lastSavedFile;

        static MainWindow()
        {
            ConfigFileName = @"config.cfg";
        }

        public MainWindow()
        {
            InitializeComponent();

            ConfigLoad();

            Closing += (sender, args) =>
            {
                if (MainStackPanel.Children.Count == 0 || _notesCount == MainStackPanel.Children.Count) return;
                const string msg = "Do you want to save Notes?";
                var result = MessageBox.Show(msg,
                    "Task Scheduler", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
            };
            Closed += (sender, args) =>
            {
                if (string.IsNullOrEmpty(_lastSavedFile.Trim()))
                {
                    return;
                }
                File.WriteAllText(ConfigFileName, _lastSavedFile);
            };
        }

        private void ConfigLoad()
        {
            if (File.Exists(ConfigFileName))
            {
                var lastFile =  File.ReadAllText(ConfigFileName);
                switch (Path.GetExtension(lastFile))
                {
                    case ".xml":
                        OpenXmlFile(lastFile);
                        break;
                    case ".csv":
                        OpenCsvFile(lastFile);
                        break;
                    case ".txt":
                        OpenTxtFile(lastFile);
                        break;
                }
                _notesCount = MainStackPanel.Children.Count;
            }
            else
            {
                MessageBox.Show("Welcome, new User!");
            }
        }

        private void AddNoteButton_OnClick(object sender, RoutedEventArgs e)
        {
            AddNote();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                FileName = "Task list",
                DefaultExt = ".xml",
                Filter = "Xml documents(*.xml)|*.xml|CSV Files(*.csv)|*.csv|Text Files(*.txt)|*.txt",
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
            {
                MainStackPanel.Children.Clear();
                try
                {
                    switch (Path.GetExtension(dlg.FileName))
                    {
                        case ".xml":
                            OpenXmlFile(dlg.FileName);
                            break;
                        case ".csv":
                            OpenCsvFile(dlg.FileName);
                            break;
                        case ".txt":
                            OpenTxtFile(dlg.FileName);
                            break;
                    }
                    _notesCount = MainStackPanel.Children.Count;
                }
                catch (Exception)
                {
                    MessageBox.Show("Can't read this format");
                }
            }

        }

        private void AddNote()
        {
            var dialog = new AddNotes();
            var number = (Service.Count).ToString();

            if (dialog.ShowDialog() != true) return;
            var newNote = new CheckBox
            {
                Name = "CheckBox" + number,
                Margin = new Thickness(0, 3, 0, 2),
                Cursor = Cursors.Hand
            };
            newNote.MouseLeave += Note_OnDelete;
            newNote.MouseRightButtonDown += Note_ChangeColor;
            newNote.IsKeyboardFocusedChanged += (sender, args) =>
            {
                NoteRemove(sender);
            };
                


            var newNoteText = new TextBlock();
            switch (dialog.ResponseStatus)
            {
                case Service.TaskPriority.Critical:
                    newNoteText.Foreground = Brushes.Red;
                    break;

                case Service.TaskPriority.Normal:
                    newNoteText.Foreground = Brushes.Black;
                    break;

                case Service.TaskPriority.Postponed:
                    newNoteText.Foreground = Brushes.Gray;
                    break;
            }

            newNoteText.TextTrimming = TextTrimming.CharacterEllipsis;
            newNoteText.Name = "TextBlock" + number;
            newNoteText.Height = 16;
            newNoteText.Text = dialog.ResponseText;
            newNoteText.ToolTip = newNoteText.Text;

            newNote.Content = newNoteText;


            MainStackPanel.Children.Add(newNote);
        }

        private void AddNote(string note, string color)
        {
            if (note == null || color == null)
            {
                throw new FileFormatException();
            }

            var number = (Service.Count).ToString();

            var newNote = new CheckBox
            {
                Name = "CheckBox" + number,
                Margin = new Thickness(0, 3, 0, 2),
                Cursor = Cursors.Hand
            };
            newNote.MouseLeave += Note_OnDelete;
            newNote.MouseRightButtonDown += Note_ChangeColor;


            var newNoteText = new TextBlock();
            var o = (ColorConverter.ConvertFromString(color));
            newNoteText.Foreground = o != null ? new SolidColorBrush((Color) o) : Brushes.Black;

            newNoteText.TextTrimming = TextTrimming.CharacterEllipsis;
            newNoteText.Name = "TextBlock" + number;
            newNoteText.Height = 16;
            newNoteText.Text = note;
            newNoteText.ToolTip = note;

            newNote.Content = newNoteText;

            MainStackPanel.Children.Add(newNote);
        }

        private void Note_OnDelete(object sender, MouseEventArgs e)
        {
            NoteRemove(sender);
        }

        private void NoteRemove(object sender)
        {
            var t = sender as CheckBox;
            if (t?.IsChecked == true)
            {
                MainStackPanel.Children.Remove(t);
            }
        }

        private void SaveFile()
        {
            var dlg = new SaveFileDialog
            {
                FileName = "Task list " + Service.ListCount,
                DefaultExt = ".xml",
                Filter = "Xml documents(*.xml)|*.xml|Text Files(*.txt)|*.txt"
            };

            if (dlg.ShowDialog() != true) return;
            switch (Path.GetExtension(dlg.FileName))
            {
                case ".xml":
                    SaveXmlFile(dlg.FileName);
                    break;
                case ".txt":
                    SaveTxtFile(dlg.FileName);
                    break;
            }
            _lastSavedFile = dlg.FileName;
            _notesCount = MainStackPanel.Children.Count;
        }

        private void SaveTxtFile(string fileName)
        {
            var list = (from CheckBox child in MainStackPanel.Children select (TextBlock) (child.Content) into temp select $"{temp.Text}, {temp.Foreground}").ToList();
            File.WriteAllLines(fileName, list);
        }

        private void SaveXmlFile(string fileName)
        {
            using (var writer = XmlWriter.Create(fileName))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Notes");

                foreach (CheckBox chkBox in MainStackPanel.Children)
                {
                    writer.WriteStartElement("Node");
                    var temp = (TextBlock) (chkBox.Content);

                    writer.WriteElementString("Note", temp.Text);
                    writer.WriteElementString("Status", temp.Foreground.ToString());

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


        }

        private void OpenXmlFile(string fileName)
        {
            using (var reader = XmlReader.Create(fileName))
            {
                string note = null;
                string color = null;
                try
                {
                    while (reader.Read())
                    {
                        if (!reader.IsStartElement()) continue;
                        if (reader.Name != "Note")
                        {
                            if (reader.Name != "Status") continue;
                            if (reader.Read())
                            {
                                color = reader.Value.Trim();
                            }
                            AddNote(note, color);
                        }
                        else
                        {
                            if (reader.Read())
                            {
                                note = reader.Value.Trim();
                            }
                        }
                    }
                }
                catch (FileFormatException)
                {
                    MessageBox.Show("Can't process file");
                }
            }
        }

        private void OpenCsvFile(string fileName)
        {
            if (!File.Exists(fileName)) return;
            var s = File.ReadAllLines(fileName);
            foreach (var str in s.Where(str => !string.IsNullOrWhiteSpace(str)))
            {
                AddNote(str, Brushes.Black.ToString());
            }
        }

        private void OpenTxtFile(string fileName)
        {
            if (!File.Exists(fileName)) return;
            var s = File.ReadAllLines(fileName);
            foreach (var note in from str in s where !string.IsNullOrWhiteSpace(str) select Regex.Match(str, @"(.*),\s*(.*)"))
            {
                AddNote(note.Groups[1].Value, note.Groups[2].Value);
            }
        }

        private static void Note_ChangeColor(object sender, MouseButtonEventArgs e)
        {
            if (sender is CheckBox)
            {
                TextBlock tempObj = (TextBlock)((sender as CheckBox).Content);
                Debug.Print(tempObj.Foreground.ToString());
                var temp = tempObj.Foreground.ToString();
                if (temp == Brushes.Red.ToString())
                {
                    tempObj.Foreground = Brushes.Gray;
                    
                }
                else if (temp == Brushes.Black.ToString())
                {
                    tempObj.Foreground = Brushes.Red;
                }
                else if (temp == Brushes.Gray.ToString())
                {
                    tempObj.Foreground = Brushes.Black;
                }
            }
        }

        private void HotKeysCatcher(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddNote();
            }
        }
    }
}
