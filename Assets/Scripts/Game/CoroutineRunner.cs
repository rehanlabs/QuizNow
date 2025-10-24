using UnityEngine;

namespace QuizGame
{
    // Attach this to an empty GameObject in the bootstrap scene to run coroutines from non-Mono classes.
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        public static CoroutineRunner Instance 
        { 
            get 
            {
                if (_instance == null)
                {
                    var go = new GameObject("CoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            } 
        }
    }
}
