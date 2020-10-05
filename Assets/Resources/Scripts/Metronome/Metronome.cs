using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Metronome : MonoBehaviour
{
    // Start is called before the first frame update

    public Image thisImage;

    private void Start()
    {
        MusicManager.Instance.RegisterBeatEvent(HandleBeat);
    }

    private void OnDestroy()
    {
        MusicManager.Instance.UnregisterBeatEvent(HandleBeat);
    }

    public void HandleBeat()
    {
        if (MusicManager.Instance.Beat % 2 == 0)
        {
           Image currentImage =  this.gameObject.GetComponent<Image>();
            Color newColor = Color.green;
            newColor.a = 1;
            currentImage.color = newColor;
        }
        else
        {
            Image currentImage = this.gameObject.GetComponent<Image>();
            Color newColor = Color.red;
            newColor.a = 1;
            currentImage.color = newColor;
        }
    }

    }
