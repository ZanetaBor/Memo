using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Generic;
using System;
using System.Diagnostics.Eventing.Reader;

namespace Memo
{
    public partial class MainWindow : Window
    {
        private readonly List<string> words = new List<string>
         {
             "Dog", "Cat", "Fish", "Bird", "Horse", "Mouse", "Lion", "Elephant",
             "Pies", "Kot", "Ryba", "Ptak", "Koń", "Mysz", "Lew", "Słoń"
         };

        private readonly Dictionary<string, string> wordPairs = new Dictionary<string, string>
         {
             { "Dog", "Pies" }, { "Cat", "Kot" }, { "Fish", "Ryba" }, { "Bird", "Ptak" },
             { "Horse", "Koń" }, { "Mouse", "Mysz" }, { "Lion", "Lew" }, { "Elephant", "Słoń" }
         };

        private readonly Dictionary<string, SolidColorBrush> wordColors = new Dictionary<string, SolidColorBrush>
         {
             { "Dog", Brushes.Red }, { "Cat", Brushes.Blue }, { "Fish", Brushes.Green }, { "Bird", Brushes.Yellow },
             { "Horse", Brushes.Orange }, { "Mouse", Brushes.Purple }, { "Lion", Brushes.Pink }, { "Elephant", Brushes.LightBlue }, { "Pies", Brushes.Red }, { "Kot", Brushes.Blue }, { "Ryba", Brushes.Green }, { "Ptak", Brushes.Yellow },
             { "Koń", Brushes.Orange }, { "Mysz", Brushes.Purple }, { "Lew", Brushes.Pink }, { "Słoń", Brushes.LightBlue }
         };

        private readonly Dictionary<string, string> buttonWords = new Dictionary<string, string>();

        private readonly Random random = new Random();
        private Button firstClickedButton = null;
        private string firstWord = " ";

        private DispatcherTimer timer;
        private DateTime startTime;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void InitializeGame()
        {
            var remainingWords = new List<string>(words);
            // Create a 4x4 grid of buttons
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    var wordIndex = random.Next(remainingWords.Count);
                    var randomWord = remainingWords[wordIndex];
                    remainingWords.RemoveAt(wordIndex);

                    var coordinates = $"{row},{col}";
                    buttonWords.Add(coordinates, randomWord); // assign a word to the coordinates of the button
                    var button = new Button
                    {
                        Height = 50,
                        Width = 50,
                        Padding = new Thickness(5),
                        Background = Brushes.Gray,
                        Content = "",
                        Tag = coordinates
                    };

                    button.Click += Button_Click;
                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);
                    gameGrid.Children.Add(button);
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var coordinates = button.Tag.ToString();
            var word = buttonWords[coordinates];
            button.Content = word;
            if (firstClickedButton == null)  // First click
            {
                firstClickedButton = button;
                firstWord = word;
                var firstcolor = word;
                button.Background = wordColors[firstcolor];
            }
            else
            {
                var secondWord = word; // Second click 
                button.Background = wordColors[secondWord];
                bool findKey = await Task.Run(() => CheckPairValidity(firstWord, secondWord));

                if (!findKey)
                {
                    // Incorrect pair
                    await Task.Delay(1000);
                    if (firstClickedButton != null)
                    {
                        firstClickedButton.Content = "";
                        firstClickedButton.Background = Brushes.Gray;
                    }
                    button.Content = "";
                    button.Background = Brushes.Gray;
                }
                else
                {
                    // Correct pair
                    button.IsEnabled = false; // If correct enable button
                    firstClickedButton.IsEnabled = false;
                }

                firstClickedButton = null;
            }
            CheckIfAllPairsFound();
        }

        private bool CheckPairValidity(string firstWord, string secondWord)
        {
            if (wordPairs.ContainsKey(firstWord))
            {
                if (wordPairs.ContainsKey(firstWord) && wordPairs[firstWord] == secondWord)
                {
                    return true;
                }
            }
            else
            {
                foreach (var para in wordPairs)
                {
                    if (para.Value == firstWord)
                    {
                        if (wordPairs.ContainsKey(secondWord) && wordPairs[secondWord] == firstWord)
                            return true;
                    }
                }
            }
            return false;
        }

        private void CheckIfAllPairsFound()
        {
            bool allPairsFound = true;
            foreach (var button in gameGrid.Children)
            {
                if (button is Button)
                {
                    if (((Button)button).IsEnabled)
                    {
                        allPairsFound = false; 
                    }
                }
            }

            if (allPairsFound)
            {
                timer.Stop();
                MessageBox.Show("Congratulations! You've found all pairs.");
            }
        }
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            gameGrid.Children.Clear();
            InitializeGame();
            startTime = DateTime.Now;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - startTime;
            timeDisplay.Text = elapsed.ToString(@"hh\:mm\:ss");
        }
    }
}
