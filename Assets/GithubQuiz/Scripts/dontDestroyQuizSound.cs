using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dontDestroyQuizSound : MonoBehaviour
{
    private static dontDestroyQuizSound playerInstance;
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        if (playerInstance == null)
        {
            playerInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
