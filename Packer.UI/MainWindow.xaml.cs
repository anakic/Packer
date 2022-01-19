using Jot;
using Jot.Configuration.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Packer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Tracker tracker = new Tracker();

        public MainWindow()
        {
            InitializeComponent();
            tracker.Configure<Window>()
                .Id(w => w.Name, SystemInformation.VirtualScreen.Size) // <-- include the screen resolution in the id
                .Properties(w => new { w.Top, w.Width, w.Height, w.Left, w.WindowState })
                .PersistOn(nameof(Window.Closing))
                .StopTrackingOn(nameof(Window.Closing));

            SourceInitialized += MainWindow_SourceInitialized;

            var vm = new MainWindowVM();
            DataContext = vm;
            tracker.Track(vm);
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            tracker.Track(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            tracker.PersistAll();
            base.OnClosed(e);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            msgs.ScrollToEnd();
        }
    }

    class MainWindowVM : INotifyPropertyChanged
    {
        private string? pbitFilePath;
        private string? repositoryFolderPath;

        Engine engine;

        public MainWindowVM()
        {
            var loggerFactory = new LoggerFactory().AddDebugLogger(AddMessage);
            engine = new Engine(loggerFactory);
        }

        [Trackable]
        public string PbitFilePath
        {
            get => pbitFilePath;
            set
            {
                pbitFilePath = value;
                OnPropertyChanged(nameof(PbitFilePath));
            }
        }

        [Trackable]
        public string RepositoryFolderPath
        {
            get => repositoryFolderPath;
            set
            {
                repositoryFolderPath = value;
                OnPropertyChanged(nameof(RepositoryFolderPath));
            }
        }

        ICommand? browsePbitFile;
        public ICommand BrowsePbitFile
        {
            get
            {
                return browsePbitFile ?? (browsePbitFile = new RelayCommand(() =>
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Filter = "pbit files|*.pbit";
                    ofd.CheckFileExists = false;
                    ofd.Multiselect = false;
                    ofd.FileName = PbitFilePath;
                    if (ofd.ShowDialog() == DialogResult.OK)
                        PbitFilePath = ofd.FileName;
                }, _ => !IsWorking));
            }
        }

        ICommand? browseRepositoryFolder;
        public ICommand BrowseRepositoryFolder
        {
            get
            {
                return browseRepositoryFolder ?? (browseRepositoryFolder = new RelayCommand(() =>
                {
                    var dialog = new FolderBrowserDialog();
                    dialog.SelectedPath = RepositoryFolderPath;
                    if(dialog.ShowDialog() == DialogResult.OK)
                        RepositoryFolderPath = dialog.SelectedPath;
                }, _ => !IsWorking));
            }
        }

        ICommand? extractCommand;
        public ICommand ExtractCommand
        {
            get { return extractCommand ?? (extractCommand = new RelayCommand(async () => 
            {
                IsWorking = true;
                Messages = "";
                try
                {
                    await Task.Run(() => engine.Extract(PbitFilePath, RepositoryFolderPath));
                }
                finally
                {
                    CommandManager.InvalidateRequerySuggested();
                    IsWorking = false;
                }
            }, _ => !IsWorking)); }
        }

        ICommand? packCommand;
        public ICommand PackCommand
        {
            get { return packCommand ?? (packCommand = new RelayCommand(() => 
            {
                RunAsync(() => engine.Pack(RepositoryFolderPath, PbitFilePath));
            }, _ => !IsWorking)); }
        }

        private string messages;

        public string Messages
        {
            get { return messages; }
            set { messages = value; OnPropertyChanged(nameof(Messages)); }
        }


        public bool IsWorking { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void RunAsync(Action a)
        {
            IsWorking = true;
            Messages = "";
            try
            {
                await Task.Run(a);
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                IsWorking = false;
            }
        }

        private void AddMessage(string message)
        {
            Messages += message + Environment.NewLine;
        }
    }

    public class RelayCommand : ICommand
    {
        private Action commandTask;
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action t_workToDo, Func<object, bool> canExecute)
        {
            commandTask = t_workToDo;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return canExecute(parameter);
        }

        public void Execute(Object parameter)
        {
            commandTask();
        }
    }




    public class DebugLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers;
        private readonly Action<string> logAction;

        public DebugLoggerProvider(Action<string> logAction)
        {
            _loggers = new ConcurrentDictionary<string, ILogger>();
            this.logAction = logAction;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, new DebugLogger(logAction));
        }
    }

    public class DebugLogger : ILogger
    {
        private readonly Action<string> logAction;

        public DebugLogger(Action<string> logAction)
        {
            this.logAction = logAction;
        }

        public void Log<TState>(
           LogLevel logLevel, EventId eventId,
           TState state, Exception exception,
           Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                logAction(formatter(state, exception));
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }

    public static class DebugLoggerFactoryExtensions
    {
        public static ILoggerFactory AddDebugLogger(
           this ILoggerFactory factory, Action<string> logAction)
        {
            factory.AddProvider(new DebugLoggerProvider(logAction));
            return factory;
        }
    }
}
