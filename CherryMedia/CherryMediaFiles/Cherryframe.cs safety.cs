/*using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;

namespace Diplomska
{

    public partial class CherryFrame : UserControl
    {
        public CherryFrame()
        {
            InitializeComponent();
            LoadMoviesFromFolderAndDatabase();

        }
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Placeholder.Visibility = Visibility.Collapsed;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                Placeholder.Visibility = Visibility.Visible;
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchAndFilter();
        }

        private void LoadMoviesFromFolderAndDatabase()
        {

            string folderPath = @"C:\Projects\CherryMedia\Diplomska\Diplomska\Diplomska\Movies\";
            string posterPath = @"C:\Projects\CherryMedia\Diplomska\Diplomska\Diplomska\Movies\Posters";
            string connectionString = @"Data Source=C:\Projects\CherryMedia\Diplomska\Diplomska\Diplomska\SQLite_Connection.db;Version=3;";//@"Data Source=C:\Users\Uporabnik\Documents\GitHub\Diplomska\Diplomska\Diplomska\SQLite_Connection.db;Version=3;";

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Mapa za filme ne obstaja.", "Napaka", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            allMovies.Clear();

            string[] videoFiles = Directory.GetFiles(folderPath, "*.mp4");

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                foreach (string videoPath in videoFiles)
                {
                    string fileName = Path.GetFileName(videoPath);

                    string query = "SELECT * FROM Movies WHERE file_path LIKE @file";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@file", "%" + fileName);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string title = reader["title"].ToString();
                                string genre = reader["genre"].ToString();
                                string imagePath = Path.ChangeExtension(videoPath, ".jpg");
                                if (!File.Exists(imagePath))
                                    imagePath = Path.ChangeExtension(videoPath, ".png");

                                allMovies.Add(new Movie
                                {
                                    Title = title,
                                    Genre = genre,
                                    ImagePath = imagePath,
                                    VideoPath = videoPath
                                });
                            }
                        }
                    }
                }
            }

            // Dodaj vse žanre v ComboBox
            var genres = allMovies.Select(m => m.Genre).Distinct().OrderBy(g => g).ToList();
            FilterBox.Items.Clear();
            FilterBox.Items.Add("All Movies");
            foreach (var genre in genres)
            {
                FilterBox.Items.Add(genre);
            }
            FilterBox.SelectedIndex = 0;

            DisplayMovies("All Movies");
        }


        private void AddMovieToUI(string title, string imagePath, string videoPath)
        {
            StackPanel movieItem = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5),
                Width = 150,
                Cursor = Cursors.Hand,
                Tag = videoPath
            };

            Image movieImage = new Image
            {
                Width = 150,
                Height = 220,
                Margin = new Thickness(5),
                Source = File.Exists(imagePath) ? new BitmapImage(new Uri(imagePath)) : null
            };

            TextBlock movieTitle = new TextBlock
            {
                Text = title,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5, 5, 5, 0)
            };


            movieItem.MouseLeftButtonDown += (s, e) =>
            {
                Window playerWindow = new Window
                {
                    Title = "Movie Player",
                    Content = new MoviePlayer(videoPath),
                    Height = 600,
                    Width = 1100,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = Brushes.Black
                };

                playerWindow.Show();
            };



            movieItem.Children.Add(movieImage);
            movieItem.Children.Add(movieTitle);
            MovieWrap.Children.Add(movieItem);
        }

        public class Movie
        {
            public string Title { get; set; }
            public string Genre { get; set; }
            public string ImagePath { get; set; }
            public string VideoPath { get; set; }
        }
        private List<Movie> allMovies = new List<Movie>();

        private void DisplayMovies(string genre)
        {
            MovieWrap.Children.Clear();

            var filtered = genre == "All Movies"
                ? allMovies
                : allMovies.Where(m => m.Genre == genre).ToList();

            foreach (var movie in filtered)
            {
                AddMovieToUI(movie.Title, movie.ImagePath, movie.VideoPath);
            }
        }

        private void ApplySearchAndFilter()
        {
            string selectedGenre = FilterBox.SelectedItem?.ToString() ?? "All Movies";
            string searchText = SearchBox.Text?.ToLower() ?? "";

            var filtered = allMovies
                .Where(m => (selectedGenre == "All Movies" || m.Genre == selectedGenre) &&
                            (string.IsNullOrWhiteSpace(searchText) || m.Title.ToLower().Contains(searchText)))
                .ToList();

            MovieWrap.Children.Clear();
            foreach (var movie in filtered)
            {
                AddMovieToUI(movie.Title, movie.ImagePath, movie.VideoPath);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Window newLoggedInWindow = new Window
            {
                Title = "CherryNote",
                Content = new loggedIn(),
                Height = 600,
                Width = 1100,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            newLoggedInWindow.Show();

            Window.GetWindow(this)?.Close();
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySearchAndFilter();
        }

        private async void Baza_Click(object sender, RoutedEventArgs e)
        {

            string folderPath = @"C:\Projects\CherryMedia\Diplomska\Diplomska\Diplomska\Movies\";

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Mapa s filmi ne obstaja!");
                return;
            }


            string[] videoFiles = Directory.GetFiles(folderPath, "*.mp4");

            if (videoFiles.Length == 0)
            {
                MessageBox.Show("V mapi ni nobenih .mp4 datotek.");
                return;
            }

            string apiKey = "84e89b4a";
            using (HttpClient client = new HttpClient())
            {
                foreach (string videoPath in videoFiles)
                {
                    // Očistimo ime datoteke za API
                    string rawName = Path.GetFileNameWithoutExtension(videoPath);
                    string cleanTitle = rawName.Replace("_", " ");

                    string url = $"http://www.omdbapi.com/?t={Uri.EscapeDataString(cleanTitle)}&apikey={apiKey}";

                    try
                    {
                        var response = await client.GetStringAsync(url);
                        JObject data = JObject.Parse(response);

                        if (data["Response"].ToString() == "True")
                        {

                            string runtimeRaw = data["Runtime"]?.ToString() ?? "N/A";
                            int runtimeMinutes = 0;


                            if (runtimeRaw != "N/A" && runtimeRaw.Contains(" min"))
                            {
                                int.TryParse(runtimeRaw.Replace(" min", ""), out runtimeMinutes);
                            }


                            string info = $"DATOTEKA: {rawName}\n" +
                                          $"API NASLOV: {data["Title"]}\n" +
                                          $"LETO: {data["Year"]}\n" +
                                          $"REŽISER: {data["Director"]}\n" +
                                          $"TRAJANJE: {runtimeMinutes} min (Original: {runtimeRaw})\n" +
                                          $"ŽANR: {data["Genre"]}\n" +
                                          $"IMDb OCENA: {data["imdbRating"]}";

                            MessageBox.Show(info, "Testni Pull Podatkov");
                        }
                        else
                        {
                            MessageBox.Show($"API ni našel filma za: {rawName}", "Ni zadetka");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Napaka pri {rawName}: {ex.Message}");
                    }
                }
            }
            MessageBox.Show("Testiranje vseh datotek je končano.");
        }


    }
}

*/