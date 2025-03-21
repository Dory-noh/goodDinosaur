using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class DaytoNight : MonoBehaviour
{

    [Header("���� �� ����")]
    public Material Day;
    public Material Night;
    Material skyboxInstance; // ��ī�̹ڽ� ���纻


    [Header("�¾� ����")]
    public Light sun;
    public Light night;
    public Transform sunPivot;

    [Header("�ð�")]
    public float daytime = 60f;
    float time;
    float dayRatio;


    [Header("���� ����")]
    public Material cloudMaterial_L;
    public Material cloudMaterial_H;
    Material cloudMaterialInstance_L;
    Material cloudMaterialInstance_H;
    public Color dayCloudColor; // ������ ��� ����
    public Color nightCloudColor; // �� ���� �� 




    void Awake()
    {
        dayRatio = time / daytime;
        time = 0f;
        // ��ī�̹ڽ� ���纻 ���� �� ����
        skyboxInstance = new Material(Day);
        RenderSettings.skybox = skyboxInstance;

        // ���� ��Ƽ���� ���纻 ����
        cloudMaterialInstance_L = new Material(cloudMaterial_L);
        cloudMaterialInstance_H = new Material(cloudMaterial_H);
    }


    void Update()
    {
        time +=Time.deltaTime; //�ð��� ��� ������Ʈ 
        if (time > daytime) { time = 0f; } //�Ϸ簡 ������ �ð��ʱ�ȭ
        sunRise();
        Day2Night();
    }
    void sunRise() 
    {
        dayRatio = time / daytime;
        float sunAng = dayRatio * 360f; //���� �㿡 ������ ���� �¾��� ��
        sunPivot.transform.rotation = Quaternion.Euler(sunAng - 90f, 90f, 0f);
        //x�� 90�� ������ �� ���Ĵ� ����(���� 180��)�� �� �¾��� �������� �ְ� ����� ����. 
        //y���� �¾��� ��ġ�� ���� ����, �̴� ���⿡ �°� ���� ���� 
        // �յ� ����� �ʿ� �����Ƿ� z���� 0f 
        //���� �߾ӿ� ��ġ�� sunPivot�� �������� 360�� ȸ�� 

    }
    void Day2Night() 
    {
        dayRatio = time / daytime;
        if (dayRatio > 0.2 && dayRatio < 0.7)
        {
            float num = dayRatio * 2;
            //Debug.Log("���̿���.");
            sun.intensity = Mathf.Lerp(0.1f, 1f, dayRatio * 2); //�ذ� �ߴ� ���� 
            //�¾��� �� �е��� 0.1���� 1��, �������� 2�� ���� ����ŭ�� �ӵ��� �ٲ��.
            sun.color = Color.Lerp(Color.red, Color.white, dayRatio * 2);//���� �� ��ȭ 
            night.intensity = Mathf.Lerp(3, 0, dayRatio *2);
            UpdateCloudColor(dayCloudColor, nightCloudColor);
            skyChange(Day, Night, num);
        }
        else
        {
            float num = (dayRatio - 0.5f) * 2;
            //Debug.Log("���� �Ǿ����ϴ�.");
            sun.intensity = Mathf.Lerp(1f, 0f, (dayRatio-0.5f) * 2); //�ذ� �ߴ� ���� 
            //�¾��� �� �е��� 1���� 0.1��, ������� 2�� ���� ����ŭ�� �ӵ��� �ٲ��.
            sun.color = Color.Lerp(Color.white, Color.red, (dayRatio - 0.5f) * 2); //���� �� ��ȭ 
            night.intensity = Mathf.Lerp(0, 3, dayRatio * 2);
            UpdateCloudColor(nightCloudColor, dayCloudColor);
            skyChange(Day, Night, num);
        }
        DynamicGI.UpdateEnvironment();
        //Debug.Log($"���� �ϴ� ����: {skyboxInstance.name}");

    }
    void skyChange(Material a, Material b, float num) 
    {
        Color skyColor = Color.Lerp(a.GetColor("_SkyTint"), b.GetColor("_SkyTint"), num);
        skyboxInstance.SetColor("_SkyTint", skyColor);
    }


    void UpdateCloudColor(Color a, Color b)
    {
        float dayRatio = time / daytime;

        // ���� ������ ������ ���, �㿡�� ȸ������ ����
        Color currentCloudColor = Color.Lerp(a, b, Mathf.Sin(dayRatio * Mathf.PI));

        // ���� ��Ƽ���� ���纻�� ���� ����
        cloudMaterialInstance_L.SetColor("_BaseColor", currentCloudColor);
        cloudMaterialInstance_H.SetColor("_BaseColor", currentCloudColor);

        // ����� ��Ƽ���� ����
        cloudMaterial_L = cloudMaterialInstance_L;
        cloudMaterial_H = cloudMaterialInstance_H;
        //Debug.Log($"\"���� ���� ����: {currentCloudColor}");
    }
}

