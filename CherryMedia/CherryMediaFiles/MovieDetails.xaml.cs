using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Diplomska.CherryFrame;

namespace Diplomska
{
    /// <summary>
    /// Interaction logic for MovieDetails.xaml
    /// </summary>
    /// 
   
    public partial class MovieDetails : Window
    {
        private CherryFrame.Movie _selectedMovie;

        private string connectionString = "Data Source=C:\\Projects\\CherryMedia\\CherryMedia\\CherryMediaFiles\\SqLite_Connection.db;Version=3;";
        public MovieDetails(CherryFrame.Movie movie)
        {
            InitializeComponent();

            _selectedMovie = movie;

            this.DataContext = _selectedMovie;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            // Ko klikneš Play, odpreš predvajalnik s potjo do videa
            Window playerWindow = new Window
            {
                Content = new MoviePlayer(_selectedMovie.VideoPath),
                WindowState = WindowState.Maximized
            };
            playerWindow.Show();
            this.Close(); // Zapreš okno s podrobnostmi
        }

        private void Rate_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string rating = btn.Tag.ToString();

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();

                    // Ustvarimo tabelo, če še ne obstaja (priprava na prihodnost)
                    string createTable = "CREATE TABLE IF NOT EXISTS UserRatings (Title TEXT PRIMARY KEY, Rating TEXT)";
                    using (SQLiteCommand cmd = new SQLiteCommand(createTable, conn)) { cmd.ExecuteNonQuery(); }

                    // Vstavimo ali posodobimo oceno (INSERT or REPLACE)
                    string sql = "INSERT OR REPLACE INTO UserRatings (Title, Rating) VALUES (@title, @rating)";
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", _selectedMovie.Title);
                        cmd.Parameters.AddWithValue("@rating", rating);
                        cmd.ExecuteNonQuery();
                    }
                }

                CurrentRatingText.Text = "Tvoja ocena: " + rating + " ⭐";
                MessageBox.Show($"Film {_selectedMovie.Title} si ocenil z {rating}!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Napaka pri shranjevanju ocene: " + ex.Message);
            }
        }
    }
}
