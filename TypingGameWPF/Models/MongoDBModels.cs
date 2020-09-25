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
        public int NoAtempts { get; set; }
        public int HighScore { get; set; }

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
