using System;
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

        private double PersonSizePx;

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
        }

        private void RenderProblem(ProblemSpec problem)
        {
            BaseRender.Children.Clear();
            MusicianRender.Children.Clear();
            ZoomArea.Reset();

            // Test hackery
            BaseRender.Width = problem.RoomWidth; // Should be problem width/height eventually
            BaseRender.Height = problem.RoomHeight;

            MusicianRender.Width = problem.RoomWidth; // You'll need to check whether manual placement is on the stage on your own
            MusicianRender.Height = problem.RoomHeight;

            // Dynamic dot sizes!? Should be about 0.5% of the long axis, but no more than 10 because musicians' no-touching zones
            double longAxis = Math.Max(problem.RoomWidth, problem.RoomHeight);
            PersonSizePx = Math.Min(longAxis / 200.0, 10.0);

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
            border.Stroke = new SolidColorBrush(Colors.Black);
            border.StrokeThickness = 0.5;
            BaseRender.Children.Add(border);

            // Stage
            Rectangle stage = new Rectangle();
            stage.Width = problem.StageWidth;
            stage.Height = problem.StageHeight;
            stage.Stroke = new SolidColorBrush(Colors.Black);
            stage.Fill = new SolidColorBrush(Colors.LightGray);
            Canvas.SetBottom(stage, problem.StageBottomLeft.Y);
            Canvas.SetLeft(stage, problem.StageBottomLeft.X);
            BaseRender.Children.Add(stage);

            // Attendees
            foreach (Attendee a in problem.Attendees)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = PersonSizePx;
                ellipse.Height = PersonSizePx;
                ellipse.Stroke = new SolidColorBrush(Colors.Red);
                ellipse.Fill = new SolidColorBrush(Colors.Salmon);
                Canvas.SetTop(ellipse, problem.RoomHeight - a.Location.Y - PersonSizePx / 2);
                Canvas.SetLeft(ellipse, a.Location.X - PersonSizePx / 2);
                BaseRender.Children.Add(ellipse);
            }
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

            var adapter = new UIAdapterImpl(this);

            // This should probably happen *after* the task runs
            ResetProblem(); // Wipe all state.

            RunTask(() =>
            {
                solver.Invoke(_currentProblem, settings, adapter);

                // This will totally screw me over later, but it lets a final Render call go through.
                Task.Delay(50).Wait();

                // Here is where I should shut down the old UIAdapter. But I'm lazy atm.

                // Cleanup UI on the main thread.
                Dispatcher.BeginInvoke(() =>
                {
                    Console.WriteLine("Done!");
                    SolverSelector.IsEnabled = true;
                    ProblemSelector.IsEnabled = true;
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
            MusicianRender.Children.Clear();

            foreach (Point p in solution.Placements)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = PersonSizePx;
                ellipse.Height = PersonSizePx;
                ellipse.Stroke = new SolidColorBrush(Colors.Blue);
                ellipse.Fill = new SolidColorBrush(Colors.LightBlue);
                Canvas.SetTop(ellipse, _currentProblem.RoomHeight - p.Y - PersonSizePx / 2);
                Canvas.SetLeft(ellipse, p.X - PersonSizePx / 2);
                MusicianRender.Children.Add(ellipse);
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
    }
}
