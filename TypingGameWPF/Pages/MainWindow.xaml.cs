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
        IMongoDatabase db = new MongoClient().GetDatabase("TypingMetrics");

        public Stopwatch stopWatch = new Stopwatch();
        int CorrectCharacters = 0;
        int errors = 0;
        double Accuracy;
        double Time;
        double WPM;
        double score;
        int ij = 0;
   
        Brush AntiqueWhiteBrush =  (Brush)new BrushConverter().ConvertFromString("#FFFFE5D9");
        Brush GreenBrush = (Brush)new BrushConverter().ConvertFromString("#71BD85");

        private List<TutorialModel> _tutorialsList;

        private string _nextTutorial;

        private TutorialModel _currentTutorial;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public MainWindow()
        {

            DataContext = this;

            InitializeComponent();

            tutorialsList = LoadTutorials<TutorialModel>();

            int tutorialCount = tutorialsList.Count();

            if (tutorialCount > 0)
                CurrentTutorial = tutorialsList.First();

            NextTutorial = $"Tutorial: {tutorialCount + 1}";

        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Submit a custom tutorial to mongoDB
            SubmitCustomTutorial();
        }

       

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Sets timer to zero and sets all metrics to 0 for attempt.
            RetryTutorial();
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //Deletes all attempts for the current tutorial
            DeleteAttemptsForCurrentTutorial();
        }

      

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Deletes current tutorial and clears all attempts
            DeleteCurrentTutorial();
        }

       


        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //Generate random text for a custom tutorial using random word gernerator api
            GenerateRandomText();
        }

       
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



        public string NextTutorial
        {
            get { return _nextTutorial; }
            set
            {
                _nextTutorial = value;
                OnPropertyChanged("NextTutorial");
            }
        }


        public TutorialModel CurrentTutorial
        {
            get { return _currentTutorial; }
            set
            {
                if (_currentTutorial != value)
                {

                    _currentTutorial = value;
                    OnPropertyChanged("CurrentTutorial");

                    //Refresh data displayed on datagrid and line chart
                    DisplayTutorialMetrics();
                    RetryTutorial();
                }
            }
        }
    }
}
