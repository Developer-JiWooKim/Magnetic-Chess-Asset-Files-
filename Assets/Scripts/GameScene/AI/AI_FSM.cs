using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_FSM : MonoBehaviour
{
    [SerializeField]
    private GameObject magnetBallSpawnPoints;                                   // 스폰위치를 가진 오브젝트

    private List<MagnetBallSpawnPoint> spawnPointList;                          // 공 생성위치, 현재 비어있는지 여부를 판단하는 스크립트 리스트

    private List<Transform> emptyPointsTransform = new List<Transform>();       // IsEmpty가 true인 위치를 저장할 리스트
    private List<Transform> notEmptyPointsTransform = new List<Transform>();    // IsEmpty가 false인 위치를 저장할 리스트

    private List<int> final_index = new List<int>();

    private void Awake()
    {
        Setup();
    }
    private void Setup()
    {
        spawnPointList = magnetBallSpawnPoints.GetComponentsInChildren<MagnetBallSpawnPoint>().ToList();
    }
    /// <summary>
    /// Spawn Point가 empty, not empty 인지 구분해 각 리스트에 저장하는 함수
    /// </summary>
    private void CheckSpawnPoints()
    {
        emptyPointsTransform.Clear();
        notEmptyPointsTransform.Clear();
        spawnPointList.ForEach(point =>
        {
            if (point.IsEmpty)
            {
                emptyPointsTransform.Add(point.gameObject.GetComponent<Transform>().transform);
            }
            else
            {
                notEmptyPointsTransform.Add(point.gameObject.GetComponent<Transform>().transform);
            }
        });
    }
    /// <summary>
    /// 공을 생성할 위치를 결정하는 함수
    /// </summary>
    private Vector3 DecideSpawnPoint()
    {
        // 탐색 범위
        float range = 2f;
        bool non_existent = false;

        final_index.Clear();
        Search_Close_MagnetBall(range); // range안에 

        // 탐색 범위 안에 공이 없는 위치가 존재하지 않으면 다시 반복
        while (final_index.Count == 0)
        {
            // 탐색 범위를 계속 줄이다 0.5이하가 되면 어디에 놔도 근처이기 때문에 더이상 탐색을 하지 않음
            if (range < 0.5f)
            {
                non_existent = true;
                break;
            }
            range -= 0.2f; // 탐색 범위를 좁힘
            Search_Close_MagnetBall(range); // 위치 다시 탐색
        }

        // emptyPointsTransform의 인덱스
        int index = 0;

        // 탐색 범위 안에 공이 없는 위치가 존재하지 않으면 빈 곳 중 아무데나 인덱스 저장
        if (non_existent) 
        {
            index = Random.Range(0, emptyPointsTransform.Count);
        }
        // 탐색 범위 안에 공이 없는 위치가 존재하면 그 위치들 중 랜덤으로 인덱스 저장
        else
        {
            index = final_index[Random.Range(0, final_index.Count)];
        }

        // 최종 결정된 비어있는 위치를 리턴
        return emptyPointsTransform[index].position;
    }
    private void Search_Close_MagnetBall(float range)
    {
        final_index.Clear();

        float search_Range = range;     // 비어 있는 위치만큼 반복
        bool isOk;                      // 탐색 범위 안에 들어오면 리스트에서 제거시키기 위해 판별용 변수

        for (int i = 0; i < emptyPointsTransform.Count; i++)
        {
            isOk = true;
            // 빈 곳과 공이 있는 위치의 거리를 구해서 search_Range 안에 있으면 해당 빈 위치 제외
            for (int j = 0; j < notEmptyPointsTransform.Count; j++)
            {
                // i번째 빈 곳과 j번째 공의 위치를 구함
                float distance = Vector3.Distance(emptyPointsTransform[i].position, notEmptyPointsTransform[j].position);
                // 공과의 거리가 search_Range보다 작으면 해당 위치 제외
                if (distance < search_Range)
                {
                    isOk = false;
                }
            }
            if (isOk) // 모든 공과의 거리가 search_Range 보다 크면(빈 곳 기준 n거리 안에 공이 없으면)
            {
                final_index.Add(i); // 인덱스를 추가
            }
        }
    }

    /// <summary>
    /// 모든 포인트가 비어있으면 랜덤으로 위치 정보를 결정 후 위치 값을 리턴하는 함수
    /// </summary>
    private Vector3 RandomSpawnPoint()
    {
        int randomIndex = Random.Range(0, emptyPointsTransform.Count);

        return emptyPointsTransform[randomIndex].position;
    }
    public void SpawnPoint_Initialize()
    {
        spawnPointList.ForEach(point => point.Initialize());
    }
    /// <summary>
    /// 외부(GameDirector.cs)에서 호출할 AI가 생성할 마그넷 볼 위치를 리턴하는 함수
    /// </summary>
    public Vector3 AIMagnetBallSpawnPoint()
    {
        CheckSpawnPoints();

        if (notEmptyPointsTransform.Count > 0)
        {
            return DecideSpawnPoint();
        }
        else
        {
            return RandomSpawnPoint();
        }
    }
}