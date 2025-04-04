using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class sceneManager : MonoBehaviour
{
    public static sceneManager Instance;  // 싱글톤 

    public PlayableDirector timeline;  // 애니메이션용 타임라인
    private bool isPlayingCutscene = false;  // 중복 실행 방지
    [Header("Death Scene 불러오기용")]
    public string[] DeathScenes = { "WaterDeathScene", "StarveDeathScene", "EatenDeathScene" };
    GameObject AnimationCam;
    GameObject Player;
    //public Transform targetPosition;
    void Awake()
    {
        Player = GameObject.Find("Player");
        AnimationCam = GameObject.Find("AnimationCam");
        // 싱글톤 패턴 (필요하면 사용)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //씬이 모두 로드되었을 때 넘어가게 한다
    public void OnPlayerDie(string SceneName)
    {

        if (isPlayingCutscene) return; // 중복 실행 방지

        isPlayingCutscene = true; // 컷신 실행 중

        Debug.Log($"{SceneName} 재생 시작");
        Player.transform.GetComponent<ControllerEvent>().DisableRayInteractor();
        Player.SetActive(false);
        // 애니메이션용 씬 추가 로드
        SceneManager.LoadScene(SceneName, LoadSceneMode.Additive);
        

        // 일정 시간 후 타임라인 실행 (혹은 씬 로드 후 실행)
        StartCoroutine(PlayCutscene(SceneName));
    }

    IEnumerator PlayCutscene(string Scene)
    {
        
        yield return new WaitForSeconds(1f); // 씬 로딩 기다리기 (필요 시 조절)
        timeline = GameObject.Find("TimeLine").GetComponent<PlayableDirector>();
        GameObject timelineRoot = GameObject.Find("TimeLineRoot"); // 타임라인 오브젝트 이름 확인 후 변경!
        //targetPosition = timelineRoot.transform;
        if(timelineRoot == null)
        {
            Debug.Log("타임라인오브젝트없음");
        }
        //if (timelineRoot != null)
        //{
        //    timelineRoot.transform.position = targetPosition.transform.position;
        //    timelineRoot.transform.rotation = targetPosition.transform.rotation;
        //    Debug.Log($"타임라인 위치 조정 완료: {targetPosition}");
        //}
        // 타임라인 재생
        if (timeline != null && isPlayingCutscene)
        {
            timeline.Play();
            Debug.Log("타임라인 실행 중...");
            yield return new WaitForSeconds((float)timeline.duration); // 타임라인이 끝날 때까지 대기
            isPlayingCutscene = false; // 컷신 종료
        }
        // 컷신 종료 후 원래 게임 상태 복구
         Debug.Log("컷신 종료, 애니메이션 씬 언로드...");
         BackToGame(Scene);

    }

    private void BackToGame(string Scene)
    {
        SceneManager.UnloadSceneAsync(Scene);
        Player.SetActive(true);
        //InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, new List<InputDevice>());
        //StartCoroutine(ControlReset());
    }
    IEnumerator ControlReset()
    {
        Player.SetActive(true);
        yield return null; // 프레임 대기
        Debug.Log("리셋 시작");

        // XR 시스템 완전히 종료
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();



        // XR 시스템 다시 시작
        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
        XRGeneralSettings.Instance.Manager.StartSubsystems();

        Debug.Log("시스템 리셋 완료");
    }
}
