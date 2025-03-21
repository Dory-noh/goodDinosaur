using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class DaytoNight : MonoBehaviour
{

    [Header("낯과 밤 연출")]
    public Material Day;
    public Material Night;
    Material skyboxInstance; // 스카이박스 복사본


    [Header("태양 연출")]
    public Light sun;
    public Light night;
    public Transform sunPivot;

    [Header("시간")]
    public float daytime = 60f;
    float time;
    float dayRatio;


    [Header("구름 연출")]
    public Material cloudMaterial_L;
    public Material cloudMaterial_H;
    Material cloudMaterialInstance_L;
    Material cloudMaterialInstance_H;
    public Color dayCloudColor; // 낮에는 흰색 구름
    public Color nightCloudColor; // 밤 구름 색 




    void Awake()
    {
        dayRatio = time / daytime;
        time = 0f;
        // 스카이박스 복사본 생성 후 적용
        skyboxInstance = new Material(Day);
        RenderSettings.skybox = skyboxInstance;

        // 구름 머티리얼 복사본 생성
        cloudMaterialInstance_L = new Material(cloudMaterial_L);
        cloudMaterialInstance_H = new Material(cloudMaterial_H);
    }


    void Update()
    {
        time +=Time.deltaTime; //시간을 계속 업데이트 
        if (time > daytime) { time = 0f; } //하루가 지나면 시간초기화
        sunRise();
        Day2Night();
    }
    void sunRise() 
    {
        dayRatio = time / daytime;
        float sunAng = dayRatio * 360f; //낮과 밤에 비율에 대한 태양의 각
        sunPivot.transform.rotation = Quaternion.Euler(sunAng - 90f, 90f, 0f);
        //x에 90도 보정을 준 이후는 정오(절반 180도)일 때 태양이 수직으로 있게 만들기 위함. 
        //y축은 태양이 비치는 방향 조절, 이는 연출에 맞게 변경 가능 
        // 앞뒤 기울기는 필요 없으므로 z축은 0f 
        //맵의 중앙에 설치된 sunPivot을 기준으로 360도 회전 

    }
    void Day2Night() 
    {
        dayRatio = time / daytime;
        if (dayRatio > 0.2 && dayRatio < 0.7)
        {
            float num = dayRatio * 2;
            //Debug.Log("낮이에요.");
            sun.intensity = Mathf.Lerp(0.1f, 1f, dayRatio * 2); //해가 뜨는 연출 
            //태양의 빛 밀도가 0.1에서 1로, 낯비율의 2배 곱한 값만큼의 속도로 바뀐다.
            sun.color = Color.Lerp(Color.red, Color.white, dayRatio * 2);//일출 색 변화 
            night.intensity = Mathf.Lerp(3, 0, dayRatio *2);
            UpdateCloudColor(dayCloudColor, nightCloudColor);
            skyChange(Day, Night, num);
        }
        else
        {
            float num = (dayRatio - 0.5f) * 2;
            //Debug.Log("밤이 되었습니다.");
            sun.intensity = Mathf.Lerp(1f, 0f, (dayRatio-0.5f) * 2); //해가 뜨는 연출 
            //태양의 빛 밀도가 1에서 0.1로, 밤비율의 2배 곱한 값만큼의 속도로 바뀐다.
            sun.color = Color.Lerp(Color.white, Color.red, (dayRatio - 0.5f) * 2); //일출 색 변화 
            night.intensity = Mathf.Lerp(0, 3, dayRatio * 2);
            UpdateCloudColor(nightCloudColor, dayCloudColor);
            skyChange(Day, Night, num);
        }
        DynamicGI.UpdateEnvironment();
        //Debug.Log($"현재 하늘 색상: {skyboxInstance.name}");

    }
    void skyChange(Material a, Material b, float num) 
    {
        Color skyColor = Color.Lerp(a.GetColor("_SkyTint"), b.GetColor("_SkyTint"), num);
        skyboxInstance.SetColor("_SkyTint", skyColor);
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
        //Debug.Log($"\"현재 구름 색상: {currentCloudColor}");
    }
}

