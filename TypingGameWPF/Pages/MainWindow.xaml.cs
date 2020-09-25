using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TypingGameWPF.Models;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace TypingGameWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        IMongoDatabase db;

        List<TutorialModel> tutorialsList;
        private TutorialModel _currentTutorial;
        public TutorialModel CurrentTutorial
        {
            get { return _currentTutorial; }
            set
            {
                if (_currentTutorial != value)
                {
                    _currentTutorial = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _currentParagraph;
        public string CurrentParagraph
        {
            get { return _currentParagraph; }
            set
            {
                if (_currentParagraph != value)
                {
                    _currentParagraph = value;
                    OnPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public Stopwatch stopWatch = new Stopwatch();
        int CorrectCharacters = 0;
        int errors = 0;
        double Accuracy;
        double Time;
        double WPM;

        public MainWindow()
        {

            DataContext = this;

            var client = new MongoClient();
            db = client.GetDatabase("TypingMetrics");

            InitializeComponent();

            tutorialsList = LoadTutorials<TutorialModel>();
            if(tutorialsList.Count > 0)
                CurrentTutorial = tutorialsList.First();
            ComBoBox1.ItemsSource = tutorialsList;

            RefreshResultsDataGrid();

            RefreshResultsStackPanel();

            DisplayTutorialMetrics();

            int i = tutorialsList.Count() + 1;
            var NewTutorialName = $"Tutorial: {i}";
            TitleTextBlock.Text = NewTutorialName;

        }

        private void RefreshResultsDataGrid()
        {
            var attempts = LoadAttempts<AttemptModel>();
            dataGrid1.ItemsSource = attempts;
        }

        private void RefreshResultsStackPanel()
        {
            tutorialsList = LoadTutorials<TutorialModel>();

            TutorialsStackPanel.Children.Clear();

            foreach (TutorialModel t in tutorialsList)
            {
                initStackPanelButton(t.Name);
            }

            initStackPanelButton("Show All");
        }

        private void initStackPanelButton(string content)
        {
            Border bord = new Border();
            bord.BorderThickness = new Thickness(2);
            bord.CornerRadius = new CornerRadius(10);
            bord.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#343A40"));
            bord.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#707070"));
            Button b = new Button();
            b.Click += new RoutedEventHandler(DisplayTutorialMetrics);
            b.Content = content;
            bord.Child = b;
            TutorialsStackPanel.Children.Add(bord);

        }

        void DisplayTutorialMetrics(object sender, EventArgs e)
        {
            string tutorialName = (sender as Button).Content.ToString();
            TitleTextBlock1.Text = tutorialName;

            var attempts = LoadAttempts<AttemptModel>();
            var results = attempts;

            if (tutorialName != "Show All")
            {
                results = attempts.Where(a => a.Tutorial == tutorialName).ToList();
            }


            dataGrid1.ItemsSource = results;

            var tutorial = LoadTutorials<TutorialModel>();
            var t = tutorial.Where(a => a.Name == tutorialName);

            if (t.Count() > 0)
            {
                var tm = t.First();
                MetricsTextBlock.Text = $"No Attempts: { tm.NoAtempts }\nHigh Score {tm.HighScore}\n";
            }

            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();

            int i = 0;
            foreach (AttemptModel am in results)
            {

                int wpm = Convert.ToInt32(am.WPM);
                valueList.Add(new KeyValuePair<string, int>(i.ToString(), wpm));
                i++;
            }

            ResultGraph1.ItemsSource = valueList;


        }


        void DisplayTutorialMetrics()
        {
           
            var attempts = LoadAttempts<AttemptModel>();
            dataGrid1.ItemsSource = attempts;

            List<KeyValuePair<string, int>> valueList = new List<KeyValuePair<string, int>>();

            int i = 0;
            foreach (AttemptModel am in attempts)
            {
                int wpm = Convert.ToInt32(am.WPM);
                valueList.Add(new KeyValuePair<string, int>(i.ToString(), wpm));
                i++;
            }
            ResultGraph1.ItemsSource = valueList;
        }


        public void InsertTutorial<T>(T record)
        {
            var collection = db.GetCollection<T>("Tutorials");
            collection.InsertOne(record);
        }

        public void InsertAttempt<AttemptModel>(AttemptModel record)
        {
            var collection = db.GetCollection<AttemptModel>("Attempts");
            collection.InsertOne(record);
        }

        public List<TutorialModel> LoadTutorials<TutorialModel>()
        {
            var collection = db.GetCollection<TutorialModel>("Tutorials");
            return collection.Find(new BsonDocument()).ToList();
        }

        public List<AttemptModel> LoadAttempts<AttemptModel>()
        {
            var collection = db.GetCollection<AttemptModel>("Attempts");
            return collection.Find(new BsonDocument()).ToList();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var t = LoadTutorials<TutorialModel>();
            int i = t.Count() + 1;

            var NewTutorialName = $"Tutorial: {i}";
            var FutureTutorialName = $"Tutorial: {i + 1}";

            TitleTextBlock.Text = FutureTutorialName;

            TutorialModel tm = new TutorialModel();
            tm.Paragraph = CustomTextBox.Text;
            tm.Name = NewTutorialName;
            tm.NoAtempts = 0;
            tm.HighScore = 0;

            InsertTutorial(tm);
            tutorialsList = LoadTutorials<TutorialModel>();
            ComBoBox1.ItemsSource = tutorialsList;
            RefreshResultsStackPanel();
            CustomTextBox.Text = "";

        }

      

      

        int ij = 0;
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {

            if (!stopWatch.IsRunning)
                stopWatch.Start();

            string input = textboxInput.Text;

            if (input.Length > ij)
            {
                CheckIfCharacterIsCorrect(input);

                CaluculateMetrics();

                CheckIfTutorialFinished();
            }
        }

        private void CaluculateMetrics()
        {
            TimeSpan ts = stopWatch.Elapsed;
            var v = ts.TotalMinutes;
            WPM = Math.Floor((double)CorrectCharacters / (5 * v));

            Accuracy = Math.Round(1 - ((double)errors / (double)CorrectCharacters), 3);
            Time = stopWatch.Elapsed.TotalMilliseconds;
            var timeInMilliseconds = Math.Round(Time / 1000, 2);

            MetricsTextbox.Text = "WPM: " + WPM + "\nErrors: " + errors.ToString() + "\nAccuracy: " + Accuracy + "\nTime: " + timeInMilliseconds;
        }

        private void CheckIfCharacterIsCorrect(string input)
        {
            if (CurrentTutorial != null)
            {

                var c = input[ij];
                if (c == CurrentTutorial.Paragraph[CorrectCharacters])
                {

                    if (c == ' ')
                    {
                        textboxInput.Text = "";
                        ij = -1;
                    }
                    textBox2.Text = textBox2.Text + c;
                    CorrectCharacters++;
                    ij++;
                    textBox2.Foreground = Brushes.Green;

                }
                else
                {
                    errors++;
                    textBox2.Foreground = Brushes.Red;
                }
            }
        }

        private void CheckIfTutorialFinished()
        {
            if (CorrectCharacters == CurrentTutorial.Paragraph.Length)
            {
                SubmitAttempt();

                CorrectCharacters = 0;
                ij = 0;
                textboxInput.Text = "";
                textBox2.Text = "";
                stopWatch.Restart();
                stopWatch.Stop();
                errors = 0;
                WPM = 0;
                Accuracy = 0;

                CurrentTutorial = null;
            }
        }


        private void SubmitAttempt()
        {
            AttemptModel am = new AttemptModel();
            am.Date = DateTime.Now.Date;
            am.Accuracy = Accuracy;
            am.Time = Time;
            var timeInMilliseconds = Math.Round(Time / 1000,2);

        
            am.TimeString = timeInMilliseconds.ToString();
            am.Tutorial = CurrentTutorial.Name;
            am.WPM = WPM;
            am.Score = Math.Round(Accuracy * WPM,0);
            InsertAttempt<AttemptModel>(am);
            RefreshResultsDataGrid();
            DisplayTutorialMetrics();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            CorrectCharacters = 0;
            ij = 0;
            textboxInput.Text = "";
            textBox2.Text = "";
            stopWatch.Restart();
            stopWatch.Stop();
            errors = 0;
            WPM = 0;
            Accuracy = 0;
            textboxInput.Focus();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var AttemptsCollection = db.GetCollection<AttemptModel>("Attempts");

            var deleteFilter = Builders<AttemptModel>.Filter.Eq("Tutorial", TitleTextBlock1.Text);
            AttemptsCollection.DeleteMany(deleteFilter);

            RefreshResultsDataGrid();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string tutorial = TitleTextBlock1.Text;
            if(tutorial != "Show All")
            {
                var AttemptsCollection = db.GetCollection<AttemptModel>("Attempts");
                var deleteFilter = Builders<AttemptModel>.Filter.Eq("Tutorial", tutorial);
                AttemptsCollection.DeleteMany(deleteFilter);

                RefreshResultsDataGrid();


                var Tutorialollection = db.GetCollection<TutorialModel>("Tutorials");

                var deleteTutorialFilter = Builders<TutorialModel>.Filter.Eq("Name", tutorial);
                Tutorialollection.DeleteMany(deleteTutorialFilter);

                RefreshResultsDataGrid();
                RefreshResultsStackPanel();
            }

           
            
        }
    }
}
