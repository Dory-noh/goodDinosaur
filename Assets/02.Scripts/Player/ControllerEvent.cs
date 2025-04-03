using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerEvent : MonoBehaviour
{
    public InputActionProperty leftHandTriggerAction;
    public InputActionProperty rightHandTriggerAction;
    public UnityEvent ToggleRay;
    public UnityEvent ToggleRayOff;
    [SerializeField] private GameObject colorBall;
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;

    private void Start()
    {
        EnableRayInteractor();
    }

    //fish를 ray로 hover할 때 작동하는 메서드
    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        GameObject dinosaur = args.interactableObject.transform.gameObject;
        if (dinosaur != null)
        {
            //감지한 공룡이 랩터면 공의 색을 노란 색으로 한다.
            if (dinosaur.GetComponent<Raptor>() != null) { colorBall.GetComponent<MeshRenderer>().material.color = Color.yellow; return; }
            //Debug.Log($"공룡 감지 : {dinosaur.name}");
            int level = dinosaur.GetComponent<Animal>().infoIdx;
            int count;
            if (GetComponent<PlayerControl>().leader != null) count = GameManager.Instance.playerTeamSize;
            else //플레이어는 항상 리더이기 때문에 leader가 null이 되지 않지만 오류 방지를 막기 위해 넣어 두었다.
            {
                count = 1;
            }

            //현재 속한 무리의 팀원 수가(리더 포함) 5 이상이면 어떤 공룡이든 잡을 수 있으므로 공의 색을 파란색으로 한다.
            if (count >= 5) colorBall.GetComponent<MeshRenderer>().material.color = Color.blue;
            //현재 속한 무리의 팀원 수가(리더 포함) 3 이상이고 5 미만이면
            else if (count >= 3)
            {
                //size가 중형 이하인 공룡만 잡을 수 있다.
                if (level <= 1) colorBall.GetComponent<MeshRenderer>().material.color = Color.blue;

                else colorBall.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else //현재 속한 무리의 팀원 수가(리더 포함) 3 미만이면
            {
                //소형 동물만 잡을 수 있다.
                if (level == 0) colorBall.GetComponent<MeshRenderer>().material.color = Color.blue;
                else colorBall.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            Invoke("resetColorBall", 3f);
        }
        else
        {
            Debug.Log("현재 Ray에 감지된 Fish가 없습니다. ColorBall 초기화");
            resetColorBall();
        }
    }

    private void resetColorBall()
    {
        colorBall.GetComponent<MeshRenderer>().material.color = Color.white;
    }

    private void OnEnable()
    {
        leftHandTriggerAction.action.performed += OnTriggerPressed;
        rightHandTriggerAction.action.performed += OnTriggerPressed;
        leftHandTriggerAction.action.canceled += OnTriggerReleased;
        rightHandTriggerAction.action.canceled += OnTriggerReleased;
    }

    private void OnDisable()
    {
        leftHandTriggerAction.action.performed -= OnTriggerPressed;
        rightHandTriggerAction.action.performed -= OnTriggerPressed;
        leftHandTriggerAction.action.canceled -= OnTriggerReleased;
        rightHandTriggerAction.action.canceled -= OnTriggerReleased;
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        EnableRayInteractor();
    }

    private void OnTriggerReleased(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.GameOver || GameManager.Instance.IsPlay == false) return;
        Invoke("DisableRayInteractor", 0.5f);
    }

    public void DisableRayInteractor()
    {
        leftController.GetComponent<XRRayInteractor>().enabled = false;
        rightController.GetComponent<XRRayInteractor>().enabled = false;

        ToggleRayOff.Invoke();
    }
    public void EnableRayInteractor()
    {
        leftController.GetComponent<XRRayInteractor>().enabled = true;
        rightController.GetComponent<XRRayInteractor>().enabled = true;
        ToggleRay.Invoke();
    }
}
