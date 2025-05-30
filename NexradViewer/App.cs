using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NexradViewer.Services;
using NexradViewer.Utils;

namespace NexradViewer
{
    /// <summary>
    /// Main application class
    /// </summary>
    public class App : Application
    {
        /// <summary>
        /// Application start timestamp
        /// </summary>
        public static readonly DateTime StartTime = DateTime.Now;

        /// <summary>
        /// Initialize the XAML
        /// </summary>
        public override void Initialize()
        {
            // Initialize logger first thing
            Logger.Initialize();
            Logger.Log("Application starting");
            
            // Add global exception handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            
            // Log application paths
            LogApplicationPaths();
            
            // Load XAML
            try
            {
                AvaloniaXamlLoader.Load(this);
                Logger.Log("XAML loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "XAML Loading");
            }
        }

        /// <summary>
        /// Called when application has started
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            try
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    // Set up application exit handler
                    desktop.Exit += Desktop_Exit;
                    
                    // Create main window
                    Logger.Log("Creating main window");
                    desktop.MainWindow = new MainWindow();
                    
                    // Log successful startup
                    Logger.Log($"Application startup completed in {(DateTime.Now - StartTime).TotalMilliseconds}ms");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Framework Initialization");
                ShowErrorDialog("Startup Error", 
                    "An error occurred during application startup. Please check the logs for details.", ex);
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        private void Desktop_Exit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Logger.Log($"Application exiting with code {e.ApplicationExitCode}");
        }
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Logger.LogException(exception, "Unhandled Exception");
            Logger.Log($"IsTerminating: {e.IsTerminating}");
            
            if (e.IsTerminating)
            {
                ShowErrorDialog("Fatal Error", 
                    "A fatal error has occurred and the application needs to close. Error details have been logged.", 
                    exception);
            }
        }
        
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Logger.LogException(e.Exception, "Unobserved Task Exception");
            e.SetObserved(); // Mark as observed to prevent application termination
        }
        
        private void LogApplicationPaths()
        {
            try
            {
                Logger.Log("=== Application Paths ===");
                Logger.Log($"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
                Logger.Log($"Current Directory: {Environment.CurrentDirectory}");
                
                // Log important directories
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                string mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapData");
                
                Logger.Log($"Logs Directory: {logPath} (Exists: {Directory.Exists(logPath)})");
                Logger.Log($"Data Directory: {dataPath} (Exists: {Directory.Exists(dataPath)})");
                Logger.Log($"Map Directory: {mapPath} (Exists: {Directory.Exists(mapPath)})");
                Logger.Log("========================");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Logging Paths");
            }
        }
        
        private void ShowErrorDialog(string title, string message, Exception ex)
        {
            try
            {
                // We need to show a dialog on the UI thread if possible
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && 
                    desktop.MainWindow != null)
                {
                    var dialog = new Window
                    {
                        Title = title,
                        Width = 500,
                        Height = 400,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Content = CreateErrorContent(message, ex)
                    };
                    
                    dialog.Show();
                }
                else
                {
                    // If we can't show a dialog, just log the error
                    Logger.Log($"Error dialog could not be shown: {title} - {message}");
                }
            }
            catch
            {
                // Last resort: just write to console
                Console.WriteLine($"FATAL ERROR: {title} - {message}");
                if (ex != null)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            }
        }
        
        private Control CreateErrorContent(string message, Exception ex)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(20)
            };
            
            // Add error icon or image
            panel.Children.Add(new TextBlock
            {
                Text = "⚠️",
                FontSize = 48,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });
            
            // Add error message
            panel.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 20)
            });
            
            // Add exception details if available
            if (ex != null)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = "Technical Details:",
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Margin = new Thickness(0, 0, 0, 5)
                });
                
                panel.Children.Add(new TextBox
                {
                    Text = $"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    IsReadOnly = true,
                    Height = 150,
                    AcceptsReturn = true,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                });
            }
            
            // Add log file location
            panel.Children.Add(new TextBlock
            {
                Text = $"Log File: {Logger.GetLogFilePath()}",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Thickness(0, 20, 0, 0)
            });
            
            return panel;
        }
    }
}
