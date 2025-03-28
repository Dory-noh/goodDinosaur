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
    
    private bool gamveOver;

    public bool GamveOver
    {
        get; set;
    }
    
    public int Day;
    public bool isPlay;

    public bool IsPlay
    {
        get { return isPlay; }
        set { 
            isPlay = value;
            //플레이 모드가 되면 공룡들을 n일 차의 공룡 수에 맞게 활성화한다.
            if (isPlay == true) PoolingManager.Instance.SetDinos();
            //플레이 모드가 해제되면 공룡들을 모두 비활성화한다.
            else PoolingManager.Instance.ResetDinos();
        }
    }

    //게임 관리에 필요한 정보. 접근이 편하도록 GameManager에서 관리함.
    [Header("플레이어 정보")]
    PlayerControl player;
    //랩터인 플레이어의 팀원 수 (플레이어 포함)
    public int playerTeamSize;

    //배고픔 지수 : 0이 되면 Die
    public int maxHungerLevel = 5;
    public int currentHungerLevel;
    
    private void Awake()
    {
        if(instance == null) { instance = this; }
        if(instance != this ) { 
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        //플레이어 정보 초기화
        player = GameObject.FindWithTag("Player").GetComponent<PlayerControl>();
        Day = 1;
        playerTeamSize = player.followers.Count + 1; //팀원 수 : 팔로워 + 본인
        currentHungerLevel = maxHungerLevel;
    }

    public void ChangeDay()
    {
        Day++;
        Debug.Log($"{Day}일 차 입니다.");
    
        //if (Day == 8) PoolingManager.Instance.SetDinosWithReset();
        if (Day == 8) PoolingManager.Instance.AdjustDinoCount();
        else if(Day == 15)
        {
            Debug.Log("게임 종료");
            gamveOver = true;
        }
    }

    public void ReduceHungerLevel()
    {
        currentHungerLevel--;
        currentHungerLevel = Mathf.Clamp(currentHungerLevel, 0, maxHungerLevel);
        Debug.Log($"0.2일이 지나 현재 hungerLevel이 감소합니다. : {currentHungerLevel}");
        if( currentHungerLevel <= 0)
        {
            Debug.Log("굶어 죽었습니다.");
            gamveOver = true;
            player.Die();
        }
    }
}
