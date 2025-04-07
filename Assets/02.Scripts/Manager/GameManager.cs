
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool gameOver; //게임 오버 확인 함수 

    public bool GameOver 
    {
        get { return gameOver; }
        set 
        { 
            gameOver = value;

            if (gameOver == true)
            {
                Debug.Log("끝!");
                UiManagerInGame.Instance.SetGameOverUI();
                PoolingManager.Instance.ResetDinos();
                EnableRay();
            }
        }

    }
    [SerializeField ]private bool isPlay;
    public bool IsPlay
    {
        get { return isPlay; }
        set 
        { 
            isPlay = value;
            if (isPlay == true)
            {
                PoolingManager.Instance.SetDinos();
                DisableRay();
            }
            else
            {
                nextHungerTime = 0;
            }
        }
    }
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }
    public int Day;
    //게임 관리에 필요한 정보. 접근이 편하도록 GameManager에서 관리함.
    [Header("플레이어 정보")]
    PlayerControl player;
    //랩터인 플레이어의 팀원 수 (플레이어 포함)
    public int playerTeamSize;

    //배고픔 지수 : 0이 되면 Die
    public int maxHungerLevel = 5;
    public int currentHungerLevel;

    [Header("허기짐 체크")]
    public float hungerInterval = 60f;
    public float nextHungerTime = 0f;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this) Destroy(gameObject);
        player = GameObject.FindWithTag("Player").GetComponent<PlayerControl>();
        Day = 1;
        playerTeamSize = player.followers.Count + 1; //팀원 수 : 팔로워 + 본인
        currentHungerLevel = maxHungerLevel;
    }
    void Start()
    {

    }
    void Update()
    {
        if (GameManager.Instance.GameOver == true || isPlay == false) return;
        if (nextHungerTime == 0)
        {
            nextHungerTime = Time.time + hungerInterval;
            Debug.Log("초기 배고픔 시간 설정");
        }
        playerTeamSize = player.followers.Count + 1; //팀원 수 : 팔로워 + 본인
        CheckHungerInterval();
    }
    private void DisableRay()
    {
        player.gameObject.GetComponent<ControllerEvent>().DisableRayInteractor();
        player.GetComponent<ControllerEvent>().ToggleRayOff.Invoke();
    }
    private void EnableRay()
    {
        player.gameObject.GetComponent<ControllerEvent>().EnableRayInteractor();
        player.GetComponent<ControllerEvent>().ToggleRay.Invoke();
    }
    public void ChangeDay()
    {
        Debug.Log($"{Day}일 차 입니다.");
        Day++;
        if (Day == 8)
        {
            sceneManager.Instance.OnPlayCutScene(sceneManager.Instance.AnimationScene[1]); //***8일차 컷씬 재생 
            PoolingManager.Instance.AdjustDinoCount();
        }
        else if (Day == 15)
        {
            sceneManager.Instance.OnPlayCutScene(sceneManager.Instance.AnimationScene[2]);
            Debug.Log("게임 종료");
            GameOver = true;
        }
    }
    public void ReduceHungerLevel()
    {
        currentHungerLevel--;
        currentHungerLevel = Mathf.Clamp(currentHungerLevel, 0, maxHungerLevel);
        Debug.Log($"0.2일이 지나 현재 hungerLevel이 감소합니다. : {currentHungerLevel}");
        if (currentHungerLevel <= 0)
        {
            Debug.Log("굶어 죽었습니다.");
            GameOver = true;
            sceneManager.Instance.OnPlayCutScene(sceneManager.Instance.DeathScenes[1]);
            player.Die();
        }
    }

    void CheckHungerInterval()
    {
        if(Time.time >= nextHungerTime)
        {
            //허기짐 감소
            ReduceHungerLevel();
            //다음 허기짐 감소 시간 업데이트
            nextHungerTime = Time.time + hungerInterval;
        }
    }
}
