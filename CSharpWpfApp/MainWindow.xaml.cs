using System.Windows;

namespace CustomButtonExample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CustomButton_Click(object sender, RoutedEventArgs e)
        {
            // Your logic here
            MessageBox.Show("Button Clicked!");
        }
    }
}
