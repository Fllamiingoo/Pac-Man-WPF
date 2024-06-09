using System.ComponentModel.DataAnnotations;
using System.Drawing.Configuration;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PacMan_WPF
{
    public partial class MainWindow : Window
    {
        DispatcherTimer gameTimer = new DispatcherTimer();

        bool canMoveLeft, canMoveRight, canMoveDown, canMoveUp;

        int speed = 8;
        int ghostSpeed = 10;
        int ghostStep = 170;
        int currentGhostStep;
        int score = 0;

        Rect pacmanHitBox;

        public MainWindow()
        {
            InitializeComponent();
            GameStart();
        }

        private void CanvasKeyDown(object sender, KeyEventArgs e)
        {
            canMoveLeft = canMoveRight = canMoveUp = canMoveDown = false;

            if (e.Key == Key.Left) { canMoveLeft = true; pacman.RenderTransform = new RotateTransform(-180, pacman.Width / 2, pacman.Height / 2); }
            if (e.Key == Key.Right) { canMoveRight = true; pacman.RenderTransform = new RotateTransform(0, pacman.Width / 2, pacman.Height / 2); }
            if (e.Key == Key.Up) { canMoveUp = true; pacman.RenderTransform = new RotateTransform(-90, pacman.Width / 2, pacman.Height / 2); }
            if (e.Key == Key.Down) { canMoveDown = true; pacman.RenderTransform = new RotateTransform(90, pacman.Width / 2, pacman.Height / 2); }
        }

        private void GameStart()
        {
            MyCanvas.Focus();
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Start();
            currentGhostStep = ghostStep;

            LoadImage(pacman, "pack://application:,,,/images/pacman.png");
            LoadImage(blue, "pack://application:,,,/images/blue.png");
            LoadImage(purple, "pack://application:,,,/images/purple.png");
            LoadImage(pink, "pack://application:,,,/images/pink.png");

            Uri coinImage = new Uri("pack://application:,,,/images/coin.png");
            foreach (var child in MyCanvas.Children)
            {
                if (child is Rectangle rectangle && rectangle.Tag?.ToString() == "coin")
                {
                    rectangle.Fill = new ImageBrush(new BitmapImage(coinImage));
                }
            }
        }

        private void LoadImage(Rectangle rect, string uri)
        {
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(uri));
            rect.Fill = imageBrush;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            txtScore.Content = "Score: " + score;

            MovePacman();

            pacmanHitBox = new Rect(Canvas.GetLeft(pacman), Canvas.GetTop(pacman), pacman.Width, pacman.Height);

            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                Rect hitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                switch (x.Tag?.ToString())
                {
                    case "wall":
                        HandleWallCollision(x, hitBox);
                        break;
                    case "coin":
                        HandleCoinCollision(x, hitBox);
                        break;
                    case "ghost":
                        HandleGhostCollision(x, hitBox);
                        MoveGhost(x);
                        break;
                }
            }

            if (score == 152)
            {
                GameOver("You Win, you collected all of the coins");
            }
        }

        private void MovePacman()
        {
            double left = Canvas.GetLeft(pacman);
            double top = Canvas.GetTop(pacman);

            if (canMoveRight && left + 70 <= Application.Current.MainWindow.Width) { Canvas.SetLeft(pacman, left + speed); }
            if (canMoveLeft && left - 10 >= 1) { Canvas.SetLeft(pacman, left - speed); }
            if (canMoveUp && top >= 1) { Canvas.SetTop(pacman, top - speed); }
            if (canMoveDown && top + 80 <= Application.Current.MainWindow.Height) { Canvas.SetTop(pacman, top + speed); }
        }

        private void HandleWallCollision(Rectangle wall, Rect wallHitBox)
        {
            if (pacmanHitBox.IntersectsWith(wallHitBox))
            {
                if (canMoveLeft) { Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) + 10); canMoveLeft = false; }
                if (canMoveRight) { Canvas.SetLeft(pacman, Canvas.GetLeft(pacman) - 10); canMoveRight = false; }
                if (canMoveDown) { Canvas.SetTop(pacman, Canvas.GetTop(pacman) - 10); canMoveDown = false; }
                if (canMoveUp) { Canvas.SetTop(pacman, Canvas.GetTop(pacman) + 10); canMoveUp = false; }
            }
        }

        private void HandleCoinCollision(Rectangle coin, Rect coinHitBox)
        {
            if (pacmanHitBox.IntersectsWith(coinHitBox) && coin.Visibility == Visibility.Visible)
            {
                coin.Visibility = Visibility.Hidden;
                score++;
            }
        }

        private void HandleGhostCollision(Rectangle ghost, Rect ghostHitBox)
        {
            if (pacmanHitBox.IntersectsWith(ghostHitBox))
            {
                GameOver("Ghosts got you, click ok to play again");
            }
        }

        private void MoveGhost(Rectangle ghost)
        {
            double left = Canvas.GetLeft(ghost);
            if (ghost.Name == "blue") { Canvas.SetLeft(ghost, left - ghostSpeed); }
            else { Canvas.SetLeft(ghost, left + ghostSpeed); }

            currentGhostStep--;
            if (currentGhostStep < 1)
            {
                currentGhostStep = ghostStep;
                ghostSpeed = -ghostSpeed;
            }
        }

        private void GameOver(string message)
        {
            gameTimer.Stop();
            MessageBox.Show(message, "PacMan WPF interpretation");

            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}