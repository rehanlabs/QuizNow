using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashLoader : MonoBehaviour
{
    [SerializeField] private float delay = 3f; // time in seconds before loading next scene

    private void Start()
    {
        Invoke("LoadNextScene", delay);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(1); // loads scene at index 1
    }
}
