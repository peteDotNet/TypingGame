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
using System.Collections.ObjectModel;
using System.Net;

namespace TypingGameWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        IMongoDatabase db;

        private List<TutorialModel> _tutorialsList;


        public List<TutorialModel> tutorialsList
        {
            get { return _tutorialsList; }
            set
            {
                if (_tutorialsList != value)
                {
                    _tutorialsList = value;
                    OnPropertyChanged("tutorialsList");

                }
            }
        }

        private string _nextTutorial;

        public string NextTutorial
        {
            get { return _nextTutorial; }
            set
            {
                _nextTutorial = value;
                OnPropertyChanged("NextTutorial");
            }
        }


        private TutorialModel _currentTutorial;
        public TutorialModel CurrentTutorial
        {
            get { return _currentTutorial; }
            set
            {
                if (_currentTutorial != value)
                {
                   
                    _currentTutorial = value;
                    OnPropertyChanged("CurrentTutorial");
                    DisplayTutorialMetrics();

                    ResetTutorial();
                }
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public Stopwatch stopWatch = new Stopwatch();
        int CorrectCharacters = 0;
        int errors = 0;
        double Accuracy;
        double Time;
        double WPM;
        double score;
        BrushConverter converter = new System.Windows.Media.BrushConverter();
        Brush w;
        Brush g;
        public MainWindow()
        {
            w = (Brush)converter.ConvertFromString("#FFFFE5D9");
            g = (Brush)converter.ConvertFromString("#71BD85");
            DataContext = this;

            var client = new MongoClient();
            db = client.GetDatabase("TypingMetrics");


            InitializeComponent();

            try
            {
                tutorialsList = LoadTutorials<TutorialModel>();

                CurrentTutorial = tutorialsList.First();


                int i = tutorialsList.Count() + 1;
                NextTutorial = $"Tutorial: {i}";
            }
            catch { };
   

        }



        private void SelectCurrentTutorial(object sender, RoutedEventArgs e)
        {
            string tutorialName = (sender as Button).Content.ToString();

            tutorialsList = LoadTutorials<TutorialModel>();
            var t = tutorialsList.Where(a => a.Name == tutorialName).First();


            CurrentTutorial = t;
            CustomTextBox.Text = "";

        }



        void DisplayTutorialMetrics(object sender, EventArgs e)
        {
            string tutorialName = (sender as Button).Content.ToString();

            var attempts = LoadAttempts<AttemptModel>();
            var results = attempts;

            if (tutorialName != "Show All")
            {
                results = attempts.Where(a => a.Tutorial == tutorialName).ToList();
            }

            dataGrid1.ItemsSource = results;

            var tutorial = LoadTutorials<TutorialModel>();
            var t = tutorial.Where(a => a.Name == tutorialName);


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

           
            if (CurrentTutorial != null)
            {
                var results = LoadAttempts<AttemptModel>();

                var attempts = results.Where(a => a.Tutorial == CurrentTutorial.Name).ToList();


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

        private List<TutorialModel> UpdateTutorials<TutorialModel>()
        {
            var collection = db.GetCollection<TutorialModel>("Tutorials");
            var filter = Builders<TutorialModel>.Filter.Eq("Id", CurrentTutorial.Id);


            if (score > CurrentTutorial.HighScore)
            {
                var updateHighScore = Builders<TutorialModel>.Update.Set("HighScore", score);
                collection.UpdateOne(filter, updateHighScore);
            }

            if (WPM > CurrentTutorial.FastestWPM)
            {
                var updateFastestWPM = Builders<TutorialModel>.Update.Set("FastestWPM", WPM);
                collection.UpdateOne(filter, updateFastestWPM);
            }
            if (Time > CurrentTutorial.FastestTime)
            {
                var updateFastestTime = Builders<TutorialModel>.Update.Set("FastestTime", Time);
                collection.UpdateOne(filter, updateFastestTime);
            }


            var updateAttempts = Builders<TutorialModel>.Update.Set("NoAttempts", CurrentTutorial.NoAttempts + 1);
            collection.UpdateOne(filter, updateAttempts);


            return collection.Find(new BsonDocument()).ToList();
        }

      

        private List<AttemptModel> LoadAttempts<AttemptModel>()
        {
            var collection = db.GetCollection<AttemptModel>("Attempts");
            return collection.Find(new BsonDocument()).ToList();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var t = LoadTutorials<TutorialModel>();
            int i = t.Count() + 1;

            var NewTutorialName = $"Tutorial: {i}";
            NextTutorial = $"Tutorial: {i + 1}";


            TutorialModel tm = new TutorialModel();
            tm.Paragraph = CustomTextBox.Text;
            tm.Name = NewTutorialName;
            tm.NoAttempts = 0;
            tm.HighScore = 0;
            tm.FastestWPM = 0;

            InsertTutorial(tm);
            tutorialsList = LoadTutorials<TutorialModel>();
            CurrentTutorial = tutorialsList.First();
            CustomTextBox.Text = "";

        }



        int ij = 0;
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {

            MetricsTextbox.Foreground = w;
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
            var FullTime = stopWatch.Elapsed.TotalMilliseconds;
            Time = Math.Round(FullTime / 1000, 2);
            score = Math.Round(Accuracy * WPM, 0);
            MetricsTextbox.Text = $"Score: {score}\nWPM: {WPM}\nAccuracy: {Accuracy}\nTime: {Time}";
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
                MetricsTextbox.Foreground = g;

                var id = CurrentTutorial.Id;
                tutorialsList = UpdateTutorials<TutorialModel>();
                foreach (TutorialModel tm in tutorialsList)
                {
                    if (tm.Id == id)
                        CurrentTutorial = tm;
                }

                ResetTutorial();
            }
        }


        private void SubmitAttempt()
        {
            AttemptModel am = new AttemptModel();
            am.Date = DateTime.Now.Date;
            am.Accuracy = Accuracy;
            am.Time = Time;
            //var timeInMilliseconds = Math.Round(Time / 1000, 2);


            am.TimeString = Time.ToString();
            am.Tutorial = CurrentTutorial.Name;
            am.WPM = WPM;
            am.Score = score;
            InsertAttempt<AttemptModel>(am);
            DisplayTutorialMetrics();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ResetTutorial();

        }

        private void ResetTutorial()
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

            var collection = db.GetCollection<TutorialModel>("Tutorials");
            var filter = Builders<TutorialModel>.Filter.Eq("Id", CurrentTutorial.Id);

            var updateHighScore = Builders<TutorialModel>.Update.Set("HighScore", 0);
            collection.UpdateOne(filter, updateHighScore);

            var updateAttempts = Builders<TutorialModel>.Update.Set("NoAttempts", 0);
            collection.UpdateOne(filter, updateAttempts);

            var AttemptsCollection = db.GetCollection<AttemptModel>("Attempts");

            var deleteFilter = Builders<AttemptModel>.Filter.Eq("Tutorial", CurrentTutorial.Name);
            AttemptsCollection.DeleteMany(deleteFilter);

            var id = CurrentTutorial.Id;
            tutorialsList = LoadTutorials<TutorialModel>();

            foreach (TutorialModel tm in tutorialsList)
            {
                if (tm.Id == id)
                    CurrentTutorial = tm;
            }

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            string tutorial = CurrentTutorial.Name;

            var AttemptsCollection = db.GetCollection<AttemptModel>("Attempts");
            var deleteFilter = Builders<AttemptModel>.Filter.Eq("Tutorial", tutorial);
            AttemptsCollection.DeleteMany(deleteFilter);

            var Tutorialollection = db.GetCollection<TutorialModel>("Tutorials");

            var deleteTutorialFilter = Builders<TutorialModel>.Filter.Eq("Name", tutorial);
            Tutorialollection.DeleteMany(deleteTutorialFilter);

            tutorialsList = LoadTutorials<TutorialModel>();

            try
            {
                CurrentTutorial = tutorialsList.Last();

                int i = tutorialsList.Count();
                NextTutorial = $"Tutorial: {i + 1}";
            }
            catch { }
    
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var json = new WebClient().DownloadString("https://random-word-api.herokuapp.com/word?number=15");
            var djson = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(json);

            StringBuilder sb = new StringBuilder();

            foreach (string s in djson)
            {
                sb.Append(s);
                sb.Append(" ");
            }
            sb.Append(".");

            string ss = sb.ToString();

            CustomTextBox.Text = sb.ToString();
        }
    }
}
