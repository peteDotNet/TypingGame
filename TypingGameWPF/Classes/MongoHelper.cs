using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TypingGameWPF.Models;

namespace TypingGameWPF
{
    public partial class MainWindow : Window
    {


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

        private void DeleteCurrentTutorial()
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


       

        private void DeleteAttemptsForCurrentTutorial()
        {
            var collection = db.GetCollection<TutorialModel>("Tutorials");
            var filter = Builders<TutorialModel>.Filter.Eq("Id", CurrentTutorial.Id);

            var updateHighScore = Builders<TutorialModel>.Update.Set("HighScore", 0);
            collection.UpdateOne(filter, updateHighScore);

            var updateAttempts = Builders<TutorialModel>.Update.Set("NoAttempts", 0);
            collection.UpdateOne(filter, updateAttempts);

            var updateFastestTime = Builders<TutorialModel>.Update.Set("FastestTime", 0);
            collection.UpdateOne(filter, updateFastestTime);

            var updateWPM = Builders<TutorialModel>.Update.Set("FastestWPM", 0);
            collection.UpdateOne(filter, updateWPM);

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
    }
}

