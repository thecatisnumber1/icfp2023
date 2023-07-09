using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
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

        private static SolidColorBrush PillarBorderBrush = new SolidColorBrush(Colors.Gray);
        private static SolidColorBrush PillarFillBrush = new SolidColorBrush(Colors.LightGray);

        // private static SolidColorBrush UnselectedMusicianBorderBrush = new SolidColorBrush(Colors.Blue);
        // private static SolidColorBrush UnselectedMusicianFillBrush = new SolidColorBrush(Colors.LightBlue);

        private static SolidColorBrush SelectedPersonBorderBrush = new SolidColorBrush(Colors.Green);
        private static SolidColorBrush SelectedPersonFillBrush = new SolidColorBrush(Colors.LightGreen);

        private static SolidColorBrush TransparentBrush = new SolidColorBrush(Colors.Transparent);

        private static Color GoodScoreColor = Colors.Green;
        private static Color BadScoreColor = Colors.Red;
        private static List<SolidColorBrush> MagicGradient = new List<SolidColorBrush>();
        private const int MagicGradientSteps = 1024;
        #endregion

        #region Control Tracking for Highlighting
        private Dictionary<Shape, (Musician Musician, SolidColorBrush FillBrush)> _musicianShapeToMusician = new Dictionary<Shape, (Musician Musician, SolidColorBrush FillBrush)>();
        private Dictionary<Musician, Shape> _musicianToShape = new Dictionary<Musician, Shape>();

        private Dictionary<Shape, (Attendee Attendee, SolidColorBrush OriginalColor)> _attendeeShapeToAttendee = new Dictionary<Shape, (Attendee Attendee, SolidColorBrush OriginalColor)>();
        private Dictionary<Attendee, Shape> _attendeeToShape = new Dictionary<Attendee, Shape>();
        #endregion

        #region Hacks for Manual Placement
        private Rectangle _stageOverlayRect;
        #endregion

        private Dictionary<int, Ellipse> AttendeeToEllipse;

        private Solution _currentSolution;
        private AudienceColorizers.Colorizer _currentAudienceColorizer;

        // Change this to false if you want to bound to audience + stage.
        private bool AutoZoomToStage = true;

        public MainWindow()
        {
            InitializeComponent();

            settings = new SquigglizerSettings();
            settings.ParseArgs(Environment.GetCommandLineArgs()[1..]);
            SettingsControl.Settings = settings;

            _allProblems = new ProblemCatalog();
            var idToNames = _allProblems.Names.ToDictionary(name => int.Parse(name.Substring(name.IndexOf('-') + 1)), name => name);
            List<int> ids = idToNames.Keys.ToList();
            ids.Sort();
            List<string> sortedNames = new();
            foreach (int idx in ids)
            {
                sortedNames.Add(idToNames[idx]);
            }
            ProblemSelector.ItemsSource = sortedNames;

            string[] solverList = Solvers.Names();
            SolverSelector.ItemsSource = solverList;

            string[] colorizerList = AudienceColorizers.Names();
            ColorizerSelector.ItemsSource = colorizerList;

            // Initialize the magic gradient before things can try to use it
            int size = MagicGradientSteps / 2;
            for (int i = 0; i < size; i++)
            {
                var rAverage = (byte)(BadScoreColor.R + (byte)((Colors.White.R - BadScoreColor.R) * i / size));
                var gAverage = (byte)(BadScoreColor.G + (byte)((Colors.White.G - BadScoreColor.G) * i / size));
                var bAverage = (byte)(BadScoreColor.B + (byte)((Colors.White.B - BadScoreColor.B) * i / size));
                MagicGradient.Add(new SolidColorBrush(Color.FromArgb(255, rAverage, gAverage, bAverage)));
            }

            for (int i = 0; i < size; i++)
            {
                var rAverage = (byte)(Colors.White.R + (byte)((GoodScoreColor.R - Colors.White.R) * i / size));
                var gAverage = (byte)(Colors.White.G + (byte)((GoodScoreColor.G - Colors.White.G) * i / size));
                var bAverage = (byte)(Colors.White.B + (byte)((GoodScoreColor.B - Colors.White.B) * i / size));
                MagicGradient.Add(new SolidColorBrush(Color.FromArgb(255, rAverage, gAverage, bAverage)));
            }

            // Have some default selected
            if (ProblemSelector.SelectedIndex < 0)
            {
                ProblemSelector.SelectedIndex = 0;
            }

            if (SolverSelector.SelectedIndex < 0)
            {
                SolverSelector.SelectedIndex = 0;
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

            double longAxis = Math.Max(problem.RoomWidth, problem.RoomHeight);

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

            // Add a hack control that's the size of the stage for hit-detection on the stage
            _stageOverlayRect = new Rectangle();
            _stageOverlayRect.Width = _currentProblem.StageWidth - 10;
            _stageOverlayRect.Height = _currentProblem.StageHeight - 10;
            _stageOverlayRect.Fill = TransparentBrush;
            Canvas.SetTop(_stageOverlayRect, _currentProblem.RoomHeight - _currentProblem.StageTop + 5); // Add the no-touching zone.
            Canvas.SetLeft(_stageOverlayRect, _currentProblem.StageLeft + 5);
            MusicianRender.Children.Add(_stageOverlayRect);

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
            // Compute gradient values
            Dictionary<Attendee, double> evaluatedAttendees;
            if (_currentAudienceColorizer != null)
            {
                evaluatedAttendees = _currentAudienceColorizer.Invoke(_currentProblem);
            }
            else
            {
                evaluatedAttendees = _currentProblem.Attendees.ToDictionary(a => a, _ => 0d);
            }

            double audienceMin = double.MaxValue;
            double audienceMax = double.MinValue;

            foreach (var kvp in evaluatedAttendees)
            {
                audienceMin = Math.Min(audienceMin, kvp.Value);
                audienceMax = Math.Max(audienceMax, kvp.Value);
            }

            int ai = 0;
            AttendeeToEllipse = new Dictionary<int, Ellipse>();
            foreach (Attendee a in problem.Attendees)
            {
                Ellipse ellipse = new Ellipse();
                AttendeeToEllipse.Add(a.Index, ellipse);
                ellipse.Width = PersonSizePx;
                ellipse.Height = PersonSizePx;
                ellipse.Stroke = UnselectedAttendeeBorderBrush;
                Canvas.SetTop(ellipse, problem.RoomHeight - a.Location.Y - PersonSizePx / 2);
                Canvas.SetLeft(ellipse, a.Location.X - PersonSizePx / 2);
                ellipse.ToolTip = string.Join(", ", a.Tastes);
                ellipse.MouseLeftButtonDown += AttendeeBubble_MouseDown;
                SolidColorBrush originalBrush = GetBrushFromMagicGradient(evaluatedAttendees[a], audienceMin, audienceMax);
                ellipse.Fill = originalBrush;
                _attendeeShapeToAttendee.Add(ellipse, (a, originalBrush));
                _attendeeToShape.Add(a, ellipse);

                BaseRender.Children.Add(ellipse);

                // Do some stuff for figuring out initial transform.
                if (!AutoZoomToStage)
                {
                    minAttendeeX = Math.Min(minAttendeeX, a.Location.X);
                    maxAttendeeX = Math.Max(maxAttendeeX, a.Location.X);
                    minAttendeeY = Math.Min(minAttendeeY, a.Location.Y);
                    maxAttendeeY = Math.Max(maxAttendeeY, a.Location.Y);
                }
            }

            foreach (Pillar p in problem.Pillars)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = p.Radius * 2;
                ellipse.Height = p.Radius * 2;
                ellipse.Stroke = PillarBorderBrush;
                Canvas.SetTop(ellipse, problem.RoomHeight - p.Center.Y - p.Radius);
                Canvas.SetLeft(ellipse, p.Center.X - p.Radius);
                ellipse.Fill = PillarFillBrush;
                ellipse.Opacity = 0.5;

                BaseRender.Children.Add(ellipse);
            }

            // Account for stage maybe not with the attendees for initial transform.
            double padding = AutoZoomToStage ? 100 : 0; // Make this zero if using the default zoom
            minAttendeeX = Math.Min(minAttendeeX, Math.Max(problem.StageBottomLeft.X - padding, 0));
            minAttendeeY = Math.Min(minAttendeeY, Math.Max(problem.StageBottomLeft.Y - padding, 0));
            maxAttendeeX = Math.Max(maxAttendeeX, Math.Min(problem.StageBottomLeft.X + problem.StageWidth + padding, problem.RoomWidth));
            maxAttendeeY = Math.Max(maxAttendeeY, Math.Min(problem.StageBottomLeft.Y + problem.StageHeight + padding, problem.RoomHeight));

            // Now try to transform stuff. I haaaate math.
            // Ratio of width and height for scale transform
            double widthRatio = problem.RoomWidth / (maxAttendeeX - minAttendeeX);
            double heightRatio = problem.RoomHeight / (maxAttendeeY - minAttendeeY);
            double scaleRatio = Math.Max(widthRatio, heightRatio);

            // Apply transform.
            var scale = (ScaleTransform)((TransformGroup)ZoomGrid.RenderTransform).Children.First(tr => tr is ScaleTransform);
            var translate = (TranslateTransform)((TransformGroup)ZoomGrid.RenderTransform).Children.First(tr => tr is TranslateTransform);

            translate.X = -minAttendeeX * scaleRatio;
            translate.Y = (-(problem.RoomHeight - maxAttendeeY) + 1) * scaleRatio;

            scale.ScaleX = scaleRatio;
            scale.ScaleY = scaleRatio;
        }

        private void ResetProblem()
        {
            string problemName = ProblemSelector.SelectedItem.ToString();
            _currentProblem = _allProblems.GetSpec(problemName);
            RenderProblem(_currentProblem);
            // Load best solution
            string bestSaveFile = $"best-solves/{problemName}.json";
            if (FileUtil.FileExists(bestSaveFile))
            {
                Solution bestSolve = Solution.Read(bestSaveFile, _currentProblem);
                //bestSolve.InitializeScore(); // This will be very slow if you uncomment, but it will show you score gradients.
                RenderSolution(bestSolve);
            }
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

            MusicianRender.Children.Clear();
            _musicianShapeToMusician.Clear();
            _musicianToShape.Clear();

            // Add a hack control that's the size of the stage for hit-detection
            MusicianRender.Children.Add(_stageOverlayRect);

            long minEffect = -1;
            long maxEffect = 1;
            // Compute min/max for gradient purposes
            for (int i = 0; i < solution.Placements.Count; i++)
            {
                long effect = _currentSolution.SupplyGradientToUI(i);
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
                long effect = _currentSolution.SupplyGradientToUI(i);
                ellipse.ToolTip = (solution.GetScoreForMusician(i) / solution.ScoreCache).ToString("P2");

                SolidColorBrush brush = GetBrushFromMagicGradient(effect, minEffect, maxEffect);
                ellipse.Fill = brush; // More negative = more red. More positive = more green.

                Canvas.SetTop(ellipse, _currentProblem.RoomHeight - p.Y - PersonSizePx / 2);
                Canvas.SetLeft(ellipse, p.X - PersonSizePx / 2);
                ellipse.MouseLeftButtonDown += MusicianBubble_MouseDown;
                Musician m = _currentProblem.Musicians.Single(m => m.Index == i);
                _musicianShapeToMusician.Add(ellipse, (m, brush));
                _musicianToShape.Add(m, ellipse);

                MusicianRender.Children.Add(ellipse);
            }

            for (int i = 0; i < solution.Problem.Attendees.Count; i++)
            {
                AttendeeToEllipse[i].ToolTip = $"{solution.GetScoreForAttendee(i)}, {string.Join(", ", solution.Problem.Attendees[i].Tastes)}";
            }
        }

        private SolidColorBrush GetBrushFromMagicGradient(double val, double minVal, double maxVal)
        {
            int index;
            int half = MagicGradientSteps / 2;
            if (val == 0)
            {
                index = half;
            }
            else if (val < 0)
            {
                index = half - (int)(val / minVal * half);
            }
            else
            {
                index = half + (int)(val / maxVal * half) - 1;
            }
            return MagicGradient[Math.Min(index, 1023)];
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
                bool blocked = _currentSolution.IsMusicianBlocked(attendee.Key, sourceMusician);

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
            Attendee attendee = _attendeeShapeToAttendee[attendeeBubble].Attendee;
            Point p = attendee.Location;

            List<Musician> allMusicians = _musicianShapeToMusician.Values.Select(x => x.Musician).ToList();
            foreach (Musician sourceMusician in allMusicians)
            {
                bool blocked = _currentSolution.IsMusicianBlocked(attendee, sourceMusician);

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
                attendeeBubble.Fill = _attendeeShapeToAttendee[attendeeBubble].OriginalColor;
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
            WinPoint cursorPosition = e.GetPosition(_stageOverlayRect);
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

        private void ColorizerSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentAudienceColorizer = AudienceColorizers.GetColorizer(ColorizerSelector.SelectedItem.ToString());
            // Re-colorize anything we need to. This will hose highlighted stuff

            // TODO: Refactor
            Dictionary<Attendee, double> evaluatedAttendees = _currentAudienceColorizer.Invoke(_currentProblem);

            double audienceMin = double.MaxValue;
            double audienceMax = double.MinValue;

            foreach (var kvp in evaluatedAttendees)
            {
                audienceMin = Math.Min(audienceMin, kvp.Value);
                audienceMax = Math.Max(audienceMax, kvp.Value);
            }

            foreach (var kvp in _attendeeShapeToAttendee)
            {
                Shape s = kvp.Key;
                Attendee a = kvp.Value.Attendee;

                SolidColorBrush brush = GetBrushFromMagicGradient(evaluatedAttendees[a], audienceMin, audienceMax);
                s.Fill = brush;
                _attendeeShapeToAttendee[s] = (a, brush);
            }
        }
    }
}
