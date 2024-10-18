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
using System.Windows.Media;

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
        public Dictionary<UIElement, object> startElements = new Dictionary<UIElement, object>();
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
            startElements.Add(rootButton,((mindMapCanvas.ActualWidth - rootButton.Width) / 2, (mindMapCanvas.ActualHeight - rootButton.Height) / 2));
          //  Canvas.SetLeft(rootButton, (mindMapCanvas.ActualWidth - rootButton.Width) / 2);
            //Canvas.SetTop(rootButton, (mindMapCanvas.ActualHeight - rootButton.Height) / 2);
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

                if (leafButton == button)
                {
                    x = newX;
                    y = newY;
                }
            }

            // Create X and Y animations using keyframes
            DoubleAnimationUsingKeyFrames moveXAnimation = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(300)
            };
            moveXAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(x, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));

            DoubleAnimationUsingKeyFrames moveYAnimation = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(300)
            };
            moveYAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(y, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));

            Storyboard moveStoryboard = new Storyboard();
            Storyboard.SetTarget(moveXAnimation, button);
            Storyboard.SetTargetProperty(moveXAnimation, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTarget(moveYAnimation, button);
            Storyboard.SetTargetProperty(moveYAnimation, new PropertyPath("(Canvas.Top)"));

            moveStoryboard.Children.Add(moveXAnimation);
            moveStoryboard.Children.Add(moveYAnimation);

            // Sync lines with button movement using keyframes
            AnimateLines(exlopCopy, moveStoryboard);

            moveStoryboard.Begin();
        }





        private void AnimateLines(Dictionary<Bbutton, (double newX, double newY)> exlop, Storyboard storyboard)
        {
            foreach (var kvp in exlop)
            {
                Bbutton leafButton = kvp.Key;
                (double newX, double newY) = kvp.Value;

            //    Debug.WriteLine($"Key: {leafButton.Content}, Value: {kvp.Value}");

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
                    // Create keyframe animations from the current positions to the new target positions
                    DoubleAnimationUsingKeyFrames x1Animation = new DoubleAnimationUsingKeyFrames();
                    x1Animation.KeyFrames.Add(new LinearDoubleKeyFrame(newParentX, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));

                    DoubleAnimationUsingKeyFrames y1Animation = new DoubleAnimationUsingKeyFrames();
                    y1Animation.KeyFrames.Add(new LinearDoubleKeyFrame(newParentY, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));

                    DoubleAnimationUsingKeyFrames x2Animation = new DoubleAnimationUsingKeyFrames();
                    x2Animation.KeyFrames.Add(new LinearDoubleKeyFrame(newChildX, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));

                    DoubleAnimationUsingKeyFrames y2Animation = new DoubleAnimationUsingKeyFrames();
                    y2Animation.KeyFrames.Add(new LinearDoubleKeyFrame(newChildY, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));

                    // Apply the animations to the connection line
                    Storyboard.SetTarget(x1Animation, leafButton.connectionLine);
                    Storyboard.SetTargetProperty(x1Animation, new PropertyPath(Line.X1Property));

                    Storyboard.SetTarget(y1Animation, leafButton.connectionLine);
                    Storyboard.SetTargetProperty(y1Animation, new PropertyPath(Line.Y1Property));

                    Storyboard.SetTarget(x2Animation, leafButton.connectionLine);
                    Storyboard.SetTargetProperty(x2Animation, new PropertyPath(Line.X2Property));

                    Storyboard.SetTarget(y2Animation, leafButton.connectionLine);
                    Storyboard.SetTargetProperty(y2Animation, new PropertyPath(Line.Y2Property));

                    // Add animations to the existing storyboard to keep everything in sync
                    storyboard.Children.Add(x1Animation);
                    storyboard.Children.Add(y1Animation);
                    storyboard.Children.Add(x2Animation);
                    storyboard.Children.Add(y2Animation);
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
                        X1 = 0,
                        Y1 = 0,
                        X2 = 0,
                        Y2 = 0,

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
                    startElements.Add(childButton,(x - childButton.Width / 2, y - childButton.Height / 2));
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
        public void AddLines()
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
                        
                        startElements.Add(leafButton.connectionLine,(newParentX,newParentY,newChildX,newChildY));
                       
                        //leafButton.connectionLine.X1 = newParentX;// leafButton.coordinates[0];
                        //leafButton.connectionLine.Y1 = newParentY; //leafButton.coordinates[1];
                       
                        // leafButton.connectionLine.X2 = newChildX;
                        // leafButton.connectionLine.Y2 = newChildY;
                    }

                }
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
            if ((button as Bbutton).isRoot) { AnimateButton(button, exlop); }//UpdateButtonPositions(); }
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

            // Center the root button
            Canvas.SetLeft(rootButton, (mindMapCanvas.ActualWidth - rootButton.Width) / 2);
            Canvas.SetTop(rootButton, (mindMapCanvas.ActualHeight - rootButton.Height) / 2);

            // Apply animation to rootButton
            StartButton(rootButton);

            // Position and animate other buttons
            for (int i = 1; i < mindMapCanvas.Children.Count; i++)
            {
                if (mindMapCanvas.Children[i] is Button leafButton && leafButton is Bbutton customLeafButton)
                {
                    double angle = customLeafButton.angle;
                    double x = (mindMapCanvas.ActualWidth / 2) + Math.Cos(angle * Math.PI / 180) * customLeafButton.distance;
                    double y = (mindMapCanvas.ActualHeight / 2) + Math.Sin(angle * Math.PI / 180) * customLeafButton.distance;

                    // Position the leaf button
                    Canvas.SetLeft(leafButton, x - leafButton.Width / 2);
                    Canvas.SetTop(leafButton, y - leafButton.Height / 2);

                    // Apply animation to each leaf button
                    StartButton(leafButton);
                        // Update the position of the connection line
                        if (customLeafButton.connectionLine != null)
                        {
                            // Get the current position of the parent button (center point)
                            double parentX = Canvas.GetLeft(customLeafButton.parent) + customLeafButton.parent.Width / 2;
                            double parentY = Canvas.GetTop(customLeafButton.parent) + customLeafButton.parent.Height / 2;

                            // Get the current position of the child button (center point)
                            double childX = Canvas.GetLeft(leafButton) + leafButton.Width / 2;
                            double childY = Canvas.GetTop(leafButton) + leafButton.Height / 2;

                            // Calculate the angle from the parent to the child
                            double anglee = Math.Atan2(childY - parentY, childX - parentX);

                            // Calculate the radius of the buttons (assuming they're circular, i.e., using Width/2)
                            double parentRadius = customLeafButton.parent.Width / 2;
                            double childRadius = leafButton.Width / 2;

                            // Calculate the new points on the edges of the buttons based on the angle
                            double newParentX = parentX + parentRadius * Math.Cos(anglee);
                            double newParentY = parentY + parentRadius * Math.Sin(anglee);

                            double newChildX = childX - childRadius * Math.Cos(anglee);  // Subtract to adjust for direction
                            double newChildY = childY - childRadius * Math.Sin(anglee);  // Subtract to adjust for direction

                            // Save the initial coordinates if not already set
                            if (!customLeafButton.isSet)
                            {
                            customLeafButton.coordinates[0] = newParentX;
                            customLeafButton.coordinates[1] = newParentY;
                            customLeafButton.isSet = true;
                            }
                            customLeafButton.connectionLine.X1 = newParentX;
                        customLeafButton.connectionLine.Y1 = newParentY;    
                            // Animate the line from the initial coordinates to the new coordinates
                          //  AnimateLine(customLeafButton.connectionLine, newParentX, newParentY);  // Animate X1, Y1 (parent edge)
                            AnimateLine(customLeafButton.connectionLine, newChildX, newChildY);    // Animate X2, Y2 (child edge)
                        }
                    

                }
            }
        }

        // Function to animate the line growing from X1, Y1 (start) to X2, Y2 (end)
        void AnimateLine(Line line, double toX, double toY)
        {
            // Set initial X2 and Y2 to be the same as X1 and Y1 (line starts as a point)
            line.X2 = line.X1;
            line.Y2 = line.Y1;

            // Create animations for X2 and Y2
            DoubleAnimation animateX2 = new DoubleAnimation
            {
                To = toX,  // Animate to the final X2 position
                Duration = TimeSpan.FromSeconds(0.5),  // Animation duration
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation animateY2 = new DoubleAnimation
            {
                To = toY,  // Animate to the final Y2 position
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Create a storyboard to group the animations
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animateX2);
            storyboard.Children.Add(animateY2);

            // Set the target of the animations to the line
            Storyboard.SetTarget(animateX2, line);
            Storyboard.SetTarget(animateY2, line);

            // Animate X2 and Y2 to grow the line from X1, Y1 to X2, Y2
            Storyboard.SetTargetProperty(animateX2, new PropertyPath("X2"));
            Storyboard.SetTargetProperty(animateY2, new PropertyPath("Y2"));

            // Begin the animation
            storyboard.Begin();
        }




        public void StartButton(Button button)
        {
            // Create a scale transform
            ScaleTransform scaleTransform = new ScaleTransform(0, 0);
            button.RenderTransform = scaleTransform;
            button.RenderTransformOrigin = new Point(0.5, 0.5);  // Scale from the center of the button

            // Create the animation for the scale transform
            var scaleXAnimation = new DoubleAnimation
            {
                From = 0, // Start from 0 (invisible)
                To = 1,   // Grow to full size
                Duration = TimeSpan.FromSeconds(0.5), // Animation duration
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } // Smooth ease-out effect
            };

            var scaleYAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // Create the storyboard to apply both X and Y scale animations
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(scaleXAnimation);
            storyboard.Children.Add(scaleYAnimation);

            // Set the target of the animations to the button's ScaleTransform
            Storyboard.SetTarget(scaleXAnimation, button);
            Storyboard.SetTarget(scaleYAnimation, button);

            // Set the properties to target (scaleX and scaleY)
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));

            // Begin the animation
            storyboard.Begin();
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