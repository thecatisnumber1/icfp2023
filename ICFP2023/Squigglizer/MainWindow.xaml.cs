﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ICFP2023.ProblemSpec;
using WinPoint = System.Windows.Point;

namespace ICFP2023
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SquigglizerSettings settings;
        private ProblemCatalog _allProblems;
        private ProblemSpec _currentProblem;
        private CancellationTokenSource _tokenSource;

        private double PersonSizePx = 10.0;

        #region Colors
        private static SolidColorBrush BlackBrush = new SolidColorBrush(Colors.Black);
        private static SolidColorBrush StageFillBrush = new SolidColorBrush(Colors.LightGray);

        private static SolidColorBrush UnselectedAttendeeBorderBrush = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush UnselectedAttendeeFillBrush = new SolidColorBrush(Colors.Salmon);

        // private static SolidColorBrush UnselectedMusicianBorderBrush = new SolidColorBrush(Colors.Blue);
        // private static SolidColorBrush UnselectedMusicianFillBrush = new SolidColorBrush(Colors.LightBlue);

        private static SolidColorBrush SelectedPersonBorderBrush = new SolidColorBrush(Colors.Green);
        private static SolidColorBrush SelectedPersonFillBrush = new SolidColorBrush(Colors.LightGreen);

        private static Color GoodScoreColor = Colors.Green;
        private static Color BadScoreColor = Colors.Red;
        private static List<Color> MagicGradient = new List<Color>();
        private const int MagicGradientSteps = 1024;
        #endregion

        #region Control Tracking for Highlighting
        private Dictionary<Shape, (Musician Musician, SolidColorBrush FillBrush)> _musicianShapeToMusician = new Dictionary<Shape, (Musician Musician, SolidColorBrush FillBrush)>();
        private Dictionary<Musician, Shape> _musicianToShape = new Dictionary<Musician, Shape>();

        private Dictionary<Shape, Attendee> _attendeeShapeToAttendee = new Dictionary<Shape, Attendee>();
        private Dictionary<Attendee, Shape> _attendeeToShape = new Dictionary<Attendee, Shape>();
        #endregion

        private Solution _currentSolution;

        public MainWindow()
        {
            InitializeComponent();

            settings = new SquigglizerSettings();
            settings.ParseArgs(Environment.GetCommandLineArgs()[1..]);
            SettingsControl.Settings = settings;

            _allProblems = new ProblemCatalog();
            ProblemSelector.ItemsSource = _allProblems.Names;

            string[] solverList = Solvers.Names();
            SolverSelector.ItemsSource = solverList;

            // Have some default selected
            if (ProblemSelector.SelectedIndex < 0)
            {
                ProblemSelector.SelectedIndex = 0;
            }

            if (SolverSelector.SelectedIndex < 0)
            {
                SolverSelector.SelectedIndex = 0;
            }

            int size = MagicGradientSteps / 2;
            for (int i = 0; i < size; i++)
            {
                var rAverage = (byte)(BadScoreColor.R + (byte)((Colors.White.R - BadScoreColor.R) * i / size));
                var gAverage = (byte)(BadScoreColor.G + (byte)((Colors.White.G - BadScoreColor.G) * i / size));
                var bAverage = (byte)(BadScoreColor.B + (byte)((Colors.White.B - BadScoreColor.B) * i / size));
                MagicGradient.Add(Color.FromArgb(255, rAverage, gAverage, bAverage));
            }

            for (int i = 0; i < size; i++)
            {
                var rAverage = (byte)(Colors.White.R + (byte)((GoodScoreColor.R - Colors.White.R) * i / size));
                var gAverage = (byte)(Colors.White.G + (byte)((GoodScoreColor.G - Colors.White.G) * i / size));
                var bAverage = (byte)(Colors.White.B + (byte)((GoodScoreColor.B - Colors.White.B) * i / size));
                MagicGradient.Add(Color.FromArgb(255, rAverage, gAverage, bAverage));
            }
        }

        private void RenderProblem(ProblemSpec problem)
        {
            BaseRender.Children.Clear();
            MusicianRender.Children.Clear();
            _musicianShapeToMusician.Clear();
            _musicianToShape.Clear();
            _attendeeShapeToAttendee.Clear();
            _attendeeToShape.Clear();
            ZoomArea.Reset();

            // Test hackery
            BaseRender.Width = problem.RoomWidth; // Should be problem width/height eventually
            BaseRender.Height = problem.RoomHeight;

            MusicianRender.Width = problem.RoomWidth; // You'll need to check whether manual placement is on the stage on your own
            MusicianRender.Height = problem.RoomHeight;

            // Dynamic dot sizes!? Should be about 0.5% of the long axis, but no more than 20 because musicians' no-touching zones (radius 10, diameter 20)
            double longAxis = Math.Max(problem.RoomWidth, problem.RoomHeight);
            // PersonSizePx = Math.Min(longAxis / 200.0, 20.0);

            // Lazy-assed thing to make everything hit-testable
            Rectangle hack = new Rectangle();
            hack.Width = problem.RoomWidth;
            hack.Height = problem.RoomHeight;
            hack.Fill = new SolidColorBrush(Colors.White);
            hack.Opacity = 0;
            BaseRender.Children.Add(hack);

            // Border
            Rectangle border = new Rectangle();
            border.Width = problem.RoomWidth;
            border.Height = problem.RoomHeight;
            border.Stroke = BlackBrush;
            border.StrokeThickness = 1;
            BaseRender.Children.Add(border);

            // Stage
            Rectangle stage = new Rectangle();
            stage.Width = problem.StageWidth;
            stage.Height = problem.StageHeight;
            stage.Stroke = BlackBrush;
            stage.Fill = StageFillBrush;
            Canvas.SetBottom(stage, problem.StageBottomLeft.Y);
            Canvas.SetLeft(stage, problem.StageBottomLeft.X);
            BaseRender.Children.Add(stage);

            // Do not place the centers of your musician dots between this thing and the stage
            Rectangle innerStage = new Rectangle();
            innerStage.Width = problem.StageWidth - 10.0;
            innerStage.Height = problem.StageHeight - 10.0;
            innerStage.Stroke = new SolidColorBrush(Colors.Blue);
            innerStage.Fill = new SolidColorBrush(Colors.Transparent);
            Canvas.SetBottom(innerStage, problem.StageBottomLeft.Y + 5);
            Canvas.SetLeft(innerStage, problem.StageBottomLeft.X + 5);
            BaseRender.Children.Add(innerStage);

            double minAttendeeX = problem.RoomWidth;
            double minAttendeeY = problem.RoomHeight;
            double maxAttendeeX = 0;
            double maxAttendeeY = 0;

            // Attendees
            foreach (Attendee a in problem.Attendees)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = PersonSizePx;
                ellipse.Height = PersonSizePx;
                ellipse.Stroke = UnselectedAttendeeBorderBrush;
                ellipse.Fill = UnselectedAttendeeFillBrush;
                Canvas.SetTop(ellipse, problem.RoomHeight - a.Location.Y - PersonSizePx / 2);
                Canvas.SetLeft(ellipse, a.Location.X - PersonSizePx / 2);
                ellipse.ToolTip = string.Join(", ", a.Tastes);
                ellipse.MouseLeftButtonDown += AttendeeBubble_MouseDown;
                _attendeeShapeToAttendee.Add(ellipse, a);
                _attendeeToShape.Add(a, ellipse);

                BaseRender.Children.Add(ellipse);

                // Do some stuff for figuring out initial transform.
                minAttendeeX = Math.Min(minAttendeeX, a.Location.X);
                maxAttendeeX = Math.Max(maxAttendeeX, a.Location.X);
                minAttendeeY = Math.Min(minAttendeeY, a.Location.Y);
                maxAttendeeY = Math.Max(maxAttendeeY, a.Location.Y);
            }

            // Account for stage maybe not with the attendees for initial transform.
            minAttendeeX = Math.Min(minAttendeeX, problem.StageBottomLeft.X);
            minAttendeeY = Math.Min(minAttendeeY, problem.StageBottomLeft.Y);
            maxAttendeeX = Math.Max(maxAttendeeX, problem.StageBottomLeft.X + problem.StageWidth);
            maxAttendeeY = Math.Max(maxAttendeeY, problem.StageBottomLeft.Y + problem.StageHeight);

            // Now try to transform stuff. I haaaate math.
            // Ratio of width and height for scale transform
            double widthRatio = problem.RoomWidth / (maxAttendeeX - minAttendeeX);
            double heightRatio = problem.RoomHeight / (maxAttendeeY - minAttendeeY);
            double scaleRatio = Math.Min(widthRatio, heightRatio);

            // Apply transform.
            var scale = (ScaleTransform)((TransformGroup)ZoomGrid.RenderTransform).Children.First(tr => tr is ScaleTransform);
            var translate = (TranslateTransform)((TransformGroup)ZoomGrid.RenderTransform).Children.First(tr => tr is TranslateTransform);

            translate.X = minAttendeeX * scaleRatio;
            translate.Y = (-(problem.RoomHeight - maxAttendeeY) + 1) * scaleRatio;

            scale.ScaleX = scaleRatio;
            scale.ScaleY = scaleRatio;
        }

        private void ResetProblem()
        {
            _currentProblem = _allProblems.GetSpec(ProblemSelector.SelectedItem.ToString());
            RenderProblem(_currentProblem);
        }

        private void ProblemSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetProblem();
        }

        private void SolverSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SolverRunButton.IsEnabled = true;
        }

        private void SolverRunButton_OnClick(object sender, RoutedEventArgs e)
        {
            Solvers.Solver solver = Solvers.GetSolver(SolverSelector.SelectedItem.ToString());

            SolverRunButton.IsEnabled = false;
            SolverSelector.IsEnabled = false;
            ProblemSelector.IsEnabled = false;

            _tokenSource = new CancellationTokenSource();

            var adapter = new UIAdapterImpl(this, _tokenSource.Token);

            RunTask(() =>
            {
                solver.Invoke(_currentProblem, settings, adapter);

                // This will totally screw me over later, but it lets a final Render call go through.
                Task.Delay(50).Wait();

                // Solver has finished, but if they ran to completion, the UI logger might still be pumping. Kill it with our token.
                if (!_tokenSource.IsCancellationRequested)
                {
                    _tokenSource.Cancel();
                }

                // Cleanup UI on the main thread.
                Dispatcher.BeginInvoke(() =>
                {
                    Console.WriteLine("Done!");
                    SolverSelector.IsEnabled = true;
                    ProblemSelector.IsEnabled = true;
                    SolverRunButton.IsEnabled = true;
                });
            });
        }

        // In debug mode an exception will properly trigger a break in the debugger if "Just My Code" is on.
        // It doesn't appear there's any way to get this behavior in release mode with optimizations on.
        // At least the logging will make sure the user doesn't just sit there wondering why nothing happened.
        private static Task RunTask(Action action)
        {
            return Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                    throw;
                }
            });
        }

        // This blindly assumes your solution has the correct problemspec. If it doesn't, you're hosed.
        internal void RenderSolution(Solution solution)
        {
            _currentSolution = solution;
            //_ = _currentSolution.InitializeScore(); // Not doing anything with the total score for now.

            _musicianShapeToMusician.Clear();
            _musicianToShape.Clear();
            MusicianRender.Children.Clear();

            long minEffect = -1;
            long maxEffect = 1;
            // Compute min/max for gradient purposes
            for (int i = 0; i < solution.Placements.Count; i++)
            {
                long effect = _currentSolution.GetScoreForMusician(i);
                minEffect = Math.Min(effect, minEffect);
                maxEffect = Math.Max(effect, maxEffect);
            }

            for (int i = 0; i < solution.Placements.Count; i++)
            {
                Point p = solution.Placements[i];

                // Actual musician
                var ellipse = new Ellipse();
                ellipse.Width = PersonSizePx;
                ellipse.Height = PersonSizePx;
                ellipse.Stroke = BlackBrush;

                SolidColorBrush fillBrush;
                long effect = _currentSolution.GetScoreForMusician(i);
                ellipse.ToolTip = effect.ToString();

                int index = 0;
                int half = MagicGradientSteps / 2;
                if (effect == 0) {
                    index = half;
                }
                else if (effect < 0)
                {
                    index = half - (int)((double)effect / minEffect * half);
                }
                else
                {
                    index = half + (int)((double)effect / maxEffect * half) - 1;
                }
                var brush = new SolidColorBrush(MagicGradient[index]);
                ellipse.Fill = brush; // More negative = more red. More positive = more green.

                Canvas.SetTop(ellipse, _currentProblem.RoomHeight - p.Y - PersonSizePx / 2);
                Canvas.SetLeft(ellipse, p.X - PersonSizePx / 2);
                ellipse.MouseLeftButtonDown += MusicianBubble_MouseDown;
                Musician m = _currentProblem.Musicians.Single(m => m.Index == i);
                _musicianShapeToMusician.Add(ellipse, (m, brush));
                _musicianToShape.Add(m, ellipse);

                MusicianRender.Children.Add(ellipse);
            }
        }

        private void MusicianBubble_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ResetPeople();

            // User clicks a musician. Find/highlight the musician.
            Shape musicianBubble = (Shape)sender;
            musicianBubble.Stroke = SelectedPersonBorderBrush;
            musicianBubble.Fill = SelectedPersonFillBrush;

            // Highlight any attendees that can hear them.
            Musician sourceMusician = _musicianShapeToMusician[musicianBubble].Musician;
            List<Musician> otherMusicians = _musicianToShape.Keys.Where(m => m != sourceMusician).ToList();
            
            foreach (var attendee in _attendeeToShape)
            {
                Point a = attendee.Key.Location;
                // If any musician blocks, continue
                bool blocked = false;
                foreach (Musician otherMusician in otherMusicians)
                {
                    if (_currentSolution.IsMusicianBlocked(a, sourceMusician, otherMusician))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                {
                    attendee.Value.Stroke = SelectedPersonBorderBrush;
                    attendee.Value.Fill = SelectedPersonFillBrush;

                    // TODO: Draw ray
                }
            }
        }

        private void AttendeeBubble_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ResetPeople();

            Shape attendeeBubble = (Shape)sender;
            attendeeBubble.Stroke = SelectedPersonBorderBrush;
            attendeeBubble.Fill = SelectedPersonFillBrush;

            // Highlight the musicians they can hear
            Attendee attendee = _attendeeShapeToAttendee[attendeeBubble];
            Point p = attendee.Location;

            List<Musician> allMusicians = _musicianShapeToMusician.Values.Select(x => x.Musician).ToList();
            foreach (Musician sourceMusician in allMusicians)
            {
                bool blocked = false;
                foreach (Musician otherMusician in allMusicians.Where(m => m != sourceMusician))
                {
                    if (_currentSolution.IsMusicianBlocked(p, sourceMusician, otherMusician))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                {
                    Shape musicianBubble = _musicianToShape[sourceMusician];
                    musicianBubble.Stroke = SelectedPersonBorderBrush;
                    musicianBubble.Fill = SelectedPersonFillBrush;

                    // TODO: Draw ray
                }
            }

        }

        private void ResetPeople()
        {
            foreach (Shape attendeeBubble in _attendeeShapeToAttendee.Keys)
            {
                attendeeBubble.Stroke = UnselectedAttendeeBorderBrush;
                attendeeBubble.Fill = UnselectedAttendeeFillBrush;
            }

            foreach (var musician in _musicianShapeToMusician)
            {
                Shape musicianBubble = musician.Key;
                musicianBubble.Stroke = BlackBrush;
                musicianBubble.Fill = musician.Value.FillBrush;
            }
        }

        private void ManualMove_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WinPoint cursorPosition = e.GetPosition(ZoomArea);
            // Ignore stuff outside of the zoom area's bounds
            if (!IsInZoomArea(e))
            {
                return;
            }
            
        }

        private bool IsInZoomArea(MouseButtonEventArgs e)
        {
            WinPoint cursorPosition = e.GetPosition(ZoomArea);
            return !(cursorPosition.X < 0 || cursorPosition.X > ZoomArea.ActualWidth || cursorPosition.Y < 0 || cursorPosition.Y > ZoomArea.ActualHeight);
        }

        internal void SetMusicianColor(int index, string color)
        {
            Musician m = _currentSolution.Problem.Musicians[index];
            Shape s = _musicianToShape[m];
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

            s.Fill = brush;
            _musicianShapeToMusician[s] = (m, brush); // This may have to change. Score heatmap data is lost.
        }

        internal void ClearMusicianColor(int index)
        {
            SetMusicianColor(index, "White"); // This is insanely inefficient instead of using a const brush.
        }

        internal void ClearAllColors()
        {
            // Doesn't actually line up, but this we're clearing everything so who cares.
            for (int i = 0; i < _currentSolution.Problem.Musicians.Count; i++)
            {
                ClearMusicianColor(i);
            }
        }
    }
}
