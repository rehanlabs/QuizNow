using System.Collections.Generic;
using UnityEngine;

namespace QuizGame.Models
{
    [CreateAssetMenu(fileName = "QuestionBank", menuName = "Quiz/Question Bank")]
    public class QuestionBank : ScriptableObject
    {
        public string topicId;
        public string displayName;
        public List<QuestionData> questions = new List<QuestionData>();

        public static List<QuizTopic> LoadFromJson(TextAsset json)
        {
            var wrapper = JsonUtility.FromJson<TopicWrapper>(json.text);
            return new List<QuizTopic>(wrapper.topics);
        }

        [System.Serializable]
        private class TopicWrapper
        {
            public QuizTopic[] topics;
        }
    }
}
