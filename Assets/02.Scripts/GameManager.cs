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
    public int Day;
    //public int Day { get; private set; }
    private void Awake()
    {
        if(instance == null) { instance = this; }
        if(instance != this ) { 
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Day = 1;
    }

    public void ChangeDay()
    {
        Day++;
        Debug.Log($"{Day}일 차 입니다.");
    
        //if (Day == 8) PoolingManager.Instance.SetDinosWithReset();
        if (Day == 8) PoolingManager.Instance.AdjustDinoCount();
    }
}
