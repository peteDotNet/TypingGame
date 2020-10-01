using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TypingGameWPF.Models
{
    public partial class MainWindow : Window
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Speed { get; set; }
        public string Accuracy { get; set; }

    }

    public class TutorialModel
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Paragraph { get; set; }
        public int NoAttempts { get; set; }

        public int HighScore { get; set; }


        public int FastestWPM { get; set; }
        public double FastestTime { get; set; }

        private string _numberOfAttempts;


        public string NumberOfAttempts
        {
            get { return $"Attempts: {NoAttempts}"; }
            private set { _numberOfAttempts = value; }
        }

        private string _highScoreString;

        public string HighScoreString
        {
            get { return $"High Score: {HighScore}"; }
            private set { _highScoreString = value; }
        }

        private string _fastestWPMString;

        public string FastestWPMString
        {
            get { return $"Top WPM: {FastestWPM}"; }
            private set { _fastestWPMString = value; }
        }

        private string _fastestTimeString;

        public string FastestTimeString
        {
            get { return $"Best Time: {FastestTime}(s)"; }
            private set { _fastestTimeString = value; }
        }






    }

    public class AttemptModel
    {
        [BsonId]
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public double WPM { get; set; }
        public double Time { get; set; }
        public string TimeString { get; set; }
        public double Accuracy { get; set; }
        public double Score { get; set; }
        public string Tutorial { get; set; }
    }
}

