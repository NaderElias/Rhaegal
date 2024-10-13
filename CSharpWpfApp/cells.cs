using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Animation;
using SharpVectors.Dom;
using System.IO;
using System.Diagnostics;

//using ExCSS;

namespace MindMap
{
    public class HalfValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return d / 2;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Bbutton : Button
    {
        public Button parent = null;
        public double distance;
        public double angle;
        public Line connectionLine;
        public double[] coordinates;
        public bool isSet;
        public bool isRoot =false;
        // Line connection

        public Bbutton(Button parent, double distance, double angle, Line line,bool root)
        {
            this.parent = parent;
            this.distance = distance;
            this.angle = angle;
            connectionLine = line;
            isSet = false;
            isRoot = root;
            coordinates = new double[2];
        }
        public Bbutton(bool root) {
          //  MessageBox.Show("su ck");
            isRoot = root; }
    }

    public class Cells
    {
        private Canvas mindMapCanvas;
        private double baseDistance = 100; // Base distance from parent to child
        private Dictionary<Bbutton, (double newX, double newY)> exlop;

        public Cells(Canvas canvas)
        {
            mindMapCanvas = canvas;
        }

        public Button CreateMindMap()
        {
            // Root node (central)
            Bbutton rootButton = new Bbutton(true)
            {
                Content = "button",
                Width = 100,
                Height = 100,
                Style = (Style)mindMapCanvas.FindResource("roro")
            };
            mindMapCanvas.Children.Add(rootButton);
            rootButton.Click += Button_Click;

            // Place root button at the center of the canvas
            Canvas.SetLeft(rootButton, (mindMapCanvas.ActualWidth - rootButton.Width) / 2);
            Canvas.SetTop(rootButton, (mindMapCanvas.ActualHeight - rootButton.Height) / 2);
            return rootButton;
        }

        private Dictionary<Bbutton, (double newX, double newY)> CalculateButtonPositionsAfterMove(Bbutton butt)
        {
            // Assuming the center of the canvas is the origin for the circular arrangement
            double centerX = mindMapCanvas.ActualWidth / 2;
            double centerY = mindMapCanvas.ActualHeight / 2;

            // Dictionary to store new positions of each button
            var newPositions = new Dictionary<Bbutton, (double newX, double newY)>();

            // Iterate through all children of the canvas
            foreach (UIElement element in mindMapCanvas.Children)
            {
                // Check if the child is of type Bbutton
                if (element.GetType() == typeof(Bbutton))
                {

                    double offsetX = (mindMapCanvas.ActualWidth / 2) - (Canvas.GetLeft(butt) + butt.Width / 2);
                    double offsetY = (mindMapCanvas.ActualHeight / 2) - (Canvas.GetTop(butt) + butt.Height / 2);


                    // Using StreamWriter to write to the file



                    // Calculate the new position for the moved button
                    newPositions[element as Bbutton] = (Canvas.GetLeft(element) + offsetX, Canvas.GetTop(element) + offsetY);


                }
            }

             var message = new System.Text.StringBuilder("New Positions:\n");

              // Iterate through the dictionary to get all key-value pairs
             /* foreach (var kvp in newPositions)
              {
                  (double newX, double newY) = kvp.Value;
                  message.AppendLine($"New X: {newX}, New Y: {newY}");
                message.AppendLine(kvp.Key.isRoot.ToString());
            }

              // Show the message box with all values
              MessageBox.Show(message.ToString());*/
            return newPositions;
        }


        private List<Button> GetAllButtons(Canvas canvas)
        {
            List<Button> buttons = new List<Button>();

            // Iterate through all children of the canvas
            foreach (var child in canvas.Children)
            {
                // Check if the child is a Button
                if (child is Button button)
                {
                    buttons.Add(button);
                }
            }

            return buttons;
        }

        private void AnimateButton(Button button, Dictionary<Bbutton, (double newX, double newY)> exlop)
        {
            var exlopCopy = new Dictionary<Bbutton, (double newX, double newY)>(exlop);

            double x = 0.0;
            double y = 0.0;
            foreach (var kvp in exlop)
            {
                Bbutton leafButton = kvp.Key;
                (double newX, double newY) = kvp.Value;
                //AnimateLines(exlop);
                /* MessageBox.Show(
                    leafButton.Content.ToString());*/
                if (leafButton == button)
                { x = newX; y = newY; }
            }
            DoubleAnimation moveXAnimation = new DoubleAnimation
            {
                To = x,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation moveYAnimation = new DoubleAnimation
            {
                To = y,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard moveStoryboard = new Storyboard();
            Storyboard.SetTarget(moveXAnimation, button);
            Storyboard.SetTargetProperty(moveXAnimation, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTarget(moveYAnimation, button);
            Storyboard.SetTargetProperty(moveYAnimation, new PropertyPath("(Canvas.Top)"));

            moveStoryboard.Children.Add(moveXAnimation);
            moveStoryboard.Children.Add(moveYAnimation);

            // Update line positions during animation


            AnimateLines(exlopCopy);
            moveStoryboard.Begin();

            

        }

        private void AnimateLine(Dictionary<Bbutton, (double newX, double newY)> newPositions, Bbutton button)
        {
            foreach (var kvp in newPositions)
            {
                Bbutton leafButton = kvp.Key;
                (double newX, double newY) = kvp.Value;

                if (leafButton == button)
                {

                }
            }
        }


        private void AnimateLines(Dictionary<Bbutton, (double newX, double newY)> exlop)
        {
            foreach (var kvp in exlop)
            {
                Bbutton leafButton = kvp.Key;
                (double newX, double newY) = kvp.Value;

                Debug.WriteLine($"Key: {leafButton.Content}, Value: {kvp.Value}");

                // Check if the parent exists
                if (leafButton.parent == null) { continue; }

                // Safe cast to Bbutton
                var parentButton = leafButton.parent as Bbutton;
                if (parentButton == null) { continue; }

                // Get the current position of the parent button (center point)
                (double papX, double papY) = exlop[parentButton];
                double parentX = papX + parentButton.Width / 2;
                double parentY = papY + parentButton.Height / 2;

                // Get the new position of the child button (center point)
                double childX = newX + leafButton.Width / 2;
                double childY = newY + leafButton.Height / 2;

                // Calculate the angle from the parent to the child
                double anglee = Math.Atan2(childY - parentY, childX - parentX);

                // Calculate the radius of the buttons
                double parentRadius = parentButton.Width / 2;
                double childRadius = leafButton.Width / 2;

                // Calculate the new points on the edges of the buttons
                double newParentX = parentX + parentRadius * Math.Cos(anglee);
                double newParentY = parentY + parentRadius * Math.Sin(anglee);
                double newChildX = childX - childRadius * Math.Cos(anglee);
                double newChildY = childY - childRadius * Math.Sin(anglee);

                // Check if connectionLine exists
                if (leafButton.connectionLine != null)
                {
                    // Create animations from the current positions to the new target positions
                    DoubleAnimation x1Animation = new DoubleAnimation(newParentX, TimeSpan.FromMilliseconds(240));
                    DoubleAnimation y1Animation = new DoubleAnimation(newParentY, TimeSpan.FromMilliseconds(240));
                    DoubleAnimation x2Animation = new DoubleAnimation(newChildX, TimeSpan.FromMilliseconds(240));
                    DoubleAnimation y2Animation = new DoubleAnimation(newChildY, TimeSpan.FromMilliseconds(240));

                    // Apply the animations to the connection line
                    leafButton.connectionLine.BeginAnimation(Line.X1Property, x1Animation);
                    leafButton.connectionLine.BeginAnimation(Line.Y1Property, y1Animation);
                    leafButton.connectionLine.BeginAnimation(Line.X2Property, x2Animation);
                    leafButton.connectionLine.BeginAnimation(Line.Y2Property, y2Animation);
                }
            }
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
                        Stroke = System.Windows.Media.Brushes.Black,
                        StrokeThickness = 2,
                        X1 = x,
                        Y1 = y,
                        X2 = x,
                        Y2 = y,

                    };
                    // Create child node button

                    Bbutton childButton = new Bbutton(parentButton, dist, ang, line,false)
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
                        CenterButton(closestButton as Bbutton);
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

                private void CenterButton(Bbutton button)
                {
                    double offsetX = (mindMapCanvas.ActualWidth / 2) - (Canvas.GetLeft(button) + button.Width / 2);
                    double offsetY = (mindMapCanvas.ActualHeight / 2) - (Canvas.GetTop(button) + button.Height / 2);
                    exlop = CalculateButtonPositionsAfterMove(button as Bbutton);
                    foreach (UIElement element in mindMapCanvas.Children)
                    {
                        if (element.GetType() == typeof(Bbutton))
                        {
                    //AnimateLines(xelop);
                            AnimateButton((element as Bbutton),exlop);
                            //Canvas.SetLeft(element, Canvas.GetLeft(element) + offsetX);
                            // Canvas.SetTop(element, Canvas.GetTop(element) + offsetY);
                        }
                    }
                    if ((button as Bbutton).isRoot) { UpdateButtonPositions(); }
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
                                    // AnimateLine(leafButton.connectionLine, newParentX, newParentY, 0);
                                    // AnimateLine(leafButton.connectionLine, newChildX, newChildY, 1);
                                    // Update the line's X1, Y1 (parent edge) and X2, Y2 (child edge)
                                    //*PrintDialog(leafButton.coordinates[0]);
                                    // AnimateLine(leafButton.connectionLine, newParentX,newParentY,0);
                                    //leafButton.connectionLine.X1 = newParentX;// leafButton.coordinates[0];
                                    //leafButton.connectionLine.Y1 = newParentY; //leafButton.coordinates[1];
                                    // AnimateLine(leafButton.connectionLine, newChildX,newChildY ,1);
                                    // leafButton.connectionLine.X2 = newChildX;
                                    // leafButton.connectionLine.Y2 = newChildY;
                                }

                            }
                        }
                    }
                }

                public void UpdateButtonPositions()
                {
                    if (mindMapCanvas.Children.Count == 0) return;

                    Button rootButton = (Button)mindMapCanvas.Children[0];
                    //AnimateButton(rootButton, (mindMapCanvas.ActualWidth - rootButton.Width) / 2, (mindMapCanvas.ActualHeight - rootButton.Height) / 2);
                    Canvas.SetLeft(rootButton, (mindMapCanvas.ActualWidth - rootButton.Width) / 2);
                    Canvas.SetTop(rootButton, (mindMapCanvas.ActualHeight - rootButton.Height) / 2);

                    for (int i = 1; i < mindMapCanvas.Children.Count; i++)
                    {
                        if (mindMapCanvas.Children[i] is Bbutton leafButton)
                        {
                            double angle = leafButton.angle;
                            double x = (mindMapCanvas.ActualWidth / 2) + Math.Cos(angle * Math.PI / 180) * leafButton.distance;
                            double y = (mindMapCanvas.ActualHeight / 2) + Math.Sin(angle * Math.PI / 180) * leafButton.distance;

                            //AnimateButton(leafButton, x - leafButton.Width / 2, y - leafButton.Height / 2);
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
                                //leafButton.connectionLine.X1 = newParentX;// leafButton.coordinates[0];
                                //leafButton.connectionLine.Y1 = newParentY; //leafButton.coordinates[1];

                                //leafButton.connectionLine.X2 = newChildX;
                                //leafButton.connectionLine.Y2 = newChildY;
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