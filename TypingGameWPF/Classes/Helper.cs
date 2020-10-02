using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TypingGameWPF.Models;

namespace TypingGameWPF
{
    public partial class MainWindow : Window
    {
        private void SelectCurrentTutorial(object sender, RoutedEventArgs e)
        {
            string tutorialName = (sender as Button).Content.ToString();

            tutorialsList = LoadTutorials<TutorialModel>();
            var t = tutorialsList.Where(a => a.Name == tutorialName).First();

            CurrentTutorial = t;
            CustomTextBox.Text = "";
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

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {

            MetricsTextbox.Foreground = AntiqueWhiteBrush;
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
                MetricsTextbox.Foreground = GreenBrush;

                var id = CurrentTutorial.Id;
                tutorialsList = UpdateTutorials<TutorialModel>();
                foreach (TutorialModel tm in tutorialsList)
                {
                    if (tm.Id == id)
                        CurrentTutorial = tm;
                }
                RetryTutorial();
            }
        }

        private void SubmitCustomTutorial()
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

        private void SubmitAttempt()
        {
            AttemptModel am = new AttemptModel();
            am.Date = DateTime.Now.Date;
            am.Accuracy = Accuracy;
            am.Time = Time;


            am.TimeString = Time.ToString();
            am.Tutorial = CurrentTutorial.Name;
            am.WPM = WPM;
            am.Score = score;
            InsertAttempt<AttemptModel>(am);
            DisplayTutorialMetrics();
        }

        private void RetryTutorial()
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

        private void GenerateRandomText()
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

    }
}
