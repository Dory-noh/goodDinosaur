using System.ComponentModel;
using UnityEngine;

public class FixedVRUI : MonoBehaviour
{
    public Transform vrCamera; // VR 카메라를 연결할 변수
    public Vector3 offset = new Vector3(0.5f, 0.3f, 1f); // 카메라 기준으로 UI 위치를 조절할 오프셋
    public bool lookAtCamera = true; // 항상 카메라를 바라보게 할지 여부

    void Start()
    {
        if (vrCamera == null)
        {
            Debug.LogError("VR Camera가 FixedVRUI 스크립트에 연결되지 않았습니다.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // 원하는 위치 계산 (카메라의 로컬 좌표계를 기준으로 오프셋 적용)
        Vector3 targetPosition = vrCamera.position + vrCamera.TransformDirection(offset);
        transform.position = targetPosition;

        // 카메라를 바라보도록 회전
        if (lookAtCamera)
        {
            transform.LookAt(vrCamera);
            // 필요에 따라 Y축 회전만 고정하여 UI가 항상 수직으로 보이게 할 수 있습니다.
            // transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }
}