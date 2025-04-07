using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class StarFlicker : MonoBehaviour
{
    float dayRatio;
    public float daytime = 60f;
    float time = 0f;
    public SpriteRenderer starLight;
    public Color colorB;
    public Color colorA;
    public Color colorNone;
    public float speed = 2f;
    void Start()
    { 
        dayRatio = time / daytime;
        starLight = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (GameManager.Instance.GameOver || GameManager.Instance.IsPlay == false)
        {
            time = 0;
            GameManager.Instance.Day = 1;
            return;
        }

        time += Time.deltaTime; //시간을 계속 Update
        dayRatio = time / daytime;
        Flicker();
    }
    public void Flicker() 
    {
        if (dayRatio <= 0.5f)
        {
            starLight.color = colorNone;
        }
        else if (dayRatio > 0.5f && dayRatio <= 0.75f)
        {
            float ratio = (dayRatio - 0.5f) * 4f;
            starLight.color = Color.Lerp(colorNone, colorA, ratio);
        }
        else
        {
            // 밤 (0.75 ~ 1.0): 태양 사라짐, 대기 어두워짐
            float ratio = (dayRatio - 0.75f) * 4f; // 밤 구간을 0~1로 매핑
            starLight.color = Color.Lerp(colorA, colorB, ratio);

        }
    }


}
