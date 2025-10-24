using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine;
using Newtonsoft.Json;
using QuizGame.Models;

namespace QuizGame.Services
{
    public class PlayFabQuizService : MonoBehaviour, IQuizService
    {
        public event Action<QuizTopic[]> OnTopicsLoaded;

        private QuizDatabase _database;

        public void LoadAllTopics()
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
                FunctionName = "getQuizTopics",
                GeneratePlayStreamEvent = true,
            }, OnCloudScriptSuccess, OnCloudScriptFailure);
        }

        // ✅ NEW: Expose topics so AuthService & BootstrapGame can use them
        public QuizTopic[] GetTopics()
        {
            return _database != null ? _database.topics : new QuizTopic[0];
        }

        private void OnCloudScriptSuccess(ExecuteCloudScriptResult result)
        {
            Debug.Log("Cloud Script call successful!");

            if (result.FunctionResult != null)
            {
                var json = JsonConvert.SerializeObject(result.FunctionResult);
                var wrapper = JsonConvert.DeserializeObject<QuizDatabaseWrapper>(json);

                if (wrapper != null && wrapper.data != null)
                {
                    _database = wrapper.data;
                }
                else
                {
                    Debug.LogError("Quiz topics data not found in Cloud Script result.");
                }
            }
            else
            {
                Debug.LogError("Cloud Script FunctionResult is null.");
            }

            if (_database != null && _database.topics != null)
            {
                OnTopicsLoaded?.Invoke(_database.topics);
            }
            else
            {
                Debug.LogError("Failed to parse quiz topics from Cloud Script result.");
            }
        }

        private void OnCloudScriptFailure(PlayFabError error)
        {
            Debug.LogError($"Cloud Script call failed: {error.GenerateErrorReport()}");
        }

        public QuizTopic GetTopic(string topicId)
        {
            if (_database == null)
            {
                Debug.LogError("Quiz database not loaded.");
                return null;
            }

            foreach (var topic in _database.topics)
            {
                if (topic.topicId == topicId)
                {
                    return topic;
                }
            }

            Debug.LogError($"Topic with ID '{topicId}' not found.");
            return null;
        }
    }

    public interface IQuizService
    {
        event Action<QuizTopic[]> OnTopicsLoaded;
        void LoadAllTopics();
        QuizTopic GetTopic(string topicId);
    }

    // Wrapper to match CloudScript return format: { "data": { ... } }
    public class QuizDatabaseWrapper
    {
        public QuizDatabase data { get; set; }
    }
}
