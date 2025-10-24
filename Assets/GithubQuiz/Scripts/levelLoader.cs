using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
public class levelLoader : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider slider;
    public TMP_Text myText;
    public void loadLevel()
    {

    }

    IEnumerator loadAsync()
    {
        //if (!PhotonNetwork.IsConnected)
        //{
        //    PhotonNetwork.ConnectUsingSettings();
        //}
        yield return new WaitForSeconds(1f);
        SceneManager.LoadSceneAsync("homeScreen");
        //AsyncOperation operation = SceneManager.LoadSceneAsync("homeScreen");
        //while (!operation.isDone)
        //{
        //    float progress = Mathf.Clamp01(operation.progress / 0.9f);
        //    slider.value = progress;
        //    myText.text = progress * 100f + "%";

        //}
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(loadAsync());
    }
}
