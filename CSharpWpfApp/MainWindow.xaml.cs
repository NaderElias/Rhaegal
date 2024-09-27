using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using SharpVectors.Runtime;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace SharpVectors.Squiggle
{
    public partial class MainWindow : Window
    {
        private delegate void FileChangedToUIThread(FileSystemEventArgs e);
        private string _titleBase;
        private FileSystemWatcher _fileWatcher;
        private FileSvgReader _fileReader;
        private WpfDrawingSettings _wpfSettings;

        public MainWindow()
        {
            InitializeComponent();
            _titleBase = this.Title;

            Loaded += OnWindowLoaded;
            Closing += OnWindowClosing;

            _wpfSettings = new WpfDrawingSettings();
            _wpfSettings.CultureInfo = _wpfSettings.NeutralCultureInfo;

            _fileReader = new FileSvgReader(_wpfSettings)
            {
                SaveXaml = false,
                SaveZaml = false
            };
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            LoadSvg("../../../assets/lol.svg"); // Replace with your SVG file path
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle any cleanup if necessary
        }

        private void LoadSvg(string filePath)
        {
            try
            {
                DrawingGroup drawing = _fileReader.Read(filePath);
                SvgViewer.RenderDiagrams(drawing);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading SVG: {ex.Message}", _titleBase, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            MessageBox.Show($"Button {clickedButton.Content} clicked!");
        }
    }
}
