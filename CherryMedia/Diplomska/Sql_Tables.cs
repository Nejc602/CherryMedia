using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace Diplomska
{
    public static class Sql_Tables  
    {
        private static string databasePath = @"C:\Projects\CherryMedia\Diplomska\Diplomska\Diplomska\SqLite_Connection.db";
        private static string connectionString = $@"Data Source={databasePath};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(databasePath))
            {
                MessageBox.Show("Datoteka baze podatkov ne obstaja.", "Napaka", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Datoteka baze podatkov obstaja. Ustvarjanje tabel...", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string checkSongsTableQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='Songs';";
                using (SQLiteCommand checkTableCommand = new SQLiteCommand(checkSongsTableQuery, connection))
                {
                    var result = checkTableCommand.ExecuteScalar();
                    if (result == null)
                    {
                        string createSongsTableQuery = @"
                            CREATE TABLE Songs (
                                id INTEGER PRIMARY KEY AUTOINCREMENT,
                                title TEXT NOT NULL,
                                artist TEXT NOT NULL,
                                genre TEXT NOT NULL,
                                file_path TEXT NOT NULL
                            );";

                        using (SQLiteCommand command = new SQLiteCommand(createSongsTableQuery, connection))
                        {
                            command.ExecuteNonQuery();
                            MessageBox.Show("Tabela 'Songs' je bila uspešno ustvarjena.", "Uspešno", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Tabela 'Songs' že obstaja.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                string checkMoviesTableQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='Movies';";
                using (SQLiteCommand checkTableCommand = new SQLiteCommand(checkMoviesTableQuery, connection))
                {
                    var result = checkTableCommand.ExecuteScalar();
                    if (result == null)
                    {
                        string createMoviesTableQuery = @"
                            CREATE TABLE Movies (
                                id INTEGER PRIMARY KEY AUTOINCREMENT,
                                title TEXT NOT NULL,
                                director TEXT NOT NULL,
                                year_Released TEXT NOT NULL,
                                rating DOUBLE NOT NULL,
                                runtime INTEGER NOT NULL,
                                file_path TEXT NOT NULL
                            );";

                        using (SQLiteCommand command = new SQLiteCommand(createMoviesTableQuery, connection))
                        {
                            command.ExecuteNonQuery();
                            MessageBox.Show("Tabela 'Movies' je bila uspešno ustvarjena.", "Uspešno", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Tabela 'Movies' že obstaja.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }

            MessageBox.Show("Inicializacija baze podatkov je zaključena.", "Zaključeno", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void AddSampleUsers()
        {

        }

        public static void AddSampleBooksFromCsv(string csvPath)
        {

        }
    }
}


//KODA ZA VSTAVLJANJE PODATKOV IZ Movies_list.csv v tabelo movies
/*
  try
            {
                string filePath = @"C:\\Users\\Uporabnik\\Documents\\GitHub\\Diplomska\\Diplomska\\Diplomska\\Movies\\movies_List.csv";
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Datoteka ni bila najdena.", "Napaka", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var connection = new SQLiteConnection("Data Source=C:\\Users\\Uporabnik\\Documents\\GitHub\\Diplomska\\Diplomska\\Diplomska\\SQLite_Connection.db;Version=3;"))
                {
                    connection.Open();

                    var lines = File.ReadAllLines(filePath);
                    foreach (var line in lines.Skip(1)) 
                    {
                        var data = line.Split(',');

                        string title = data[0];
                        string director = data[1];
                        string genre = data[2];
                        string yearReleased = data[3];
                        long runtime = long.Parse(data[4]);
                        double rating = double.Parse(data[5]);
                        string filePathColumn = data[6];

                        string query = @"INSERT OR IGNORE INTO Movies (title, director, genre, year_Released, runtime, rating, file_path) 
                                        VALUES (@title, @director, @genre, @yearReleased, @runtime, @rating, @filePath);";

                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@title", title);
                            command.Parameters.AddWithValue("@director", director);
                            command.Parameters.AddWithValue("@genre", genre);
                            command.Parameters.AddWithValue("@yearReleased", yearReleased);
                            command.Parameters.AddWithValue("@runtime", runtime);
                            command.Parameters.AddWithValue("@rating", rating);
                            command.Parameters.AddWithValue("@filePath", filePathColumn);

                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Podatki so bili uspešno vstavljeni v bazo podatkov.", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Napaka pri vstavljanju podatkov: " + ex.Message, "Napaka", MessageBoxButton.OK, MessageBoxImage.Error);
            }
 */

