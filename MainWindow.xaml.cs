using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SQLtest;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ObservableCollection<Task> tasks = new();
    public List<string> tableNames = new List<string> { "Tasks" };
    public MainWindow()
    {
        InitializeComponent();

        using SqliteConnection connection = new SqliteConnection("Data Source=data.db");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $@"
            CREATE TABLE IF NOT EXISTS {tableNames[0]} (
                Id     INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
        DataB.ItemsSource = tasks;
        LoadTasks();
    }

    public void LoadTasks()
    {
        tasks.Clear();

        using SqliteConnection connection = new SqliteConnection("Data Source=data.db");
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {tableNames[0]};";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Task task = new Task
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1)
            };

            tasks.Add(task);
        }
    }
    public void InsertTask(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Input.Text))
        {
            MessageBox.Show("Empty");
            return;
        }

        using SqliteConnection connection = new SqliteConnection("Data Source=data.db");
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {tableNames[0]} (Title) VALUES ($title);";
        command.Parameters.AddWithValue("$title", Input.Text.Trim());
        command.ExecuteNonQuery();

        Input.Text = "";
        LoadTasks();
    }

    public void DeleteTask(object sender, RoutedEventArgs e)
    {
        var selected = DataB.SelectedItem as Task;
        if (selected == null)
        {
            MessageBox.Show("Select a task to delete");
            return;
        }

        using SqliteConnection connection = new SqliteConnection("Data Source=data.db");
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {tableNames[0]} WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", selected.Id);
        command.ExecuteNonQuery();

        LoadTasks();
    }
}