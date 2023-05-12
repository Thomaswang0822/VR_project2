using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource src;
    public AudioClip speed, countdown, bingo, crash;

    public void onDriving() {
        src.clip = speed;
        src.Play();
    }

    public void onReady() {
        src.clip = countdown;
        src.Play();
    }

    public void onPass() {
        src.clip = bingo;
        src.Play();
    }

    public void onCrash() {
        src.clip = speed;
        src.Play();
    }
}
