using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPCMove: MonoBehaviour
{
    [Header("이동 범위")]
    public float moveRadius = 50f;

    [Header("속도 조절")]
    public float moveSpeed = 10f;
    public float rotSpeed = 2f;

    [Header("고정 높이")]
    public float Heignt;

    private Vector3 targetPos;


    void Start()
    {
        SetNavPoint();
    }

    void Update()
    {
            // 항상 고정된 높이 유지
            Vector3 currentPosition = transform.position;
            currentPosition.y = Heignt;
            transform.position = currentPosition;

            // 목표 지점까지 이동
            Vector3 direction = (targetPos - transform.position).normalized;
            Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z); // Y 방향 제거

            // 회전
            if (flatDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(flatDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotSpeed * Time.deltaTime);
            }

            // 이동
            transform.position += flatDirection * moveSpeed * Time.deltaTime;

            // 도착했는지 확인
            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPos.x, 0, targetPos.z)) < 1f)
            {
                SetNavPoint();
            }
    }

    void SetNavPoint()
    {
        Vector2 randomXZ = Random.insideUnitCircle * moveRadius;
        targetPos = new Vector3(randomXZ.x, Heignt, randomXZ.y);
    }
}
