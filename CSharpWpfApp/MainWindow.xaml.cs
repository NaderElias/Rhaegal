using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace CSharpWpfApp
{
    public partial class MainWindow : Window
    {
        // Import the Rust function from the DLL
        [DllImport("rust_core.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void show_notification();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NotifyButton_Click(object sender, RoutedEventArgs e)
        {
            // Call the Rust function to show the notification
            show_notification();
        }
    }
}
