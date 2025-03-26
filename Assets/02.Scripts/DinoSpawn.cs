using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinoSpawn : MonoBehaviour
{
    // 초식 공룡 프리팹
    [Header("초식 공룡 프리팹")]
    [SerializeField] private GameObject[] herbivorousPrefabsGreen;
    [SerializeField] private GameObject[] herbivorousPrefabsBlue;

    // 육식 공룡 프리팹
    [Header("육식 공룡 프리팹")]
    [SerializeField] private GameObject[] carnivorePrefabsWhite;
    [SerializeField] private GameObject[] carnivorePrefabsYellow;
    [SerializeField] private GameObject[] carnivorePrefabsRed;

    // 초식 공룡 스폰 위치
    [Header("초식 공룡 스폰위치")]
    [SerializeField] private Transform[] herbivorousSpawnGreen;
    [SerializeField] private Transform[] herbivorousSpawnBlue;

    // 육식 공룡 스폰 위치
    [Header("육식 공룡 스폰위치")]
    [SerializeField] private Transform[] carnivoreSpawnWhite;
    [SerializeField] private Transform[] carnivoreSpawnYellow;
    [SerializeField] private Transform[] carnivoreSpawnRed;

    // 기즈모 색상
    [Header("기즈모 색상")]
    [SerializeField] private Color gizmoColor_LvGreen = Color.green;
    [SerializeField] private Color gizmoColor_LvBlue = Color.blue;
    [SerializeField] private Color gizmoColor_LvWhite = Color.white;
    [SerializeField] private Color gizmoColor_LvYellow = Color.yellow;
    [SerializeField] private Color gizmoColor_LvRed = Color.red;

    void Start()
    {
        // 초식 공룡 스폰 그룹
        Dictionary<Transform[], GameObject[]> herbivoreGroups = new Dictionary<Transform[], GameObject[]>
        {
            { herbivorousSpawnGreen, herbivorousPrefabsGreen },
            { herbivorousSpawnBlue, herbivorousPrefabsBlue }
        };

        // 육식 공룡 스폰 그룹
        Dictionary<Transform[], GameObject[]> carnivoreGroups = new Dictionary<Transform[], GameObject[]>
        {
            { carnivoreSpawnWhite, carnivorePrefabsWhite },
            { carnivoreSpawnYellow, carnivorePrefabsYellow },
            { carnivoreSpawnRed, carnivorePrefabsRed }
        };

        // 초식 공룡 스폰
        foreach (var group in herbivoreGroups)
        {
            SpawnPrefabs(group.Key, group.Value);
        }

        // 육식 공룡 스폰
        foreach (var group in carnivoreGroups)
        {
            SpawnPrefabs(group.Key, group.Value);
        }
    }

    /// <summary>
    /// 공룡 프리팹을 스폰 위치에 랜덤하게 생성
    /// </summary>
    private void SpawnPrefabs(Transform[] spawnPoints, GameObject[] prefabs)
    {
        if (prefabs.Length == 0 || spawnPoints.Length == 0) return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            int randomIndex = Random.Range(0, prefabs.Length);
            Instantiate(prefabs[randomIndex], spawnPoint.position, spawnPoint.rotation);
        }
    }

    /// <summary>
    /// 기즈모를 사용하여 스폰 위치를 시각적으로 표시
    /// </summary>
    private void OnDrawGizmos()
    {
        Dictionary<Transform[], Color> spawnGroups = new Dictionary<Transform[], Color>
        {
            { herbivorousSpawnGreen, gizmoColor_LvGreen },
            { herbivorousSpawnBlue, gizmoColor_LvBlue },
            { carnivoreSpawnWhite, gizmoColor_LvWhite },
            { carnivoreSpawnYellow, gizmoColor_LvYellow },
            { carnivoreSpawnRed, gizmoColor_LvRed }
        };

        foreach (var group in spawnGroups)
        {
            if (group.Key == null || group.Key.Length == 0) continue; // 스폰 배열이 비었으면 건너뜀
            Gizmos.color = group.Value; // 색상 설정

            foreach (Transform spawnPoint in group.Key)
            {
                Gizmos.DrawSphere(spawnPoint.position, 10f); // 스폰 위치를 구체로 표시
            }
        }
    }
}
