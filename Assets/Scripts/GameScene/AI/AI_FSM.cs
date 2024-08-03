using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_FSM : MonoBehaviour
{
    [SerializeField]
    private GameObject magnetBallSpawnPoints;                                   // ������ġ�� ���� ������Ʈ

    private List<MagnetBallSpawnPoint> spawnPointList;                          // �� ������ġ, ���� ����ִ��� ���θ� �Ǵ��ϴ� ��ũ��Ʈ ����Ʈ

    private List<Transform> emptyPointsTransform = new List<Transform>();       // IsEmpty�� true�� ��ġ�� ������ ����Ʈ
    private List<Transform> notEmptyPointsTransform = new List<Transform>();    // IsEmpty�� false�� ��ġ�� ������ ����Ʈ

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
    /// Spawn Point�� empty, not empty ���� ������ �� ����Ʈ�� �����ϴ� �Լ�
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
    /// ���� ������ ��ġ�� �����ϴ� �Լ�
    /// </summary>
    private Vector3 DecideSpawnPoint()
    {
        // Ž�� ����
        float range = 2f;
        bool non_existent = false;

        final_index.Clear();
        Search_Close_MagnetBall(range); // range�ȿ� 

        // Ž�� ���� �ȿ� ���� ���� ��ġ�� �������� ������ �ٽ� �ݺ�
        while (final_index.Count == 0)
        {
            // Ž�� ������ ��� ���̴� 0.5���ϰ� �Ǹ� ��� ���� ��ó�̱� ������ ���̻� Ž���� ���� ����
            if (range < 0.5f)
            {
                non_existent = true;
                break;
            }
            range -= 0.2f; // Ž�� ������ ����
            Search_Close_MagnetBall(range); // ��ġ �ٽ� Ž��
        }

        // emptyPointsTransform�� �ε���
        int index = 0;

        // Ž�� ���� �ȿ� ���� ���� ��ġ�� �������� ������ �� �� �� �ƹ����� �ε��� ����
        if (non_existent) 
        {
            index = Random.Range(0, emptyPointsTransform.Count);
        }
        // Ž�� ���� �ȿ� ���� ���� ��ġ�� �����ϸ� �� ��ġ�� �� �������� �ε��� ����
        else
        {
            index = final_index[Random.Range(0, final_index.Count)];
        }

        // ���� ������ ����ִ� ��ġ�� ����
        return emptyPointsTransform[index].position;
    }
    private void Search_Close_MagnetBall(float range)
    {
        final_index.Clear();

        float search_Range = range;     // ��� �ִ� ��ġ��ŭ �ݺ�
        bool isOk;                      // Ž�� ���� �ȿ� ������ ����Ʈ���� ���Ž�Ű�� ���� �Ǻ��� ����

        for (int i = 0; i < emptyPointsTransform.Count; i++)
        {
            isOk = true;
            // �� ���� ���� �ִ� ��ġ�� �Ÿ��� ���ؼ� search_Range �ȿ� ������ �ش� �� ��ġ ����
            for (int j = 0; j < notEmptyPointsTransform.Count; j++)
            {
                // i��° �� ���� j��° ���� ��ġ�� ����
                float distance = Vector3.Distance(emptyPointsTransform[i].position, notEmptyPointsTransform[j].position);
                // ������ �Ÿ��� search_Range���� ������ �ش� ��ġ ����
                if (distance < search_Range)
                {
                    isOk = false;
                }
            }
            if (isOk) // ��� ������ �Ÿ��� search_Range ���� ũ��(�� �� ���� n�Ÿ� �ȿ� ���� ������)
            {
                final_index.Add(i); // �ε����� �߰�
            }
        }
    }

    /// <summary>
    /// ��� ����Ʈ�� ��������� �������� ��ġ ������ ���� �� ��ġ ���� �����ϴ� �Լ�
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
    /// �ܺ�(GameDirector.cs)���� ȣ���� AI�� ������ ���׳� �� ��ġ�� �����ϴ� �Լ�
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