using System;
using Unity.VisualScripting;

namespace QuizGame.Models
{
    [Serializable]
    public class QuestionData
    {
        public string prompt;
        public string[] options;
        public int correctIndex;
        public int difficulty = 1; // 1-3
    }

    [Serializable]
    public class QuizTopic
    {
        public string topicId;
        public string topicName;
        public QuestionData[] questions;
    }

    [Serializable]
    public class QuizDatabase
    {
        public QuizTopic[] topics;
    }
}
