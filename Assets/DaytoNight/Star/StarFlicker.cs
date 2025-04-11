using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class StarFlicker : MonoBehaviour
{
    float dayRatio;
    public float daytime = 300f;
    float time = 0f;
    public SpriteRenderer starLight;
    public Color colorB;
    public Color colorA;
    public Color colorNone;
    public float speed = 2f;
    void Start()
    {
        //daytime = 300f;
        daytime = 300f;
        dayRatio = time / daytime;
        starLight = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (GameManager.Instance.GameOver || GameManager.Instance.IsPlay == false)
        {
            time = 0;
            starLight.color = colorNone;
            if (GameManager.Instance.isStop == true)
            {
                Debug.Log("8일차 애니메이션 재생 중이므로 Day를 바꾸지 않음.");

                return;           
            }
            GameManager.Instance.Day = 1;
            //Debug.Log($"Day를 1로 바꿉니다.");
            return;
        }
     if (time > daytime)
        {
            time = 0f;
        }
        time += Time.deltaTime; //시간을 계속 Update
        dayRatio = time / daytime;
   
        Flicker();
    }
    public void Flicker() 
    {


        
        if (dayRatio > 0f && dayRatio < 0.2f)
        {
            starLight.color = Color.Lerp(colorB, colorNone, 0.2f);
            //Debug.Log($"시간 비 : {dayRatio} - 별 떠 있음");
        }
        else if (dayRatio <= 0.5f)
        {
            starLight.color = colorNone;
            //Debug.Log($"시간 비 : {dayRatio} - 별 안 떠 있음");
        }
        else if (dayRatio > 0.7f && dayRatio <= 0.75f)
        {
            float ratio = (dayRatio - 0.5f) * 4f;
            starLight.color = Color.Lerp(colorNone, colorA, ratio);
            //Debug.Log($"시간 비 : {dayRatio} - 별 떠 있음");
        }
        else
        {
            // 밤 (0.75 ~ 1.0): 태양 사라짐, 대기 어두워짐
            float ratio = (dayRatio - 0.75f) * 4f; // 밤 구간을 0~1로 매핑
            starLight.color = Color.Lerp(colorA, colorB, ratio);
            //Debug.Log($"시간 비 : {dayRatio} - 별 떠 있음");
        }
    }


}
