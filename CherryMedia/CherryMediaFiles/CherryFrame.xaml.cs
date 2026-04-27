using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Diplomska.CherryFrame;

namespace Diplomska
{
    public partial class CherryFrame : UserControl
    {
        private List<Movie> allMovies = new List<Movie>();

     
        private readonly string folderPath = @"C:\Projects\CherryMedia\CherryMedia\CherryMediaFiles\Movies\";
        private readonly string connectionString = @"Data Source=C:\Projects\CherryMedia\CherryMedia\CherryMediaFiles\SQLite_Connection.db;Version=3;";

        public CherryFrame()
        {
            InitializeComponent();
            LoadMoviesFromFolderAndDatabase();
        }

        private async void LoadMoviesFromFolderAndDatabase()
        {
            if (!Directory.Exists(folderPath)) return;

            allMovies.Clear();
            string[] videoFiles = Directory.GetFiles(folderPath, "*.mp4");
            string apiKey = "84e89b4a";

            using (HttpClient client = new HttpClient())
            {
                foreach (string videoPath in videoFiles)
                {
                    string fileNameOnly = Path.GetFileNameWithoutExtension(videoPath);

                  
                    var movie = new Movie
                    {
                        Title = fileNameOnly.Replace("_", " "),
                        VideoPath = videoPath,
                        ImagePath = GetPosterPath(folderPath, fileNameOnly), 
                        Genre = "Unknown",
                        Director = "Naložim...",
                        Year = "",
                        Plot = "Iščem opis...",
                        Rating = "-"
                    };

                   
                    try
                    {
                        string url = $"http://www.omdbapi.com/?t={Uri.EscapeDataString(movie.Title)}&apikey={apiKey}";
                        var response = await client.GetStringAsync(url);
                        JObject data = JObject.Parse(response);

                        if (data["Response"]?.ToString() == "True")
                        {
                            movie.Director = data["Director"]?.ToString();
                            movie.Year = data["Year"]?.ToString();
                            movie.Genre = data["Genre"]?.ToString();
                            movie.Plot = data["Plot"]?.ToString();
                            movie.Rating = data["imdbRating"]?.ToString();
                        }
                    }
                    catch { /*  */ }

                    allMovies.Add(movie);
                }
            }

            UpdateGenreFilter();
            DisplayMovies("All Movies");
        }



        private string GetPosterPath(string currentFolder, string fileNameOnly)
        {
           
            string[] extensions = { ".webp", ".jpg", ".png", ".jpeg" };

            foreach (var ext in extensions)
            {
                string testPath = Path.Combine(currentFolder, fileNameOnly + ext);
                if (File.Exists(testPath))
                {
                    return testPath;
                }
            }
            return null;
        }

        private void UpdateGenreFilter()
        {
            var genres = allMovies.Select(m => m.Genre).Distinct().OrderBy(g => g).ToList();
            FilterBox.Items.Clear();
            FilterBox.Items.Add("All Movies");
            foreach (var genre in genres)
            {
                FilterBox.Items.Add(genre);
            }
            FilterBox.SelectedIndex = 0;
        }

        private void DisplayMovies(string genre)
        {
            MovieWrap.Children.Clear();
            var filtered = (genre == "All Movies")
                ? allMovies
                : allMovies.Where(m => m.Genre == genre).ToList();

            foreach (var movie in filtered)
            {
                AddMovieToUI(movie);
            }
        }

        private void AddMovieToUI(Movie movie)
        {

            StackPanel movieItem = new StackPanel
            {
                Width = 150,
                Margin = new Thickness(10),
                Cursor = Cursors.Hand,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45))
            };

            movieItem.MouseLeftButtonDown += (s, e) =>
            {
                MovieDetails detailsWindow = new MovieDetails(movie);
                detailsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                detailsWindow.ShowDialog();
            };


            Image movieImage = new Image { Width = 150, Height = 220, Stretch = Stretch.UniformToFill };
            if (!string.IsNullOrEmpty(movie.ImagePath) && File.Exists(movie.ImagePath))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(movie.ImagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    movieImage.Source = bitmap;
                }
                catch { }
            }

      
            TextBlock movieTitle = new TextBlock { Text = movie.Title, Foreground = Brushes.White, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 5, 0, 0), FontWeight = FontWeights.Bold };


          /*  movieItem.MouseLeftButtonDown += (s, e) => {
                Window playerWindow = new Window { Title = "Predvajanje: " + movie.Title, Content = new MoviePlayer(movie.VideoPath), Height = 600, Width = 1100, WindowStartupLocation = WindowStartupLocation.CenterScreen, Background = Brushes.Black };
                playerWindow.Show();
            };*/

            movieItem.Children.Add(movieImage);
            movieItem.Children.Add(movieTitle);


            MovieWrap.Children.Add(movieItem);
        }

        private void ApplySearchAndFilter()
        {
            string selectedGenre = FilterBox.SelectedItem?.ToString() ?? "All Movies";
            string searchText = SearchBox.Text?.ToLower() ?? "";

            var filtered = allMovies.Where(m =>
                (selectedGenre == "All Movies" || m.Genre == selectedGenre) &&
                (string.IsNullOrWhiteSpace(searchText) || m.Title.ToLower().Contains(searchText))).ToList();

            MovieWrap.Children.Clear();
            foreach (var movie in filtered) AddMovieToUI(movie);
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e) => Placeholder.Visibility = Visibility.Collapsed;
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e) { if (string.IsNullOrWhiteSpace(SearchBox.Text)) Placeholder.Visibility = Visibility.Visible; }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplySearchAndFilter();
        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplySearchAndFilter();
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Window loggedInWindow = new Window
            {
                Title = "Logged In",
                Content = new loggedIn(),
                Height = 600,
                Width = 1100,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            loggedInWindow.Show();

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }


        public class Movie
        {
            public string Title { get; set; }
            public string Genre { get; set; }
            public string ImagePath { get; set; }
            public string VideoPath { get; set; }
            public string Director { get; set; }
            public string Year { get; set; }
            public string Plot { get; set; }
            public string Rating { get; set; }

        }
       




















        /*private async void Baza_Click(object sender, RoutedEventArgs e)
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
        }*/
    }
}