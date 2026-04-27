                  using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Diplomska
{
    public partial class CherryNote : UserControl
    {

        private Dictionary<string, List<string>> playlists = new Dictionary<string, List<string>>();
        private string currentPlaylist = null;
        private DispatcherTimer timer;
        private bool isDraggingSlider = false;
        private string currentSongPath = null;
        private string connectionString = "Data Source=C:\\Projects\\CherryMedia\\Diplomska\\Diplomska\\Diplomska\\SqLite_Connection.db;Version=3;";


        public CherryNote()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateProgress;
            LoadPlaylistsFromDatabase();
        }

        private void CreatePlaylist_Click(object sender, RoutedEventArgs a)
        {
            string playlistName = PromptForPlaylistName();
            if (string.IsNullOrWhiteSpace(playlistName)) return;

            string sanitizedPlaylistName = playlistName.Replace(" ", "_").Replace("-", "_");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createPlaylistTableQuery = $@"
        CREATE TABLE IF NOT EXISTS [{sanitizedPlaylistName}] (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT NOT NULL,
            artist TEXT NOT NULL,
            file_path TEXT NOT NULL
        );";

                using (SQLiteCommand command = new SQLiteCommand(createPlaylistTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            if (!playlists.ContainsKey(sanitizedPlaylistName))
            {
                playlists[sanitizedPlaylistName] = new List<string>();
            }
            PlaylistView.Items.Add(playlistName);
            WelcomeScreen.Visibility = Visibility.Collapsed;
            PlaylistContent.Visibility = Visibility.Visible;
            PlaylistTitle.Text = playlistName;

            MessageBox.Show($"Playlist '{playlistName}' has been created!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void DeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentPlaylist))
            {
                MessageBox.Show("Please select a playlist first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete the playlist '{currentPlaylist}'?",
                                         "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    string deletePlaylistTableQuery = $"DROP TABLE IF EXISTS [{currentPlaylist.Replace(" ", "_")}];";
                    using (SQLiteCommand command = new SQLiteCommand(deletePlaylistTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Remove from UI
                PlaylistView.Items.Remove(currentPlaylist);
                PlaylistContent.Visibility = Visibility.Collapsed;
                WelcomeScreen.Visibility = Visibility.Visible;
                currentPlaylist = null;

                MessageBox.Show("Playlist deleted successfully!", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private string PromptForPlaylistName()
        {
            Window inputDialog = new Window
            {
                Title = "Enter Playlist Name",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel stackPanel = new StackPanel { Margin = new Thickness(10) };
            TextBox textBox = new TextBox { Width = 260, Margin = new Thickness(0, 5, 0, 10) };
            Button confirmButton = new Button { Content = "OK", Width = 80, HorizontalAlignment = HorizontalAlignment.Center };

            confirmButton.Click += (sender, e) => { inputDialog.DialogResult = true; inputDialog.Close(); };

            stackPanel.Children.Add(new TextBlock { Text = "Enter a name for your playlist:", FontSize = 14 });
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(confirmButton);
            inputDialog.Content = stackPanel;

            bool? result = inputDialog.ShowDialog();
            return result == true ? textBox.Text.Trim() : null;


        }

        private void AddSongToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentPlaylist))
            {
                MessageBox.Show("Please select a playlist first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Audio Files|*.mp4;*.mp3;*.wav;*.wma;*.mov;*.ogg;*.flac|All Files|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        if (playlists[currentPlaylist].Contains(filePath))
                        {
                            MessageBox.Show($"Pesem '{Path.GetFileName(filePath)}' je že v tej playlisti!", "Napaka", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        string artistName = "Unknown Artist";

                        string insertSongQuery = $@"
                INSERT INTO [{currentPlaylist.Replace(" ", "_")}] (title, artist, file_path) 
                VALUES (@title, @artist, @filePath);";

                        using (SQLiteCommand command = new SQLiteCommand(insertSongQuery, connection))
                        {
                            command.Parameters.AddWithValue("@title", fileName);
                            command.Parameters.AddWithValue("@artist", artistName);
                            command.Parameters.AddWithValue("@filePath", filePath);
                            command.ExecuteNonQuery();
                        }

                        int songIndex = playlists[currentPlaylist].Count + 1;
                        playlists[currentPlaylist].Add(filePath);

                        AddSongToUI(filePath, songIndex);
                    }
                }
            }
        }



        private void AddSongToUI(string filePath, int songIndex)
        {
            StackPanel songItem = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5),
                Background = Brushes.Transparent,
                Tag = filePath
            };

            TextBlock songNumber = new TextBlock
            {
                Text = $"{songIndex}.",
                Foreground = Brushes.White,
                Width = 30,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16
            };

            Button playButton = new Button
            {
                Content = "▶",
                Width = 30,
                Height = 30,
                Margin = new Thickness(10, 0, 10, 0)
            };
            playButton.Click += (s, e) => PlaySong(filePath);

            Button deleteButton = new Button
            {
                Content = "❌",
                Width = 30,
                Height = 30,
                Margin = new Thickness(10, 0, 10, 0)
            };
            deleteButton.Click += (s, e) => DeleteSong(filePath, songItem);

            string imagePath = File.Exists(Path.ChangeExtension(filePath, ".png"))
                ? Path.ChangeExtension(filePath, ".png")
                : File.Exists(Path.ChangeExtension(filePath, ".jpg"))
                    ? Path.ChangeExtension(filePath, ".jpg")
                    : null;

            Image songImage = new Image
            {
                Width = 50,
                Height = 50,
                Margin = new Thickness(10, 0, 10, 0),
                Visibility = (imagePath != null && File.Exists(imagePath)) ? Visibility.Visible : Visibility.Collapsed
            };

            if (imagePath != null && File.Exists(imagePath))
            {
                songImage.Source = new BitmapImage(new Uri(imagePath));
            }

            TextBlock songText = new TextBlock
            {
                Text = Path.GetFileNameWithoutExtension(filePath),
                Foreground = Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            };
            songText.MouseLeftButtonDown += (s, e) => PlaySong(filePath);

            songItem.Children.Add(songNumber);
            songItem.Children.Add(playButton);
            songItem.Children.Add(songImage);
            songItem.Children.Add(songText);
            songItem.Children.Add(deleteButton);

            SongsList.Children.Add(songItem);
        }


        private StackPanel currentPlayingSong = null;

        private void PlaySong(string filePath)
        {

            if (!System.IO.File.Exists(filePath))
            {
                MessageBox.Show("Ni datoteke" + filePath);
                return;
            }

            MediaPlayer.Source = new Uri(filePath, UriKind.Absolute);
            MediaPlayer.Play();
            timer.Start();
            currentSongPath = filePath;
            CurrentSongTitle.Text = Path.GetFileNameWithoutExtension(filePath);

            if (currentPlayingSong != null)
            {
                currentPlayingSong.Background = Brushes.Transparent;
            }

            foreach (StackPanel item in SongsList.Children)
            {
                if (item.Tag != null && item.Tag.ToString() == filePath)
                {
                    item.Background = Brushes.DodgerBlue;
                    currentPlayingSong = item;
                    break;
                }
            }
        }

        private void DeleteSong(string filePath, StackPanel songItem)
        {
            if (string.IsNullOrEmpty(currentPlaylist))
                return;

            var result = MessageBox.Show($"Ali želite izbrisati pesem '{Path.GetFileName(filePath)}' iz playliste?",
                                         "Potrditev", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string deleteSongQuery = $"DELETE FROM [{currentPlaylist.Replace(" ", "_")}] WHERE file_path = @filePath;";
                    using (SQLiteCommand command = new SQLiteCommand(deleteSongQuery, connection))
                    {
                        command.Parameters.AddWithValue("@filePath", filePath);
                        command.ExecuteNonQuery();
                    }
                }

                playlists[currentPlaylist].Remove(filePath);
                SongsList.Children.Remove(songItem);

                MessageBox.Show("Pesem uspešno izbrisana!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PlaylistView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistView.SelectedItem != null)
            {
                currentPlaylist = PlaylistView.SelectedItem.ToString();
                WelcomeScreen.Visibility = Visibility.Collapsed;
                PlaylistContent.Visibility = Visibility.Visible;
                SongsList.Children.Clear();
                PlaylistTitle.Text = currentPlaylist;

                int songIndex = 1;
                foreach (var song in playlists[currentPlaylist])
                {
                    AddSongToUI(song, songIndex);
                    songIndex++;
                }

            }
        }
        private async void LoadPlaylistsFromDatabase()
        {
            await Task.Run(() =>
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string playlistName = reader.GetString(0);

                            if (playlistName.Equals("Users", StringComparison.OrdinalIgnoreCase) ||
                                playlistName.Equals("Songs", StringComparison.OrdinalIgnoreCase) ||
                                playlistName.Equals("Movies", StringComparison.OrdinalIgnoreCase))
                                continue;

                            Dispatcher.Invoke(() =>
                            {
                                if (!playlists.ContainsKey(playlistName))
                                {
                                    playlists[playlistName] = new List<string>();
                                    PlaylistView.Items.Add(playlistName);
                                }
                            });

                            LoadSongsFromDatabase(playlistName);
                        }
                    }
                }
            });
        }




        private void LoadSongsFromDatabase(string playlistName)
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string pragmaQuery = $"PRAGMA table_info([{playlistName.Replace(" ", "_")}] );";
                var columns = new HashSet<string>();

                using (SQLiteCommand pragmaCmd = new SQLiteCommand(pragmaQuery, connection))
                using (SQLiteDataReader pragmaReader = pragmaCmd.ExecuteReader())
                {
                    while (pragmaReader.Read())
                    {
                        columns.Add(pragmaReader["name"].ToString().ToLower());
                    }
                }

                if (!(columns.Contains("title") && columns.Contains("artist") && columns.Contains("file_path")))
                {
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    if (!playlists.ContainsKey(playlistName))
                    {
                        playlists[playlistName] = new List<string>();
                        PlaylistView.Items.Add(playlistName);
                    }
                });

                string query = $"SELECT title, artist, file_path FROM [{playlistName.Replace(" ", "_")}]";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string filePath = reader.GetString(2);
                        Dispatcher.Invoke(() =>
                        {
                            playlists[playlistName].Add(filePath);
                        });
                    }
                }
            }
        }

        private void BackToLoggedIn_Click(object sender, RoutedEventArgs e)
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


        private void UpdateProgress(object sender, EventArgs e)
        {
            if (!isDraggingSlider && MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                ProgressSlider.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                ProgressSlider.Value = MediaPlayer.Position.TotalSeconds;
                TimeLabel.Content = MediaPlayer.Position.ToString(@"mm\:ss");
            }
        }


        private void PreviousClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentPlaylist) || playlists[currentPlaylist].Count == 0)
                return;

            int currentIndex = playlists[currentPlaylist].IndexOf(currentSongPath);

            if (currentIndex > 0)
            {
                string previousSong = playlists[currentPlaylist][currentIndex - 1];
                PlaySong(previousSong);
            }
            else
            {
                MediaPlayer.Stop();
                timer.Stop();
                ProgressSlider.Value = 0;
                TimeLabel.Content = "00:00";
                currentSongPath = null;
            }
        }
        private void StopClick(object sender, RoutedEventArgs e) => MediaPlayer.Stop();
        private void PlayClick(object sender, RoutedEventArgs e) => MediaPlayer.Play();
        private void PauseClick(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Pause();
            timer.Stop();
        }
        private void NextClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentPlaylist) || playlists[currentPlaylist].Count == 0)
                return;

            int currentIndex = playlists[currentPlaylist].IndexOf(currentSongPath);

            if (currentIndex < playlists[currentPlaylist].Count - 1)
            {
                string nextSong = playlists[currentPlaylist][currentIndex + 1];
                PlaySong(nextSong);
            }
        }



        private void ProgressSlider_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDraggingSlider = true;
        }

        private void ProgressSlider_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                MediaPlayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
            }
            isDraggingSlider = false;
        }


        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }




    }
}
