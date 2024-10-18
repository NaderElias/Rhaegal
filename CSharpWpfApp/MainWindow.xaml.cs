using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace MindMap
{
    public partial class MainWindow : Window
    {
        private Cells mindMapCells; // Instance of the Cells class

        public MainWindow()
        {
            InitializeComponent();
            mindMapCells = new Cells(MindMapCanvas); // Initialize Cells with the canvas, number of leaves, and depth
            Button root=mindMapCells.CreateMindMap(); // Start with the root
            Bbutton child_1=mindMapCells.AddNode(root,90,300);

         Bbutton grand =mindMapCells.AddNode(child_1,10,200);
             Bbutton grands = mindMapCells.AddNode(grand, 40, 200);
             Bbutton gransd = mindMapCells.AddNode(child_1, 50, 350);
            Button granssd = mindMapCells.AddNode(child_1, 100, 2000);
            // Assuming startElements is a Dictionary<UIElement, object>
            foreach (var kvp in mindMapCells.startElements)
            {
                UIElement element = kvp.Key;
                object value = kvp.Value;

                // Determine if it's a Button or a Line and print accordingly
                if (element is Button button)
                {
                    // The value for Button is expected to be (newX, newY)
                    if (value is (double newX, double newY))
                    {
                        Debug.WriteLine($"Button: {button.Name}, New Position: X = {newX}, Y = {newY}");
                    }
                }
                else if (element is Line line)
                {
                    // The value for Line is expected to be (X1, Y1, X2, Y2)
                    if (value is (double X1, double Y1, double X2, double Y2))
                    {
                        Debug.WriteLine($"Line: {line.Name}, Start: (X1 = {X1}, Y1 = {Y1}), End: (X2 = {X2}, Y2 = {Y2})");
                    }
                }
            }


           // mindMapCells.StartUI(mindMapCells.startElements);
            this.SizeChanged += MainWindow_SizeChanged; // Handle size changes
            this.KeyDown += MainWindow_KeyDown; // Handle key down events
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mindMapCells.UpdateButtonPositions(); // Update button positions when the window is resized
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    mindMapCells.MoveToClosestButton(Direction.Up);
                    break;
                case Key.Down:
                    mindMapCells.MoveToClosestButton(Direction.Down);
                    break;
                case Key.Left:
                    mindMapCells.MoveToClosestButton(Direction.Left);
                    break;
                case Key.Right:
                    mindMapCells.MoveToClosestButton(Direction.Right);
                    break;
            }
        }
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // Minimizes the window when clicking the minimize button
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Closes the window when clicking the close button
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

  
}
