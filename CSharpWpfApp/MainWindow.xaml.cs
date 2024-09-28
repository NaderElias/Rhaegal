using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MindMap
{
    public partial class MainWindow : Window
    {
        private int maxDepth = 2; // Control the maximum depth of the mind map
        private int numberOfLeaves = 5; // Control the number of leaves for each node

        public MainWindow()
        {
            InitializeComponent();
            CreateMindMap(0, maxDepth); // Start with the root
            this.SizeChanged += MainWindow_SizeChanged; // Handle size changes
            this.KeyDown += MainWindow_KeyDown; // Handle key down events
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateButtonPositions(); // Update button positions when the window is resized
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    MoveToClosestButton(Direction.Up);
                    break;
                case Key.Down:
                    MoveToClosestButton(Direction.Down);
                    break;
                case Key.Left:
                    MoveToClosestButton(Direction.Left);
                    break;
                case Key.Right:
                    MoveToClosestButton(Direction.Right);
                    break;
            }
        }

        private void CreateMindMap(int depth, int maxDepth)
        {
            Button rootButton = new Button
            {
                Content = $"Level {depth}.0",
                Width = 100,
                Height = 50
            };
            MindMapCanvas.Children.Add(rootButton);
            rootButton.Click += Button_Click;

            AddNode(rootButton, depth, maxDepth);
            UpdateButtonPositions();
        }

        private void AddNode(Button parentButton, int depth, int maxDepth)
        {
            if (depth < maxDepth)
            {
                double angleIncrement = 360.0 / numberOfLeaves;
                double radius = 150;

                for (int i = 0; i < numberOfLeaves; i++)
                {
                    double angle = angleIncrement * i * Math.PI / 180; // Convert to radians
                    double x = (Math.Cos(angle) * radius) - (parentButton.Width / 2);
                    double y = (Math.Sin(angle) * radius) - (parentButton.Height / 2);

                    Button leafButton = new Button
                    {
                        Content = $"Level {depth + 1}.{i + 1}",
                        Width = 100,
                        Height = 50
                    };
                    MindMapCanvas.Children.Add(leafButton);
                    leafButton.Click += Button_Click;

                    Canvas.SetLeft(leafButton, x);
                    Canvas.SetTop(leafButton, y);

                    AddNode(leafButton, depth + 1, maxDepth);
                }
            }
        }

        private void MoveToClosestButton(Direction direction)
        {
            Button closestButton = null;
            double closestDistance = double.MaxValue;
            Point center = new Point(ActualWidth / 2, ActualHeight / 2);

            foreach (Button button in MindMapCanvas.Children)
            {
                if (button.Content.ToString().StartsWith("Level")) // Only consider mind map buttons
                {
                    Point buttonPosition = new Point(Canvas.GetLeft(button) + button.Width / 2, Canvas.GetTop(button) + button.Height / 2);
                    double distance = GetDistance(center, buttonPosition);

                    if (IsInDirection(center, buttonPosition, direction) && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestButton = button;
                    }
                }
            }

            if (closestButton != null)
            {
                CenterButton(closestButton);
            }
        }

        private double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private bool IsInDirection(Point center, Point buttonPosition, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return buttonPosition.Y < center.Y;
                case Direction.Down:
                    return buttonPosition.Y > center.Y;
                case Direction.Left:
                    return buttonPosition.X < center.X;
                case Direction.Right:
                    return buttonPosition.X > center.X;
                default:
                    return false;
            }
        }

        private void CenterButton(Button button)
        {
            double offsetX = (ActualWidth / 2) - (Canvas.GetLeft(button) + button.Width / 2);
            double offsetY = (ActualHeight / 2) - (Canvas.GetTop(button) + button.Height / 2);

            foreach (UIElement element in MindMapCanvas.Children)
            {
                Canvas.SetLeft(element, Canvas.GetLeft(element) + offsetX);
                Canvas.SetTop(element, Canvas.GetTop(element) + offsetY);
            }
        }

        private void UpdateButtonPositions()
        {
            Button rootButton = (Button)MindMapCanvas.Children[0];
            Canvas.SetLeft(rootButton, (ActualWidth - rootButton.Width) / 2);
            Canvas.SetTop(rootButton, (ActualHeight - rootButton.Height) / 2);

            int leafCount = Math.Min(this.numberOfLeaves, MindMapCanvas.Children.Count - 1);
            double angleIncrement = 360.0 / leafCount;
            double radius = 150;

            for (int i = 0; i < leafCount; i++)
            {
                double angle = angleIncrement * i * Math.PI / 180;
                double x = (ActualWidth / 2) + Math.Cos(angle) * radius - (rootButton.Width / 2);
                double y = (ActualHeight / 2) + Math.Sin(angle) * radius - (rootButton.Height / 2);

                Button leafButton = (Button)MindMapCanvas.Children[i + 1];
                Canvas.SetLeft(leafButton, x);
                Canvas.SetTop(leafButton, y);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            MessageBox.Show(clickedButton.Content.ToString());
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
