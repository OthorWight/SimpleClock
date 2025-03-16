using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SimpleClock
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private bool _isStopwatchMode = false;
        private Stopwatch _stopwatch;
        private HelpWindow _helpWindow;  // Store a reference to the HelpWindow

        public MainWindow()
        {
            InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Topmost = true;
            this.Loaded += Windows_Loaded;
            this.MouseDoubleClick += Window_MouseDoubleClick;
            this.MouseRightButtonDown += Window_MouseRightClick;
            this.KeyDown += Window_KeyDown;

            _stopwatch = new Stopwatch();

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += Clock_Tick;
            _timer.Start();
        }

        private void Windows_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = SystemParameters.WorkArea.Right - this.ActualWidth;
            this.Top = SystemParameters.WorkArea.Bottom - this.ActualHeight;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Left = SystemParameters.WorkArea.Right - this.ActualWidth;
            this.Top = SystemParameters.WorkArea.Bottom - this.ActualHeight;
        }

        private void Window_MouseRightClick(object sender, MouseButtonEventArgs e)
        {
            if (!_isStopwatchMode)
            {
                _isStopwatchMode = true;
                _stopwatch.Restart();
            }
            else
            {
                _isStopwatchMode = false;
                _stopwatch.Stop();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                ShowHelp();
            }
            else if (e.Key == Key.F2)
            {
                ToggleTopmost();
            }
        }

        private void Clock_Tick(object? sender, EventArgs e)
        {
            if (_isStopwatchMode)
            {
                clockLabel.Content = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.f");
                clockLabel.ToolTip = null;
            }
            else
            {
                clockLabel.Content = DateTime.Now.ToString("hh:mm:ss.f tt");
                clockLabel.ToolTip = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
            }
        }

        private void ToggleTopmost()
        {
            this.Topmost = !this.Topmost;
        }

        private void ShowHelp()
        {
            string helpText = "SimpleClock Features:\n\n" +
                              "• Right Click: Toggle Stopwatch Mode\n" +
                              "• Double Click: Reset Window Position\n" +
                              "• F1 key: Show Help\n" +
                              "• F2 key: Toggle Topmost";

            // If a help window is already open, close it.
            if (_helpWindow != null)
            {
                _helpWindow.Close();
                _helpWindow = null;
            }

            // Create and configure the new help window.
            _helpWindow = new HelpWindow(helpText);
            _helpWindow.Owner = this; // keeps it related to the main window
            _helpWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            // When the help window closes, reset the reference.
            _helpWindow.Closed += (s, e) => { _helpWindow = null; };

            // Show help window temporarily to measure its size.
            _helpWindow.Show();
            _helpWindow.UpdateLayout();

            // Retrieve help window dimensions.
            double helpWidth = _helpWindow.ActualWidth;
            double helpHeight = _helpWindow.ActualHeight;

            // Main window (clock) position & dimensions.
            double mainLeft = this.Left;
            double mainTop = this.Top;
            double mainRight = mainLeft + this.ActualWidth;
            double mainBottom = mainTop + this.ActualHeight;

            // Work area bounds.
            Rect workArea = SystemParameters.WorkArea;

            // Try different candidate placements:
            Point candidate = new Point();
            bool placed = false;

            // Candidate 1: Help's bottom-right anchored to Main's top-left.
            double candLeft = mainLeft - helpWidth;
            double candTop = mainTop - helpHeight;
            if (candLeft >= workArea.Left && candTop >= workArea.Top)
            {
                candidate = new Point(candLeft, candTop);
                placed = true;
            }

            // Candidate 2: Help's bottom-left anchored to Main's top-right.
            if (!placed)
            {
                candLeft = mainRight;
                candTop = mainTop - helpHeight;
                if (candLeft + helpWidth <= workArea.Right && candTop >= workArea.Top)
                {
                    candidate = new Point(candLeft, candTop);
                    placed = true;
                }
            }

            // Candidate 3: Help's top-right anchored to Main's bottom-left.
            if (!placed)
            {
                candLeft = mainLeft - helpWidth;
                candTop = mainBottom;
                if (candLeft >= workArea.Left && candTop + helpHeight <= workArea.Bottom)
                {
                    candidate = new Point(candLeft, candTop);
                    placed = true;
                }
            }

            // Candidate 4: Help's top-left anchored to Main's bottom-right.
            if (!placed)
            {
                candLeft = mainRight;
                candTop = mainBottom;
                if (candLeft + helpWidth <= workArea.Right && candTop + helpHeight <= workArea.Bottom)
                {
                    candidate = new Point(candLeft, candTop);
                    placed = true;
                }
            }

            // Fallback: Clamp the help window within the work area.
            if (!placed)
            {
                candidate = new Point(
                    Math.Max(workArea.Left, Math.Min(mainLeft, workArea.Right - helpWidth)),
                    Math.Max(workArea.Top, Math.Min(mainTop, workArea.Bottom - helpHeight))
                );
            }

            _helpWindow.Left = candidate.X;
            _helpWindow.Top = candidate.Y;
        }
    }

    // Custom HelpWindow to display help text.
    public class HelpWindow : Window
    {
        public HelpWindow(string helpText)
        {
            this.Title = "Help";
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.WindowStyle = WindowStyle.ToolWindow;
            this.ResizeMode = ResizeMode.NoResize;
            this.Content = new TextBlock
            {
                Text = helpText,
                Margin = new Thickness(10),
                TextWrapping = TextWrapping.Wrap
            };
        }
    }
}
