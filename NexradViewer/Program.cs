using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;

namespace NexradViewer
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                // Ensure directories exist
                EnsureDirectoriesExist();
                
                // Register global exception handler
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                // Build and run the app
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                LogFatalError(ex);
                Console.Error.WriteLine($"Fatal error occurred: {ex.Message}\n\nSee logs for details.");
            }
        }

        /// <summary>
        /// Avalonia configuration, don't remove; also used by visual designer.
        /// </summary>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
                
        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogFatalError(e.Exception);
            e.SetObserved();
            ShowErrorDialog($"Unhandled task exception: {e.Exception.Message}");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                LogFatalError(ex);
                ShowErrorDialog($"Unhandled application exception: {ex.Message}");
            }
        }
        
        private static void ShowErrorDialog(string message)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var messageBox = new MessageBoxWindow()
                {
                    Title = "NexradViewer Error",
                    Message = $"{message}\n\nSee logs for details.",
                    OkButtonText = "OK"
                };
                
                messageBox.Show();
            }
            else
            {
                Console.Error.WriteLine($"Error: {message}");
            }
        }

        private static void EnsureDirectoriesExist()
        {
            // Create required directories
            Directory.CreateDirectory("MapData");
            Directory.CreateDirectory("Logs");
            Directory.CreateDirectory("Data");
            Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "NexradViewer", "Cache"));
        }

        private static void LogFatalError(Exception ex)
        {
            try
            {
                string logPath = Path.Combine("Logs", $"error_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                
                using (var writer = new StreamWriter(logPath, true))
                {
                    writer.WriteLine($"Fatal error occurred at {DateTime.Now}");
                    writer.WriteLine($"Exception: {ex.GetType().Name}");
                    writer.WriteLine($"Message: {ex.Message}");
                    writer.WriteLine($"Stack Trace: {ex.StackTrace}");
                    
                    if (ex.InnerException != null)
                    {
                        writer.WriteLine("Inner Exception:");
                        writer.WriteLine($"Type: {ex.InnerException.GetType().Name}");
                        writer.WriteLine($"Message: {ex.InnerException.Message}");
                        writer.WriteLine($"Stack Trace: {ex.InnerException.StackTrace}");
                    }
                    
                    writer.WriteLine(new string('-', 50));
                }
            }
            catch
            {
                // Fail silently if we can't log the error
            }
        }
        
        // Simple message box implementation for Avalonia
        public class MessageBoxWindow : Window
        {
            public string Message { get; set; }
            public string OkButtonText { get; set; } = "OK";
            
            public MessageBoxWindow()
            {
                Width = 400;
                Height = 200;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                CanResize = false;
            }
            
            protected override void OnOpened(EventArgs e)
            {
                base.OnOpened(e);
                
                var grid = new Grid();
                Content = grid;
                
                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(20),
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Spacing = 20
                };
                
                var textBlock = new TextBlock
                {
                    Text = Message,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };
                
                var button = new Button
                {
                    Content = OkButtonText,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    MinWidth = 80
                };
                
                button.Click += (s, e) => Close();
                
                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(button);
                grid.Children.Add(stackPanel);
            }
            
            public new void Show()
            {
                ShowDialog(null);
            }
        }
    }
}
