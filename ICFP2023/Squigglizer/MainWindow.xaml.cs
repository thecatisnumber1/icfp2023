using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ICFP2023
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SquigglizerSettings settings;
        private ProblemCatalog AllProblems;

        public MainWindow()
        {
            InitializeComponent();

            settings = new SquigglizerSettings();
            settings.ParseArgs(Environment.GetCommandLineArgs()[1..]);
            SettingsControl.Settings = settings;

            AllProblems = new ProblemCatalog();
            ProblemSelector.ItemsSource = AllProblems.Names;


            //RunTask(() =>
            //{
            //    Console.Write("What is your favorite color? ");
            //    string name = Console.ReadLine();
            //    Console.Error.WriteLine($"Error: {name} is not a creative color!");
            //});
        }

        private void RenderProblem(ProblemSpec problem)
        {
            BaseRender.Children.Clear();

            // Test hackery
            BaseRender.Width = problem.RoomWidth; // Should be problem width/height eventually
            BaseRender.Height = problem.RoomHeight;
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
            // Audience member
            foreach (Attendee a in problem.Attendees)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 5;
                ellipse.Height = 5;
                ellipse.Stroke = new SolidColorBrush(Colors.Red);
                ellipse.Fill = new SolidColorBrush(Colors.Salmon);
                Canvas.SetTop(ellipse, problem.RoomHeight - a.Location.Y); // These are wrong because of width/height
                Canvas.SetLeft(ellipse, a.Location.X);
                BaseRender.Children.Add(ellipse);
            }
        }

        private void ResetProblem()
        {
            ProblemSpec selectedProblem = AllProblems.GetSpec(ProblemSelector.SelectedItem.ToString());
            RenderProblem(selectedProblem);
        }

        private void ProblemSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetProblem();
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
    }
}
