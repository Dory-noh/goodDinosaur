using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    [SerializeField] private Transform cam;

    void Start()
    {
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if(cam == null && Camera.main != null) { cam = Camera.main.transform; }
        if (cam == null) return;
        if(GameManager.Instance.GameOver || GameManager.Instance.IsPlay == false) return;
        transform.forward = cam.forward;
    }
}

