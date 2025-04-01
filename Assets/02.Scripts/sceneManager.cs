using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEditor.TextCore.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour
{
    public static sceneManager Instance;  // 싱글톤 

    public PlayableDirector timeline;  // 애니메이션용 타임라인
    private bool isPlayingCutscene = false;  // 중복 실행 방지
    private string timelineSceneName = "WaterDeathScene"; // 타임라인 씬 이름
    GameObject AnimationCam;
    GameObject Player; 
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

    // 물에 빠졌을 때 호출할 함수
    public void OnPlayerFallIntoWater()
    {
        if (isPlayingCutscene) return; // 중복 실행 방지

        isPlayingCutscene = true; // 컷신 실행 중

        Debug.Log("물에 빠짐! 타임라인 씬 로드 시작...");

        Player.SetActive(false);
        // 애니메이션용 씬 추가 로드
        //SceneManager.LoadScene(timelineSceneName, LoadSceneMode.Additive);
        SceneManager.LoadScene(timelineSceneName);

        // 일정 시간 후 타임라인 실행 (혹은 씬 로드 후 실행)
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        yield return new WaitForSeconds(1f); // 씬 로딩 기다리기 (필요 시 조절)

        // 타임라인 재생
        if (timeline != null && isPlayingCutscene)
        {
            timeline.Play();
            Debug.Log("타임라인 실행 중...");
            yield return new WaitForSeconds(3f); // 타임라인이 끝날 때까지 대기
            isPlayingCutscene = false; // 컷신 종료
        }
        else
        {
            // 컷신 종료 후 원래 게임 상태 복구
            Debug.Log("컷신 종료, 애니메이션 씬 언로드...");

            Invoke("BackToGame", 6f);
        }
    }

    private void BackToGame()
    {
        SceneManager.UnloadSceneAsync(timelineSceneName);
        Player.SetActive(true);
        GameManager.Instance.GameOver = true;
        GameManager.Instance.IsPlay = false;
    }

}
