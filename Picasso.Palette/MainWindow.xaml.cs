using Picasso.Palette.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Picasso.Palette;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ObservableStack<string> _commands = [];
    private bool _isClosing = false;
    public MainWindow()
    {
        InitializeComponent();
        CommandList.ItemsSource = _commands;

        LoadCommands();

        this.Deactivated += (s, e) =>
        {
            if (!_isClosing)
            {
                SaveCommands();
                this.Close();
            }
        };
    }

    private void AddCommand_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(CommandInput.Text))
        {
            _commands.Push(CommandInput.Text);
            CommandInput.Clear();
        }
    }

    private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (CommandList != null && SearchInput.Text != "> keyword search...")
        {
            var filteredCommands = _commands.Where(c => c.Contains(SearchInput.Text)).ToList();
            CommandList.ItemsSource = filteredCommands;
        }
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
            SearchInput.Text = "> keyword search...";
        }
    }

    private void CommandInput_GotFocus(object sender, RoutedEventArgs e)
    {
        CommandInput.Text = string.Empty;
        SearchInput.Text = "> keyword search...";

        if (CommandList != null)
        {
            CommandList.ItemsSource = _commands;
        }
    }

    private void CommandInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CommandInput.Text))
        {
            CommandInput.Text = "> store new command...";
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
        string filePath = System.IO.Path.Combine(picassoPath, "commands.txt");

        if (!System.IO.Directory.Exists(picassoPath))
        {
            System.IO.Directory.CreateDirectory(picassoPath);
        }

        System.IO.File.WriteAllLines(filePath, _commands);
    }

    private void LoadCommands()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string picassoPath = System.IO.Path.Combine(appDataPath, ".picasso");
        string filePath = System.IO.Path.Combine(picassoPath, "commands.txt");

        if (System.IO.File.Exists(filePath))
        {
            var commands = System.IO.File.ReadAllLines(filePath);
            foreach (var command in commands)
            {
                _commands.Push(command);
            }
        }
    }

    private void CommandList_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            if (CommandList.SelectedItem != null)
            {
                var listBoxItem = (ListBoxItem)CommandList.ItemContainerGenerator.ContainerFromItem(CommandList.SelectedItem);
                if (listBoxItem != null)
                {
                    var item = listBoxItem.Content.ToString();
                    _commands.Remove(item);
                }
            }
        }
    }

    private async void CommandList_Selected(object sender, RoutedEventArgs e)
    {
        if (CommandList.SelectedItem != null)
        {
            var listBoxItem = (ListBoxItem)CommandList.ItemContainerGenerator.ContainerFromItem(CommandList.SelectedItem);
            if (listBoxItem != null)
            {
                var item = listBoxItem.Content.ToString();
                listBoxItem.IsSelected = false;
                var storyboard = (Storyboard)FindResource("FlashAnimation");
                Storyboard.SetTarget(storyboard, listBoxItem);
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

                storyboard.Completed += (s, e) =>
                {
                    listBoxItem.Background = Brushes.Transparent;
                };
            }
        }
    }

    private void InfoButton_Click(object sender, RoutedEventArgs e)
    {
        InfoPopup.IsOpen = true;
    }
}