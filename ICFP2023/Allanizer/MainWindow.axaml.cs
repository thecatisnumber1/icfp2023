using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ICFP2023;

public partial class MainWindow : Window
{
    private Solution _currentSolution;


    private double PersonSizePx = 10.0;

    #region Control Tracking for Highlighting
    private Dictionary<Shape, (Musician Musician, SolidColorBrush FillBrush)> _musicianShapeToMusician = new Dictionary<Shape, (Musician Musician, SolidColorBrush FillBrush)>();
    private Dictionary<Musician, Shape> _musicianToShape = new Dictionary<Musician, Shape>();

    private Dictionary<Shape, (Attendee Attendee, SolidColorBrush OriginalColor)> _attendeeShapeToAttendee = new Dictionary<Shape, (Attendee Attendee, SolidColorBrush OriginalColor)>();
    private Dictionary<Attendee, Shape> _attendeeToShape = new Dictionary<Attendee, Shape>();
    #endregion

    private bool AutoZoomToStage = true;

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

    #region Hacks for Manual Placement
    private Rectangle _stageOverlayRect;
    #endregion

    private ProblemSpec _currentProblem;

    private AudienceColorizers.Colorizer _currentAudienceColorizer;

    public MainWindow()
    {
        InitializeComponent();

        BaseRender = this.FindControl<Canvas>("BaseRender");
        ZoomGrid = this.FindControl<Grid>("ZoomGrid");

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

        _currentProblem = ProblemSpec.Read($"problem-8");
        RenderProblem(_currentProblem);

        // Solution solution = new Solution();
        // Solution best = BadAnnealingSolver.Solve(solution.Problem, new ConsoleSettings(), new DoNothingUIAdapter());
        // Console.WriteLine($"Best score: {best.InitializeScore()}");
        // Console.WriteLine($"Placement: {string.Join(", ", best.Placements)}");
        // SubmitSolution(best, i).Wait();
    }

    private void RenderProblem(ProblemSpec problem)
    {
        BaseRender.Children.Clear();
        // _musicianShapeToMusician.Clear();
        // _musicianToShape.Clear();
        // _attendeeShapeToAttendee.Clear();
        // _attendeeToShape.Clear();
        // ZoomArea.Reset();

        // Test hackery
        BaseRender.Width = problem.RoomWidth; // Should be problem width/height eventually
        BaseRender.Height = problem.RoomHeight;

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
        // _stageOverlayRect = new Rectangle();
        // _stageOverlayRect.Width = _currentProblem.StageWidth - 10;
        // _stageOverlayRect.Height = _currentProblem.StageHeight - 10;
        // _stageOverlayRect.Fill = TransparentBrush;
        // Canvas.SetTop(_stageOverlayRect, _currentProblem.RoomHeight - _currentProblem.StageTop + 5); // Add the no-touching zone.
        // Canvas.SetLeft(_stageOverlayRect, _currentProblem.StageLeft + 5);
        // MusicianRender.Children.Add(_stageOverlayRect);

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
        foreach (Attendee a in problem.Attendees)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = PersonSizePx;
            ellipse.Height = PersonSizePx;
            ellipse.Stroke = UnselectedAttendeeBorderBrush;
            Canvas.SetTop(ellipse, problem.RoomHeight - a.Location.Y - PersonSizePx / 2);
            Canvas.SetLeft(ellipse, a.Location.X - PersonSizePx / 2);
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
        // var scale = (ScaleTransform)((TransformGroup)ZoomGrid.RenderTransform).Children.First(tr => tr is ScaleTransform);
        // var translate = (TranslateTransform)((TransformGroup)ZoomGrid.RenderTransform).Children.First(tr => tr is TranslateTransform);

        // translate.X = -minAttendeeX * scaleRatio;
        // translate.Y = (-(problem.RoomHeight - maxAttendeeY) + 1) * scaleRatio;

        // scale.ScaleX = scaleRatio;
        // scale.ScaleY = scaleRatio;
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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
        return MagicGradient[index];
    }

}

