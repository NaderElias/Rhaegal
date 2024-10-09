using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
//using ExCSS;

namespace MindMap
{
    public class Bbutton : Button
    {
        public Button parent =null;
        public double distance;
        public double angle;
        public Line connectionLine;
        public double[] coordinates;
        public bool isSet;
        public bool isRoot;
        // Line connection

        public Bbutton(Button parent, double distance, double angle, Line line)
        {
            this.parent = parent;
            this.distance = distance;
            this.angle = angle;
            connectionLine = line;
            isSet = false;
            isRoot = false;
            coordinates = new double[2];
        }
        public Bbutton() { isRoot = true; }
    }

    public class Cells
    {
        private Canvas mindMapCanvas;
        private double baseDistance = 100; // Base distance from parent to child

        public Cells(Canvas canvas)
        {
            mindMapCanvas = canvas;
        }

        public Button CreateMindMap()
        {
            // Root node (central)
            Bbutton rootButton = new Bbutton
            {
                Content = "button",
                Width = 100,
                Height = 100,
                Style = (Style)mindMapCanvas.FindResource("CircularButtonStyle")
            };
            mindMapCanvas.Children.Add(rootButton);
            rootButton.Click += Button_Click;

            // Place root button at the center of the canvas
            Canvas.SetLeft(rootButton, (mindMapCanvas.ActualWidth - rootButton.Width) / 2);
            Canvas.SetTop(rootButton, (mindMapCanvas.ActualHeight - rootButton.Height) / 2);
            return rootButton;
        }

        public Bbutton AddNode(Button parentButton, double ang, double dist)
        {
            // Parent node position
            double parentX = Canvas.GetLeft(parentButton);
            double parentY = Canvas.GetTop(parentButton);

            // Calculate child position relative to parent
            double angle = ang; // In degrees
            double distance = dist; // In pixels

            // Calculate child button's position
            double x = parentX + Math.Cos(angle * Math.PI / 180) * distance;
            double y = parentY + Math.Sin(angle * Math.PI / 180) * distance;

            // Create a line connection between parent and child
            Line line = new Line
            {
                Stroke = System.Windows.Media.Brushes.OrangeRed,
                StrokeThickness = 3,
                X1 = x,
                Y1 = y,
                X2 = x,
                Y2 = y,

            };
            // Create child node button

            Bbutton childButton = new Bbutton(parentButton, dist, ang, line)
            {
                Content = "child",
                Width = 100,
                Height = 100,
                Style = (Style)mindMapCanvas.FindResource("CircularButtonStyle")
            };
            mindMapCanvas.Children.Add(childButton);
            childButton.Click += Button_Click;

            mindMapCanvas.Children.Add(line);

            // Set child node position
            Canvas.SetLeft(childButton, x - childButton.Width / 2);
            Canvas.SetTop(childButton, y - childButton.Height / 2);

            // Store the line in the child button
            childButton.connectionLine = line;

            return childButton;
        }

        public void MoveToClosestButton(Direction direction)
        {
            Button closestButton = null;
            double closestDistance = double.MaxValue;

            // Center point of the canvas
            Point center = new Point(mindMapCanvas.ActualWidth / 2, mindMapCanvas.ActualHeight / 2);

            // Find the button at the center (current "focused" button)
            Button centerButton = null;
            foreach (UIElement element in mindMapCanvas.Children)
            {
                if (element is Button button)
                {
                    Point buttonPosition = new Point(Canvas.GetLeft(button) + button.Width / 2, Canvas.GetTop(button) + button.Height / 2);
                    if (GetDistance(center, buttonPosition) < 10)
                    {
                        centerButton = button;
                        break;
                    }
                }
            }

            if (centerButton == null)
            {
                return; // No button at the center
            }

            // Find the closest button in the specified direction
            foreach (UIElement element in mindMapCanvas.Children)
            {
                if (element is Button button && button != centerButton)
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
            double offsetX = (mindMapCanvas.ActualWidth / 2) - (Canvas.GetLeft(button) + button.Width / 2);
            double offsetY = (mindMapCanvas.ActualHeight / 2) - (Canvas.GetTop(button) + button.Height / 2);

            foreach (UIElement element in mindMapCanvas.Children)
            {
                if (element.GetType() != typeof(Line))
                {
                    Canvas.SetLeft(element, Canvas.GetLeft(element) + offsetX);
                    Canvas.SetTop(element, Canvas.GetTop(element) + offsetY);
                }
            }
            if ((button as Bbutton).isRoot) { UpdateButtonPositions();  }
            else
            {
                for (int i = 1; i < mindMapCanvas.Children.Count; i++)
                {
                    if (mindMapCanvas.Children[i] is Bbutton leafButton)
                    {
                       

                        // Update the position of the connection line
                        if (leafButton.connectionLine != null)
                        {
                            // Get the current position of the parent button (center point)
                            double parentX = Canvas.GetLeft(leafButton.parent) + leafButton.parent.Width / 2;
                            double parentY = Canvas.GetTop(leafButton.parent) + leafButton.parent.Height / 2;

                            // Get the current position of the child button (center point)
                            double childX = Canvas.GetLeft(leafButton) + leafButton.Width / 2;
                            double childY = Canvas.GetTop(leafButton) + leafButton.Height / 2;

                            // Calculate the angle from the parent to the child
                            double anglee = Math.Atan2(childY - parentY, childX - parentX);

                            // Calculate the radius of the buttons (assuming they're circular, i.e., using Width/2)
                            double parentRadius = leafButton.parent.Width / 2;
                            double childRadius = leafButton.Width / 2;

                            // Calculate the new points on the edges of the buttons based on the angle
                            double newParentX = parentX + parentRadius * Math.Cos(anglee);
                            double newParentY = parentY + parentRadius * Math.Sin(anglee);

                            double newChildX = childX - childRadius * Math.Cos(anglee);  // Subtract to adjust for direction
                            double newChildY = childY - childRadius * Math.Sin(anglee);  // Subtract to adjust for direction
                            if (!leafButton.isSet)
                            {
                                leafButton.coordinates[0] = newParentX;
                                leafButton.coordinates[1] = newParentY;
                                leafButton.isSet = true;
                            }
                            // Update the line's X1, Y1 (parent edge) and X2, Y2 (child edge)
                            //*PrintDialog(leafButton.coordinates[0]);
                            leafButton.connectionLine.X1 = newParentX;// leafButton.coordinates[0];
                            leafButton.connectionLine.Y1 = newParentY; //leafButton.coordinates[1];

                            leafButton.connectionLine.X2 = newChildX;
                            leafButton.connectionLine.Y2 = newChildY;
                        }

                    }
                }
            }
        }

        public void UpdateButtonPositions()
        {
            if (mindMapCanvas.Children.Count == 0) return;

            Button rootButton = (Button)mindMapCanvas.Children[0];
            Canvas.SetLeft(rootButton, (mindMapCanvas.ActualWidth - rootButton.Width) / 2);
            Canvas.SetTop(rootButton, (mindMapCanvas.ActualHeight - rootButton.Height) / 2);

            for (int i = 1; i < mindMapCanvas.Children.Count; i++)
            {
                if (mindMapCanvas.Children[i] is Bbutton leafButton)
                {
                    double angle = leafButton.angle;
                    double x = (mindMapCanvas.ActualWidth / 2) + Math.Cos(angle * Math.PI / 180) * leafButton.distance;
                    double y = (mindMapCanvas.ActualHeight / 2) + Math.Sin(angle * Math.PI / 180) * leafButton.distance;

                    Canvas.SetLeft(leafButton, x - leafButton.Width / 2);
                    Canvas.SetTop(leafButton, y - leafButton.Height / 2);

                    // Update the position of the connection line
                    if (leafButton.connectionLine != null)
                    {
                        // Get the current position of the parent button (center point)
                        double parentX = Canvas.GetLeft(leafButton.parent) + leafButton.parent.Width / 2;
                        double parentY = Canvas.GetTop(leafButton.parent) + leafButton.parent.Height / 2;

                        // Get the current position of the child button (center point)
                        double childX = Canvas.GetLeft(leafButton) + leafButton.Width / 2;
                        double childY = Canvas.GetTop(leafButton) + leafButton.Height / 2;

                        // Calculate the angle from the parent to the child
                        double anglee = Math.Atan2(childY - parentY, childX - parentX);

                        // Calculate the radius of the buttons (assuming they're circular, i.e., using Width/2)
                        double parentRadius = leafButton.parent.Width / 2;
                        double childRadius = leafButton.Width / 2;

                        // Calculate the new points on the edges of the buttons based on the angle
                        double newParentX = parentX + parentRadius * Math.Cos(anglee);
                        double newParentY = parentY + parentRadius * Math.Sin(anglee);

                        double newChildX = childX - childRadius * Math.Cos(anglee);  // Subtract to adjust for direction
                        double newChildY = childY - childRadius * Math.Sin(anglee);  // Subtract to adjust for direction
                        if (!leafButton.isSet)
                        {
                            leafButton.coordinates[0] = newParentX;
                            leafButton.coordinates[1] = newParentY;
                            leafButton.isSet = true;
                        }
                        // Update the line's X1, Y1 (parent edge) and X2, Y2 (child edge)
                        //*PrintDialog(leafButton.coordinates[0]);
                        leafButton.connectionLine.X1 = newParentX;// leafButton.coordinates[0];
                        leafButton.connectionLine.Y1 = newParentY; //leafButton.coordinates[1];

                        leafButton.connectionLine.X2 = newChildX;
                        leafButton.connectionLine.Y2 = newChildY;
                    }

                }
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
