using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    private AudioSource target;

    private void Awake()
    {
        target = GetComponent<AudioSource>();
    }

    public void Call()
    {
        if(target.isPlaying == false) target.Play();
    }
}
