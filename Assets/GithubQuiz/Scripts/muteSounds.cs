using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class muteSounds : MonoBehaviour
{
    public AudioSource menuSound;
    public AudioSource levelSound;
    public Image mute;
    public Image unmute;
    public Button soundbtn;
    public void muted()
    {
        soundbtn = GameObject.Find("Volume").GetComponent<Button>();
        if (!PlayerPrefs.HasKey("mute"))
        {
            PlayerPrefs.SetInt("mute", 1);
        }
        if(PlayerPrefs.GetInt("mute")==1)
        {
            PlayerPrefs.SetInt("mute", 0);
            menuSound.mute=true;
            levelSound.mute = true;
            soundbtn.GetComponent<Image>().sprite = mute.sprite;
            //soundbtn.GetComponent<RectTransform>().sizeDelta = new Vector2(285, 189);
            soundbtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 285);
            soundbtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 189);
        }
        else
        {
            PlayerPrefs.SetInt("mute", 1);
            menuSound.mute = false;
            levelSound.mute = false;
            soundbtn.GetComponent<Image>().sprite = unmute.sprite;
            //soundbtn.GetComponent<RectTransform>().sizeDelta = new Vector2(285, 189);
            soundbtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100);
            soundbtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);
        }


    }
    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetInt("mute", 1);
        //soundbtn = GameObject.Find("Volume").GetComponent<Button>();
        //if (!PlayerPrefs.HasKey("mute"))
        //{
        //    PlayerPrefs.SetInt("mute", 1);
        //}
        //if (PlayerPrefs.GetInt("mute") == 1)
        //{
        //    PlayerPrefs.SetInt("mute", 0);
        //    menuSound.mute = true;
        //    levelSound.mute = true;
        //    soundbtn.GetComponent<Image>().sprite = mute.sprite;
        //    soundbtn.GetComponent<RectTransform>().sizeDelta = new Vector2(285, 189);
        //}
        //else
        //{
        //    PlayerPrefs.SetInt("mute", 1);
        //    menuSound.mute = false;
        //    levelSound.mute = false;
        //    soundbtn.GetComponent<Image>().sprite = unmute.sprite;
        //    soundbtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
