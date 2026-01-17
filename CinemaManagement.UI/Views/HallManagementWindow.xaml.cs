// CinemaManagement.UI/Views/HallManagementWindow.xaml.cs

using System.Windows;
using System.Windows.Controls;
using CinemaManagement.Core.Entities;
using CinemaManagement.Core.Managers;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.UI.Views
{
    public partial class HallManagementWindow : Window
    {
        private readonly HallManager _hallManager;
        private readonly ILogger<HallManagementWindow> _logger;

        public HallManagementWindow(HallManager hallManager, ILogger<HallManagementWindow> logger)
        {
            InitializeComponent();
            _hallManager = hallManager;
            _logger = logger;
            Loaded += async (s, e) => await LoadHalls();
        }

        private async Task LoadHalls()
        {
            try
            {
                var halls = await _hallManager.GetAllHallsAsync();
                HallsDataGrid.ItemsSource = halls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading halls");
                MessageBox.Show("Failed to load halls", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddHall_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new HallDialog();
            if (dialog.ShowDialog() == true)
            {
                _ = SaveHall(dialog.HallName, dialog.HallType, dialog.Rows, dialog.SeatsPerRow);
            }
        }

        private async Task SaveHall(string name, HallType type, int rows, int seatsPerRow)
        {
            try
            {
                await _hallManager.CreateHallAsync(name, type, rows, seatsPerRow);
                MessageBox.Show("Hall added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadHalls();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding hall");
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditHall_Click(object sender, RoutedEventArgs e)
        {
            var selected = HallsDataGrid.SelectedItem as Hall;
            if (selected == null)
            {
                MessageBox.Show("Please select a hall to edit", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new HallDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                _ = UpdateHall(selected.Id, dialog.HallName, dialog.HallType, dialog.Rows, dialog.SeatsPerRow);
            }
        }

        private async Task UpdateHall(int id, string name, HallType type, int rows, int seatsPerRow)
        {
            try
            {
                await _hallManager.UpdateHallAsync(id, name, type, rows, seatsPerRow);
                MessageBox.Show("Hall updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadHalls();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hall");
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadHalls();
        }
    }

    public class HallDialog : Window
    {
        public string HallName { get; private set; }
        public HallType HallType { get; private set; }
        public int Rows { get; private set; }
        public int SeatsPerRow { get; private set; }

        public HallDialog(Hall existing = null)
        {
            Width = 400;
            Height = 450;
            Title = existing == null ? "Add Hall" : "Edit Hall";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var stack = new StackPanel { Margin = new Thickness(20) };

            var nameBox = CreateTextBox("Hall Name:", existing?.Name ?? "");

            var typeCombo = new ComboBox { Margin = new Thickness(0, 5, 0, 15) };
            typeCombo.Items.Add(HallType.Standard);
            typeCombo.Items.Add(HallType.IMAX);
            typeCombo.Items.Add(HallType.ThreeD);
            typeCombo.SelectedIndex = existing == null ? 0 : (int)existing.Type;
            var typeLabel = new TextBlock { Text = "Hall Type:", Margin = new Thickness(0, 10, 0, 5) };

            var rowsBox = CreateTextBox("Number of Rows:", existing?.TotalRows.ToString() ?? "10");
            var seatsBox = CreateTextBox("Seats Per Row:", existing?.SeatsPerRow.ToString() ?? "12");

            stack.Children.Add(nameBox.Item1);
            stack.Children.Add(nameBox.Item2);
            stack.Children.Add(typeLabel);
            stack.Children.Add(typeCombo);
            stack.Children.Add(rowsBox.Item1);
            stack.Children.Add(rowsBox.Item2);
            stack.Children.Add(seatsBox.Item1);
            stack.Children.Add(seatsBox.Item2);

            var saveBtn = new Button
            {
                Content = "Save",
                Width = 100,
                Padding = new Thickness(15, 10, 15, 10),
                Margin = new Thickness(0, 20, 0, 0)
            };
            saveBtn.Click += (s, e) => {
                HallName = nameBox.Item2.Text;
                HallType = (HallType)typeCombo.SelectedItem;
                if (!int.TryParse(rowsBox.Item2.Text, out int r) || r <= 0)
                {
                    MessageBox.Show("Invalid rows");
                    return;
                }
                if (!int.TryParse(seatsBox.Item2.Text, out int spr) || spr <= 0)
                {
                    MessageBox.Show("Invalid seats per row");
                    return;
                }
                Rows = r;
                SeatsPerRow = spr;
                DialogResult = true;
                Close();
            };

            stack.Children.Add(saveBtn);
            Content = stack;
        }

        private (TextBlock, TextBox) CreateTextBox(string label, string value)
        {
            var lbl = new TextBlock { Text = label, Margin = new Thickness(0, 10, 0, 5) };
            var txt = new TextBox { Text = value, Padding = new Thickness(8, 5, 8, 5) };
            return (lbl, txt);
        }
    }
}