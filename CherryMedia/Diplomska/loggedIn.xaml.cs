using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Diplomska
{
    public partial class loggedIn : UserControl
    {
        public loggedIn()
        {
            InitializeComponent();
        }

        private void SpotifyButton_Click(object sender, RoutedEventArgs e)
        {
            Window newCherryNoteWindow = new Window
            {
                Title = "CherryNote",
                Content = new CherryNote(), 
                Height = 600,
                Width = 1100,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            newCherryNoteWindow.Show();

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }

        private void FilmiButton_Click(object sender, RoutedEventArgs e)
        {
            Window newCherryFrameWindow = new Window
            {
                Title = "CherryFrame",
                Height = 600,
                Width = 1100,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            CherryFrame cherryFrame = new CherryFrame(); // Assuming CherryFrame is a UserControl
            newCherryFrameWindow.Content = cherryFrame; // Set the UserControl as content

            Window parentWindow = Window.GetWindow(this);
            newCherryFrameWindow.Show();

            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }



        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow newLoginWindow = new MainWindow();
            newLoginWindow.Show();

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }
    }
}
