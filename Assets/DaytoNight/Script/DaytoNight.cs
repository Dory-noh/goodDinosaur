using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class DaytoNight : MonoBehaviour
{

    [Header("낯과 밤 연출")]
    public Material Day;
    float ThicknessLevel = 0.5f;


    [Header("태양 연출")]
    public Light sun;
    public Transform sunPivot;

    [Header("시간")]
    public float daytime = 300f;
    float time = 0f; 
    float dayRatio;
    float normalizedRatio;


    [Header("구름 연출")]
    public Material cloudMaterial_L;
    public Material cloudMaterial_H;
    Material cloudMaterialInstance_L;
    Material cloudMaterialInstance_H;
    public Color dayCloudColor; // 낮에는 흰색 구름
    public Color nightCloudColor; // 밤 구름 색 

    //[Header("허기짐체크용")]
    //private float nextThreshold = 0.2f; // 다음 실행 시점
    //private float thresholdStep = 0.2f; // 간격



    void Awake()
    {
        Day.SetFloat("_AtmosphereThickness", ThicknessLevel);
        // 스카이박스 복사본 생성 후 적용
        RenderSettings.skybox = Day;

        // 구름 머티리얼 복사본 생성
        cloudMaterialInstance_L = new Material(cloudMaterial_L);
        cloudMaterialInstance_H = new Material(cloudMaterial_H);
        daytime = 300f;
    }


    void Update()
    {
        if (GameManager.Instance.GameOver || (GameManager.Instance.IsPlay == false))
        {
            time = 0;
            if (GameManager.Instance.isStop == true)
            {
                //Debug.Log("8일차 애니메이션 재생 중이므로 Day를 바꾸지 않음.");
                return;
            }
            GameManager.Instance.Day = 1;
            //Debug.Log($"Day를 1로 바꿉니다.");
            //nextThreshold = thresholdStep;
            return;
        }

        time += Time.deltaTime; //시간을 계속 Update
        dayRatio = time / daytime;

        //CheckDayRatioThreshold();
        normalizedRatio = dayRatio <= 0.3f ? dayRatio * 2 : (dayRatio - 0.5f) * 2;
        if (time > daytime) 
        { 
            time = 0f; 
            GameManager.Instance.ChangeDay();
            //nextThreshold = thresholdStep;
        } //하루가 지나면 시간초기화
        sunRise();
        Day2Night();
    }
    //void CheckDayRatioThreshold()
    //{
    //    if (dayRatio >= nextThreshold)
    //    {
    //        // ReduceHungerLevel 메서드를 호출한다.
    //        GameManager.Instance.ReduceHungerLevel();

    //        // 다음 임계값으로 업데이트
    //        nextThreshold += thresholdStep;

    //        // 만약 dayRatio가 1을 넘어갔다면 (하루가 끝났다면) 다음 날을 위해 초기화
    //        if (nextThreshold > 1f)
    //        {
    //            nextThreshold = thresholdStep;
    //        }
    //    }
    //}
    void sunRise() 
    {
        float sunAng = dayRatio * 360f ; //낮과 밤에 비율에 대한 태양의 각
        sunPivot.transform.rotation = Quaternion.Euler(sunAng - 90f, 90f, 0f);
        //x에 90도 보정을 준 이후는 정오(절반 180도)일 때 태양이 수직으로 있게 만들기 위함. 
        //y축은 태양이 비치는 방향 조절, 이는 연출에 맞게 변경 가능 
        // 앞뒤 기울기는 필요 없으므로 z축은 0f 
        //맵의 중앙에 설치된 sunPivot을 기준으로 360도 회전 

    }
    void Day2Night() //여기선 해만 관리 
    {

        if (dayRatio <= 0.25f)
        {
            // 아침 (0.0 ~ 0.25): 태양 떠오름, 대기 밝아짐
            sun.gameObject.SetActive(true);
            sun.intensity = Mathf.Lerp(0.1f, 0.8f, normalizedRatio * 4f); // 빠르게 밝아짐
            sun.color = Color.Lerp(Color.red, Color.yellow, normalizedRatio * 4f); // 일출 색상
            ThicknessLevel = Mathf.Lerp(0.5f, 0.8f, normalizedRatio * 4f); // 대기 강도 증가
            Day.SetFloat("_AtmosphereThickness", ThicknessLevel);
            UpdateCloudColor(nightCloudColor, dayCloudColor);
            //Debug.Log("아침");
        }
        else if (dayRatio > 0.25f && dayRatio <= 0.4f)
        {
            // 정오 (0.25 ~ 0.4): 태양 최고점, 대기 가장 밝음
            float ratio = (dayRatio - 0.25f) * 4f; // 정오 구간을 0~1로 매핑
            sun.intensity = Mathf.Lerp(0.8f, 1f, ratio); // 밝기 최고조
            sun.color = Color.Lerp(Color.yellow, Color.white, ratio); // 정오 색상
            ThicknessLevel = Mathf.Lerp(0.8f, 1.5f, ratio*2f); // 대기 강도 최대
            Day.SetFloat("_AtmosphereThickness", ThicknessLevel);
            //Debug.Log("정오");
        }
        else if (dayRatio > 0.4f && dayRatio <= 0.5f)
        {
            // 오후 (0.4 ~ 0.5): 태양 기울기 시작, 색 약간 붉어짐
            float ratio = (dayRatio - 0.4f) * 10f; // 오후 구간을 0~1로 매핑
            sun.intensity = Mathf.Lerp(1f, 0.7f, ratio); // 태양 밝기 감소
            sun.color = Color.Lerp(Color.white, Color.yellow, ratio); // 오후 느낌의 색
            ThicknessLevel = Mathf.Lerp(1.5f, 2f, ratio); // 대기 강도 살짝 증가
            Day.SetFloat("_AtmosphereThickness", ThicknessLevel);
            //Debug.Log("오후");
        }
        else if (dayRatio > 0.5f && dayRatio <= 0.75f)
        {
            // 저녁 (0.5 ~ 0.75): 태양 기울기, 색 붉어짐
            float ratio = (dayRatio - 0.5f) * 4f; // 저녁 구간을 0~1로 매핑
            sun.intensity = Mathf.Lerp(0.7f, 0.3f, ratio); // 밝기 줄어듦
            sun.color = Color.Lerp(Color.yellow, Color.red, ratio); // 붉어지는 태양 색
            ThicknessLevel = Mathf.Lerp(2f, 3f, ratio * 4f); // 대기 강도 최대
            Day.SetFloat("_AtmosphereThickness", ThicknessLevel);
            UpdateCloudColor(dayCloudColor, nightCloudColor);
            //Debug.Log("저녁");
        }
        else
        {
            // 밤 (0.75 ~ 1.0): 태양 사라짐, 대기 어두워짐
            float ratio = (dayRatio - 0.75f) * 4f; // 밤 구간을 0~1로 매핑
            sun.intensity = Mathf.Lerp(0.3f, 0.1f, ratio); // 어두워짐
            sun.color = Color.Lerp(Color.red, Color.black, ratio); // 태양 색 어두워짐
            ThicknessLevel = Mathf.Lerp(3f, 0.5f, ratio * 4f); // 대기 강도 감소
            Day.SetFloat("_AtmosphereThickness", ThicknessLevel);
            //Debug.Log("밤");
        }
        RenderSettings.skybox = Day;
        DynamicGI.UpdateEnvironment();

    }

    void UpdateCloudColor(Color a, Color b)
    {
        float dayRatio = time / daytime;

        // 구름 색상을 낮에는 흰색, 밤에는 회색으로 변경
        Color currentCloudColor = Color.Lerp(a, b, Mathf.Sin(dayRatio * Mathf.PI));

        // 구름 머티리얼 복사본에 색상 적용
        cloudMaterialInstance_L.SetColor("_BaseColor", currentCloudColor);
        cloudMaterialInstance_H.SetColor("_BaseColor", currentCloudColor);

        // 변경된 머티리얼 적용
        cloudMaterial_L = cloudMaterialInstance_L;
        cloudMaterial_H = cloudMaterialInstance_H;
    }
}

