using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class StarFlicker : MonoBehaviour
{
    float dayRatio;
    public float daytime = 60f;
    float time;
    public SpriteRenderer starLight;
    public Color colorB;
    public Color colorNone;
    public float speed = 2f;
    void Start()
    { 
        dayRatio = time / daytime;
        starLight = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        Flicker();
    }
    public void Flicker() 
    {
        if (dayRatio >0.2 && dayRatio < 0.7)
        {
            starLight.color = colorNone;
        }
        else 
        {
            starLight.color = Color.Lerp(colorNone, colorB, (dayRatio-0.5f)*2f);
        }
    }


}
