using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class manageSounds : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource menuSound;
    public AudioSource levelSound;
    // Start is called before the first frame update
    void Start()
    {
        //menuSound.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            if (!levelSound.isPlaying)
            {
                levelSound.Play();
                //menuSound.Stop();
            }
        }
        else
        {
            
                //menuSound.Play();
                levelSound.Stop();
            
        }
    }
}
