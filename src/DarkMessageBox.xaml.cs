using System.Windows;

namespace uniconvert
{
    public partial class DarkMessageBox : Window
    {
        public DarkMessageBox(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public static void Show(string message, Window owner)
        {
            var box = new DarkMessageBox(message);
            box.Owner = owner;
            box.ShowDialog();
        }
    }
}