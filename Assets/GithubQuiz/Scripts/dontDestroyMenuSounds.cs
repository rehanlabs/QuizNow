using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dontDestroyMenuSounds : MonoBehaviour
{
    // Start is called before the first frame update
    private static dontDestroyMenuSounds playerInstance;
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
