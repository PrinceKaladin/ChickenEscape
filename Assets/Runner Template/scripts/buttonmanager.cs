using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class buttonmanager : MonoBehaviour
{
    public Color on;
    public Color off;
    public bool correct;
    private void Awake()
    {
        if (correct)
        {
            GameObject audiox = GameObject.Find("Audioline");
            if (audiox.GetComponent<AudioSource>().volume == 1)
            {
                this.GetComponent<Image>().color = on;
            }
            else
            {
                this.GetComponent<Image>().color = off;
            }
        }

    }
    public void MusicOnOff()
    {
        GameObject audiox = GameObject.Find("Audioline");

        if (audiox.GetComponent<AudioSource>().volume == 1)
        {
            audiox.GetComponent<AudioSource>().volume = 0;
            this.GetComponent<Image>().color = off;
        }
        else
        {
            audiox.GetComponent<AudioSource>().volume = 1;
            this.GetComponent<Image>().color = on;

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
