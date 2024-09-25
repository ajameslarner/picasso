using Microsoft.Win32;
using Picasso.Models;
using Picasso.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Picasso;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ObservableStack<CommandEntry> _entries = [];
    private List<CommandEntry> _filteredEntries = [];

    private bool _isClosing = false;
    private bool _suspendClosure = false;
    private bool _isSortDescending = true;
    private bool _showFavouritesOnly = false;

    public MainWindow()
    {
        InitializeComponent();
        CommandList.ItemsSource = _entries;

        LoadCommands();

        this.Deactivated += (s, e) =>
        {
            if (!_isClosing && !_suspendClosure)
            {
                SaveCommands();
                var popDownStoryboard = (Storyboard)FindResource("PopDownAnimation");

                popDownStoryboard.Completed += (s, e) =>
                {
                    this.Close();
                };

                popDownStoryboard.Begin(this);
            }
        };

        this.Loaded += (s, e) =>
        {
            var popUpStoryboard = (Storyboard)FindResource("PopUpAnimation");
            popUpStoryboard.Begin(this);

            var taskbar = SystemParameters.WorkArea.BottomRight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Top = taskbar.Y - Height;
            Left = taskbar.X - Width;
        };
    }

    private void AddCommand_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(CommandInput.Text))
        {
            var commandEntry = new CommandEntry
            {
                Command = CommandInput.Text,
                DateTime = DateTime.Now
            };
            _entries.Push(commandEntry);
            CommandInput.Clear();
        }
    }

    private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (CommandList == null)
            return;

        if (SearchInput.Text != "Search..." && !string.IsNullOrWhiteSpace(SearchInput.Text))
        {
            _filteredEntries = _entries.Where(c => c.Command.Contains(SearchInput.Text, StringComparison.OrdinalIgnoreCase)).ToList();
            CommandList.ItemsSource = _filteredEntries;
            return;
        }
        
        CommandList.ItemsSource = _entries;
    }

    private void CommandInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddCommand_Click(this, new RoutedEventArgs());
        }
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }

    private void SearchInput_GotFocus(object sender, RoutedEventArgs e)
    {
        SearchInput.Text = string.Empty;
    }

    private void SearchInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SearchInput.Text))
        {
            SearchInput.Text = "Search...";
        }
    }

    private void CommandInput_GotFocus(object sender, RoutedEventArgs e)
    {
        CommandInput.Text = string.Empty;
        SearchInput.Text = "Search...";

        if (CommandList != null)
        {
            CommandList.ItemsSource = _entries;
        }
    }

    private void CommandInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CommandInput.Text))
        {
            CommandInput.Text = "Add...";
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _isClosing = true;
        SaveCommands();
        this.Close();
    }

    private void SaveCommands()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string picassoPath = System.IO.Path.Combine(appDataPath, ".picasso");
        string filePath = System.IO.Path.Combine(picassoPath, "commands.csv");

        if (!System.IO.Directory.Exists(picassoPath))
        {
            System.IO.Directory.CreateDirectory(picassoPath);
        }

        using (var writer = new System.IO.StreamWriter(filePath))
        {
            writer.WriteLine("Command,DateTime,IsFavorite");
            foreach (var commandEntry in _entries)
            {
                writer.WriteLine($"{commandEntry.Command},{commandEntry.DateTime:yyyy-MM-dd HH:mm:ss},{commandEntry.IsFavourite}");
            }
        }
    }

    private void LoadCommands()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string picassoPath = System.IO.Path.Combine(appDataPath, ".picasso");
        string filePath = System.IO.Path.Combine(picassoPath, "commands.csv");

        if (System.IO.File.Exists(filePath))
        {
            using (var reader = new System.IO.StreamReader(filePath))
            {
                string line;
                reader.ReadLine(); // Skip header
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        var commandEntry = new CommandEntry
                        {
                            Command = parts[0],
                            DateTime = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss", null),
                            IsFavourite = bool.Parse(parts[2])
                        };
                        _entries.Push(commandEntry);
                    }
                }
            }
        }
    }

    private void CommandList_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            if (CommandList.SelectedItem == null)
                return;

            var item = (CommandEntry)CommandList.SelectedItem;
            _entries.Remove(item);

            if (!string.IsNullOrWhiteSpace(SearchInput.Text) && SearchInput.Text != "Search...")
            {
                _filteredEntries.Remove(item);
                CommandList.ItemsSource = _filteredEntries;
            }
            else
            {
                CommandList.ItemsSource = _entries;
            }

            CommandList.Items.Refresh();
        }
        else if (e.Key == Key.F)
        {
            if (CommandList.SelectedItem is CommandEntry selectedCommand)
            {
                selectedCommand.IsFavourite = !selectedCommand.IsFavourite;
            }
        }

        CommandList.Items.Refresh();
    }

    private void InfoButton_Click(object sender, RoutedEventArgs e)
    {
        InfoPopup.IsOpen = true;
    }

    private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        _suspendClosure = true;
        var result = MessageBox.Show("Are you sure you want to clear the history? This action cannot be undone.",
                                     "Confirm Clear History",
                                     MessageBoxButton.YesNo,
                                     MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _entries.Clear();
            CommandList.ItemsSource = _entries;
            CommandList.Items.Refresh();
        }
        _suspendClosure = false;
    }

    private void ExportCommandsButton_Click(object sender, RoutedEventArgs e)
    {
        _suspendClosure = true;

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Export Commands",
            FileName = "commands_export.csv"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            using (var writer = new System.IO.StreamWriter(saveFileDialog.FileName))
            {
                writer.WriteLine("Command,DateTime,IsFavorite");
                foreach (var commandEntry in _entries)
                {
                    writer.WriteLine($"{commandEntry.Command},{commandEntry.DateTime:yyyy-MM-dd HH:mm:ss},{commandEntry.IsFavourite}");
                }
            }

            MessageBox.Show($"Commands exported to {saveFileDialog.FileName}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        _suspendClosure = false;
    }

    private void ImportCommandsButton_Click(object sender, RoutedEventArgs e)
    {
        _suspendClosure = true;
        var openFileDialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Import Commands"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var importedCommands = new List<CommandEntry>();
            using (var reader = new System.IO.StreamReader(openFileDialog.FileName))
            {
                string line;
                reader.ReadLine(); // Skip header
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        var commandEntry = new CommandEntry
                        {
                            Command = parts[0],
                            DateTime = DateTime.ParseExact(parts[1], "yyyy-MM-dd HH:mm:ss", null),
                            IsFavourite = bool.Parse(parts[2])
                        };
                        importedCommands.Add(commandEntry);
                    }
                }
            }

            foreach (var commandEntry in importedCommands)
            {
                _entries.Push(commandEntry);
            }

            CommandList.ItemsSource = _entries;
            CommandList.Items.Refresh();

            MessageBox.Show($"Commands imported from {openFileDialog.FileName}", "Import Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        _suspendClosure = false;
    }

    private void SortCommandsButton_Click(object sender, RoutedEventArgs e)
    {
        _isSortDescending = !_isSortDescending; // Toggle sort order

        SortButton.Content = _isSortDescending ? "▼" : "▲";
        SortButton.ToolTip = _isSortDescending ? "Sort Ascending" : "Sort Descending";

        if (!string.IsNullOrWhiteSpace(SearchInput.Text) && SearchInput.Text != "Search...")
        {
            _filteredEntries = _isSortDescending
                ? _filteredEntries.OrderByDescending(c => c.DateTime).ToList()
                : _filteredEntries.OrderBy(c => c.DateTime).ToList();
            CommandList.ItemsSource = _filteredEntries;
        }
        else
        {
            var sortedCommands = _isSortDescending
                ? _entries.OrderByDescending(c => c.DateTime).ToList()
                : _entries.OrderBy(c => c.DateTime).ToList();
            _entries.Clear();
            foreach (var command in sortedCommands)
            {
                _entries.Push(command);
            }
            CommandList.ItemsSource = _entries;
        }
        CommandList.Items.Refresh();
    }

    private void FavoriteCommandsButton_Click(object sender, RoutedEventArgs e)
    {
        _showFavouritesOnly = !_showFavouritesOnly; // Toggle favorite filter

        if (_showFavouritesOnly)
        {
            var favoriteCommands = _entries.Where(c => c.IsFavourite).ToList();
            CommandList.ItemsSource = favoriteCommands;
        }
        else
        {
            CommandList.ItemsSource = _entries;
        }

        CommandList.Items.Refresh();
    }

    private async void CommandList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (CommandList.SelectedItem != null)
        {
            var listBoxItem = (ListBoxItem)CommandList.ItemContainerGenerator.ContainerFromItem(CommandList.SelectedItem);
            if (listBoxItem != null)
            {
                var item = listBoxItem.Content.ToString();
                var storyboard = (Storyboard)FindResource("FlashAnimation");
                Storyboard.SetTarget(storyboard, CommandList);
                storyboard.Begin();

                try
                {
                    Clipboard.SetText(item);
                }
                catch (Exception)
                {
                    await Task.Delay(100);
                    Clipboard.SetText(item);
                }
            }
        }
    }
}