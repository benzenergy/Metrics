using System.Windows;
using System.Windows.Controls;

namespace ver10
{
    public partial class SettingsWindow : Window
    {
        public string SelectedMethod { get; private set; } = "Процент от общей суммы";

        public SettingsWindow(string currentMethod)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(currentMethod))
            {
                PercentComboBox.SelectedIndex = 0;
                SelectedMethod = "Процент от общей суммы";
            }
            else
            {
                for (int i = 0; i < PercentComboBox.Items.Count; i++)
                {
                    if (PercentComboBox.Items[i] is ComboBoxItem item &&
                        item.Content.ToString() == currentMethod)
                    {
                        PercentComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (PercentComboBox.SelectedItem is ComboBoxItem item)
                SelectedMethod = item.Content.ToString();

            this.DialogResult = true;
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}