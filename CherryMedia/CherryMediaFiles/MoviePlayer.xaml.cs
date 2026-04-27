using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Diplomska
{
    public partial class MoviePlayer : UserControl
    {
        private string videoPath;
        private DispatcherTimer timer;
        private bool isDraggingSlider = false;
        private DispatcherTimer inactivityTimer;
        private DateTime lastMouseMoveTime;
        private bool controlsVisible = true;
        private bool isPlaying = false;
        private bool isFullScreen = false;



        public MoviePlayer(string path)
        {
            InitializeComponent();

            videoPath = path;
            Player.LoadedBehavior = MediaState.Manual;
            Player.Source = new Uri(videoPath, UriKind.Absolute);
            Player.MediaOpened += Player_MediaOpened;
            Player.MediaFailed += (s, e) => MessageBox.Show("Napaka: " + e.ErrorException.Message);
            Player.Play();
            isPlaying = true;

            // progress update timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            timer.Start();

            // inactivity detection
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromSeconds(1);
            inactivityTimer.Tick += InactivityTimer_Tick;
            inactivityTimer.Start();

            lastMouseMoveTime = DateTime.Now;
            this.MouseMove += MoviePlayer_MouseMove;

            this.KeyDown += MoviePlayer_KeyDown;

            this.Focusable = true;
            this.Focus();
        }


        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan)
            {
                ProgressSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!isDraggingSlider && Player.NaturalDuration.HasTimeSpan)
            {
                ProgressSlider.Value = Player.Position.TotalSeconds;
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDraggingSlider)
            {
                // can display preview time here if you want
            }
        }

        private void ProgressSlider_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan)
            {
                Player.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
                isDraggingSlider = false;
            }
        }

        private void ProgressSlider_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isDraggingSlider = true;
        }

        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
            if ((DateTime.Now - lastMouseMoveTime).TotalSeconds > 3 && controlsVisible)
            {
                
                ProgressSlider.Visibility = Visibility.Collapsed;
                PlayPauseButton.Visibility = Visibility.Collapsed;
                reverseButton.Visibility = Visibility.Collapsed;
                forwardButton.Visibility = Visibility.Collapsed;
                FullScreenButton.Visibility = Visibility.Collapsed;
                controlsVisible = false;
            }
        }
        private void MoviePlayer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lastMouseMoveTime = DateTime.Now;

            if (!controlsVisible)
            {
                
                ProgressSlider.Visibility = Visibility.Visible;
                PlayPauseButton.Visibility = Visibility.Visible;
                reverseButton.Visibility = Visibility.Visible;
                forwardButton.Visibility = Visibility.Visible;
                FullScreenButton.Visibility = Visibility.Visible;
                controlsVisible = true;
            }
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source == null)
                return;

            if (isPlaying)
            {
                Player.Pause();
                PlayPauseButton.Content = "▶️";
            }
            else
            {
                Player.Play();
                PlayPauseButton.Content = "⏸️";
            }

            isPlaying = !isPlaying;
        }

        private bool IsPlaying()
        {
            return Player != null && Player.Clock == null && Player.Position > TimeSpan.Zero;
        }



        private void Rewind_Click(object sender, RoutedEventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan)
            {
                Player.Position = Player.Position.Subtract(TimeSpan.FromSeconds(30));
            }
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan)
            {
                Player.Position = Player.Position.Add(TimeSpan.FromSeconds(30));
            }
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow != null)
            {
                if (!isFullScreen)
                {
                    // VKLOP FULLSCREEN
                    parentWindow.WindowStyle = WindowStyle.None; 
                    parentWindow.WindowState = WindowState.Maximized; 
                    FullScreenButton.Content = "Exit Fullscreen";
                    isFullScreen = true;
                }
                else
                {
                    // IZKLOP FULLSCREEN
                    parentWindow.WindowStyle = WindowStyle.SingleBorderWindow; 
                    parentWindow.WindowState = WindowState.Normal; 
                    FullScreenButton.Content = "📺 Fullscreen";
                    isFullScreen = false;
                }
            }
        }

        private void MoviePlayer_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (isFullScreen)
                {
                   
                    FullScreen_Click(null, null);
                }

            }
        }

    }
}
