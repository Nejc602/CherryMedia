using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Data;
using System.Windows.Controls;

namespace Diplomska
{
    public partial class MainWindow : Window
    {
        private string dbPath = "C:\\Projects\\CherryMedia\\Diplomska\\Diplomska\\Diplomska\\SQLite_Connection.db";
        private string connectionString;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = $"Data Source={dbPath};Version=3;";
            EnsureUserTableExists();
        }

        private void EnsureUserTableExists()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT NOT NULL UNIQUE,
                        password TEXT NOT NULL
                    );";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM Users WHERE username=@username AND password=@password";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 1)
                    {
                        MessageBox.Show("Uspešno prijavljen!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        MainContent.Content = new loggedIn();
                    }
                    else
                    {
                        MessageBox.Show("Napačno uporabniško ime ali geslo.", "Napaka", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            string username = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Prosim vnesi uporabniško ime in geslo.", "Napaka", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "INSERT OR IGNORE INTO Users (username, password) VALUES (@username, @password);";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Uspešno registriran!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Uporabniško ime že obstaja.", "Napaka", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }
    }
}
