using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManagerInGame : MonoBehaviour
{
    public static UiManagerInGame Instance;
    [Header("인게임UI창")]
    public GameObject MenuInGame;
    public GameObject GameOverUi;

    [Header("타이틀 UI창")]
    public GameObject Title;
    public GameObject Menu;
    public GameObject Dictionary;
    public GameObject GameClear;

    [Header("UI 요소들")]
    public Slider bgmSlider;
    public Slider BgmSlider;
    public Slider effSlider;
    public AudioSource BGMSource; //Map에 넣어놓을 audiosource
    public AudioSource[] EffSource; //Player에 들어갈 audiosource 
    public TMP_Text SurvivalDay;
    public TMP_Text RaptorCrew;
    public Image HungerMeter;
    public GameObject PlayerHpBar;

    [Header("GameMode")]
    public GameObject Player;
    public GameObject LobbyMode;
    public GameObject GameMode;
    public Transform LobbyTr;
    public Transform RespawnTr;

    [SerializeField] bool isPause;

    public InputActionProperty BtnAct;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (GameManager.Instance.GameOver == false)
        {
            GameOverUi.SetActive(GameManager.Instance.GameOver);
        }
        BgmSlider.value = 0.5f;
        bgmSlider.value = 0.5f;
        effSlider.value = 0.5f;
        AdjustBGM();
        AdjustEff();
        StartingSetup();
    }

    private void StartingSetup()
    {

        LobbyMode.SetActive(true);
        GameMode.SetActive(false);
        Title.SetActive(true);
        Dictionary.SetActive(false);
        MenuInGame.SetActive(false);
        Menu.SetActive(false);
        PlayerHpBar.SetActive(false);
        GameClear.SetActive(false);
        isPause = false;
    }

    void Start()
    {

    }
    public void Lobby()
    {
        if (GameOverUi != null && MenuInGame != null)
        {
            Invoke("LoadLobby", 2.5f); // 2초 후 LoadLobby 메서드 실행
        }
    }
    void LoadLobby() 
    {
        if (GameManager.Instance.IsPlay != true) return;
        GameManager.Instance.IsPlay = false;
        GameManager.Instance.GameOver = false;
        GameMode.SetActive(GameManager.Instance.IsPlay);
        LobbyMode.SetActive(!GameManager.Instance.IsPlay);
        Player.transform.position = LobbyTr.position;
        Player.transform.rotation = LobbyTr.rotation;
        PlayerHpBar.SetActive(false);
        GameClear.SetActive(false);

    }
    public void Quit()
    {
        if (GameOverUi != null && MenuInGame != null)
        {
            Invoke("QuitGame", 2.5f); // 2초 후 QuitGame 메서드 실행
        }
    }

    private void QuitGame()
    {
        GameManager.Instance.IsPlay = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void startGame() 
    {
        if (GameOverUi != null && MenuInGame != null)
        {
            Invoke("StartGame", 2f); // 2초 후 StartGame 메서드 실행
        }
    }
    public void StartGame() 
    {
        if (GameManager.Instance.IsPlay == true) return;
        GameManager.Instance.IsPlay = true;
        GameManager.Instance.GameOver = false;
        PlayerControl player = Player.GetComponent<PlayerControl>();
        player.isDie = false;
        player.hp = player.MaxHp;
        player.hpImg.fillAmount = (float)player.hp / player.MaxHp;
        player.GetComponent<ControllerEvent>().DisableRayInteractor();
        GameManager.Instance.currentHungerLevel = GameManager.Instance.maxHungerLevel;
        GameMode.SetActive(GameManager.Instance.IsPlay);
        LobbyMode.SetActive(!GameManager.Instance.IsPlay);
        GameOverUi.SetActive(false);
        Player.transform.position = RespawnTr.position;
        Player.transform.rotation = RespawnTr.rotation;
        PlayerHpBar.SetActive(true);
        GameClear.SetActive(false);
        player.followers.Clear();
    }

    void Update()
    {
        if (GameManager.Instance.IsPlay)
        {
            if (BtnAct.action.WasPressedThisFrame()) //키를 입력 시에 옵션이 나옴 
            {
                MenuInGame.SetActive(!MenuInGame.activeSelf);
                //Time.timeScale = (MenuInGame.activeSelf) ? 0.0f : 1.0f;
            }
        }

        SurvivalDay.text = $"Day - {GameManager.Instance.Day.ToString("00")}";
        RaptorCrew.text = $"Member : {GameManager.Instance.playerTeamSize.ToString("00")}";
        HungerMetershow();
        AdjustBGM();
        AdjustEff();
        if(!isPause) 
            Time.timeScale = 1.0f;
    }

    public void SetGameOverUI()
    {
        if (GameManager.Instance.Day == 15)        
            GameClear.SetActive(true);
        else
            GameOverUi.SetActive(true);


        PlayerHpBar.SetActive(false);
        MenuInGame.SetActive(false);
        //GameManager.Instance.IsPlay = false;
    }

    void AdjustVolume(AudioSource source, Slider slider) //볼륨조절 
    {
        if (source != null && slider != null)
        {
            source.volume = slider.value * (source.gameObject.CompareTag("dinoEffSource")?0.4f:1f);
        }
    }
    public void AdjustBGM() //BGM 조절 
    {
        AdjustVolume(BGMSource, bgmSlider);
        AdjustVolume(BGMSource, BgmSlider);
    }

    public void AdjustEff() //Effect 조절
    {
        GameObject[] effObj = GameObject.FindGameObjectsWithTag("dinoEffSource");

        foreach (var source in EffSource) AdjustVolume(source, effSlider);
        foreach (var source in effObj) AdjustVolume(source.transform.GetComponent<AudioSource>(), effSlider);
    }
    public void HungerMetershow() 
    {
        HungerMeter.fillAmount = (float)GameManager.Instance.currentHungerLevel / GameManager.Instance.maxHungerLevel;
    }
    public void Pause() 
    {
        isPause = !isPause;
        if (isPause) 
            Time.timeScale = 0.0f;     
    }
}
    #region SceneLoad방식

    //private void StartGame() 
    //{
    //    SceneManager.UnloadSceneAsync("Lobby");
    //    SceneManager.LoadScene("GameScene", LoadSceneMode.Additive);
    //    GameManager._Instance.GameOver = false;
    //}
    //private void LoadLobby()
    //{
    //    SceneManager.UnloadSceneAsync("GameScene");
    //    SceneManager.LoadScene("Lobby", LoadSceneMode.Additive);
    //    GameManager._Instance.GameOver = false;
    //}
    //public void LoadMapScene()
    //{
    //    SceneManager.LoadScene("MapScene", LoadSceneMode.Additive); // 맵 씬을 Additive 모드로 로드
    //}
    #endregion

