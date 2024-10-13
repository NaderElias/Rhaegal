using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
         //  Bbutton grands = mindMapCells.AddNode(grand, 40, 200);
           // Bbutton gransd = mindMapCells.AddNode(child_1, 50, 350);
            //Button granssd = mindMapCells.AddNode(child_1, 100, 2000);
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
    }

  
}
