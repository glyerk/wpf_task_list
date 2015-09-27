using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace wpf_assignment
{
    public partial class AddNotes : Window
    {
        public AddNotes()
        {
            InitializeComponent();
            ResponseTextBox.Focus();
        }
        
        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        public Service.TaskPriority ResponseStatus{ get; set; }
        
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Ok();
        }

        private void Ok()
        {
            if (string.IsNullOrEmpty(ResponseTextBox.Text.Trim()))
                return;
            
            if (NormalPriority.IsChecked == true)
            {
                ResponseStatus = Service.TaskPriority.Normal;
            }
            if (CriticalPriority.IsChecked == true)
            {
                ResponseStatus = Service.TaskPriority.Critical;
            }
            if (PostponedPriority.IsChecked == true)
            {
                ResponseStatus = Service.TaskPriority.Postponed;
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Cancel()
        {
            DialogResult = false;
        }

        private void ResponseTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null) textBox.Text = "";
        }

        private void HotKeysCatcher(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Ok();
            }
            if (e.Key == Key.Escape)
            {
                Cancel();
            }
        }
    }
}
