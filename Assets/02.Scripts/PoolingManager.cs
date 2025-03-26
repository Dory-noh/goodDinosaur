using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    private static PoolingManager instance;
    public static PoolingManager Instance
    {
        get 
        {
            if(instance == null)
            {
                instance = FindObjectOfType<PoolingManager>();
                if(instance == null)
                {
                    Debug.Log("풀링 매니저 인스턴스가 해당 씬에 존재하지 않습니다.");
                    return null;
                }
            }
            return instance;
        }
    }

    [SerializeField] private List<Transform[]> SpawnPoints = new List<Transform[]>();
    
    // 초식 공룡 스폰 위치
    [Header("초식 공룡 스폰위치")]
    [SerializeField] private Transform[] herbivorousSpawnGreen;
    [SerializeField] private Transform[] herbivorousSpawnBlue;

    // 육식 공룡 스폰 위치
    [Header("육식 공룡 스폰위치")]
    [SerializeField] private Transform[] carnivoreSpawnWhite;
    [SerializeField] private Transform[] carnivoreSpawnYellow;
    [SerializeField] private Transform[] carnivoreSpawnRed;

    //프리팹 - 스크립터블 오브젝트로 정리한 프리팹 가져온다. (초식/육식 순)
    [SerializeField] private DinosaurCategory[] dinoPrefabs;


    public List<GameObject>[,] dinoPools = new List<GameObject>[2, 3];
    private int[] createCounts = { 0, 2, 2, 5, 2, 2 };
    private int[,] spawnCounts = { {0, 6, 2, 12, 1, 2 }, { 0, 4, 2, 12, 2, 4 } };
    GameObject CarnParent;
    GameObject HerbParent;
    [SerializeField] float[,] respawnTime = { {0, 150, 300, 90, 180, 300}, {0, 180, 240, 60, 150, 180} };
    int countMode;
    WaitForSeconds[,] ws = new WaitForSeconds[2,6];
    void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        
        //스폰 포인트 설정
        SpawnPoints.Add(herbivorousSpawnGreen);
        SpawnPoints.Add(herbivorousSpawnBlue);
        SpawnPoints.Add(carnivoreSpawnWhite);
        SpawnPoints.Add(carnivoreSpawnYellow);
        SpawnPoints.Add(carnivoreSpawnRed);

        createDinos();
        for (int i = 0; i < 2; i++) //초식: 0, 육식: 1
        {
            for (int j = 0; j < 6; j++) //소: 0 / 중: 1 / 대: 2 
            {
                ws[i, j] = new WaitForSeconds(respawnTime[i, j]);
            }
        }
    }

    void Update()
    {
        
    }

    void createDinos()
    {
        HerbParent = new GameObject("초식 공룡");
        HerbParent.transform.parent = transform;

        CarnParent = new GameObject("육식 공룡");
        CarnParent.transform.parent = transform;

        //초식/육식으로 나누므로 i 두 번 돈다.
        for (int i = 0; i < dinoPrefabs.Length; i++)
        {
            for(int size = 0; size < 3; size++)
                CreateBySize(i, size);
        }
        SetDinos();
    }

    private void CreateBySize(int typeIdx, int sizeIdx)
    {
        GameObject[] prefabBySize = new GameObject[] { };
        switch (sizeIdx)
        {
            case 0: 
                prefabBySize = dinoPrefabs[typeIdx].small;  // 소형
                break;
            case 1:
                prefabBySize = dinoPrefabs[typeIdx].medium; // 중형
                break;
            case 2:
                prefabBySize = dinoPrefabs[typeIdx].large;  // 대형
                break;
        }

        dinoPools[typeIdx, sizeIdx] = new List<GameObject> { };

        for (int k = 0; k < prefabBySize.Length; k++)
        {
            for (int m = 0; m < createCounts[(typeIdx * 3 + sizeIdx)]; m++)
            {
                GameObject dinosaur = Instantiate(prefabBySize[k], (typeIdx == 0 ? HerbParent : CarnParent).transform);
                dinosaur.name = (sizeIdx == 0 ? "소형" : sizeIdx == 1 ? "중형" : "대형") + " " +prefabBySize[k].name.ToString();
                dinosaur.SetActive(false);
                dinoPools[typeIdx, sizeIdx].Add(dinosaur);
            }
        }
    }

    void SetDinos()
    {
        for (int typeIdx = 0; typeIdx < dinoPrefabs.Length; typeIdx++)
        {
            for (int sizeIdx = 0; sizeIdx < 3; sizeIdx++)
            {
                //day가 7일 이내면 모드 0, 아니면 모드 1 => 모드마다 크기별 스폰 수 다르다.
                countMode = GameManager.Instance.Day <= 7 ? 0 : 1;
                for (int count = 0; count < spawnCounts[countMode, (typeIdx * 3 + sizeIdx)]; count++)
                {
                    RandomSpawnDino(typeIdx, sizeIdx);
                }

            }
        }
    }
    //public void AdjustDinoCount()
    //{
    //    Debug.Log("추가생성 시작");
    //    countMode = GameManager.Instance.Day <= 7 ? 0 : 1;
    //    Debug.Log(countMode);
    //    for (int typeIdx = 0; typeIdx < dinoPrefabs.Length; typeIdx++)
    //    {
    //        for (int sizeIdx = 0; sizeIdx < 3; sizeIdx++)
    //        {
    //            int currentActiveCount = dinoPools[typeIdx, sizeIdx].FindAll(d => d.activeSelf).Count;
    //            int requiredCount = spawnCounts[countMode, (typeIdx * 3 + sizeIdx)];

    //            if (currentActiveCount < requiredCount)
    //            {
    //                // 부족한 개수만큼 추가 스폰
    //                int needToSpawn = requiredCount - currentActiveCount;
    //                for (int i = 0; i < needToSpawn; i++)
    //                {
    //                    Debug.Log("추가생성됨");
    //                    RandomSpawnDino(typeIdx, sizeIdx);
    //                }
    //            }
    //        }
    //    }
    //}
    public void SetDinosWithReset()
    {
        Debug.Log("공룡 수 조절");
        // 기존 공룡들 비활성화
        for (int typeIdx = 0; typeIdx < dinoPrefabs.Length; typeIdx++)
        {
            for (int sizeIdx = 0; sizeIdx < 3; sizeIdx++)
            {
                foreach (var dino in dinoPools[typeIdx, sizeIdx])
                {
                    dino.SetActive(false);
                }
            }
        }

        // 새로운 스폰 로직 실행
        SetDinos();
    }

    //void SpawnDino(int size)
    //{
    //    foreach (var dino in dinoPools[size])
    //    {
    //        if (dino.activeSelf == false)
    //        {
    //            Vector3 pos = SpawnPoints[(Random.Range(0, SpawnPoints.Length))].position;
    //            dino.transform.position = new Vector3(Random.Range(pos.x - 40, pos.x + 40), pos.y + 5, Random.Range(pos.z - 40, pos.z + 40));
    //            dino.transform.rotation = Quaternion.identity;
    //            dino.gameObject.SetActive(true);
    //            break;
    //        }
    //    }
    //}

    void RandomSpawnDino(int typeIdx, int sizeIdx)
    {
        int count = 0;
        while (++count < dinoPools[typeIdx, sizeIdx].Count)
        {
            GameObject dino = dinoPools[typeIdx, sizeIdx][Random.Range(0, dinoPools[typeIdx, sizeIdx].Count)];
            if (dino.activeSelf == false)
            {
                int spawnPointArrayIndex = 2 * typeIdx + sizeIdx;
                if (spawnPointArrayIndex >= 0 && spawnPointArrayIndex < SpawnPoints.Count && SpawnPoints[spawnPointArrayIndex] != null && SpawnPoints[spawnPointArrayIndex].Length > 0)
                {
                    Transform[] currentSpawnPoints = SpawnPoints[spawnPointArrayIndex];
                    int randomIndex = Random.Range(0, currentSpawnPoints.Length);
                    Vector3 pos = currentSpawnPoints[randomIndex].position;
                    dino.transform.position = new Vector3(Random.Range(pos.x - 5, pos.x + 5), pos.y + 2, Random.Range(pos.z - 5, pos.z + 5));
                    dino.transform.rotation = Quaternion.identity;
                    dino.gameObject.SetActive(true);
                    break;
                }
                else
                {
                    Debug.LogError($"Invalid or empty spawn point array at index: {spawnPointArrayIndex}");
                    break;
                }
            }
        }
    }

    IEnumerator WaitSpawnDino(int typeIdx, int sizeIdx)
    {
        Debug.Log($"{respawnTime[countMode, (typeIdx * 3 + sizeIdx)]}초 후 공룡 리스폰합니다.");
        yield return ws[countMode, (typeIdx * 3 + sizeIdx)];
        RandomSpawnDino(typeIdx, sizeIdx);
    }

    public void CallSpawn(int typeIdx,int sizeIdx)
    {
        StartCoroutine(PoolingManager.Instance.WaitSpawnDino(typeIdx, sizeIdx));
    }
}
