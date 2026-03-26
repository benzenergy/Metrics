using System.Windows;
using System.Windows.Input;

namespace ver20
{
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(string message)
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}