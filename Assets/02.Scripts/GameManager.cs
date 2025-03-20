using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }
    private bool isGamveOver;
    private void Awake()
    {
        if(instance == null) { instance = this; }
        if(instance != this ) { 
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

    }
}
