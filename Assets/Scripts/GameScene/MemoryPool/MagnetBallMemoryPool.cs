using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetBallMemoryPool : MonoBehaviour
{
    [SerializeField]
    private GameObject magnetBallPrefab; // ���׳ݺ� ������Ʈ

    private MemoryPool magnetBallMemoryPool; // ���׳ݺ� �޸�Ǯ, ����, Ȱ�� / ��Ȱ�� ����(Ǯ��)

    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        magnetBallMemoryPool = new MemoryPool(magnetBallPrefab);
    }

    // ��ġ, ȸ�� ���� �Ű������� �޾� �ش� ��ġ�� ���׳� ���� ����
    public void ActivateMagnetBall(Vector3 pos, Quaternion rot)
    {
        // GameObject item = memoryPool.ActivatePoolItem();
        //item.transform.position = pos;
        //item.transform.rotation = rot;

        //item.GetComponent<Magnet>();

        GameObject item = magnetBallMemoryPool.ActivatePoolItem(); // Ǯ ������ ����Ʈ�� ����ִ� ������Ʈ �� ��Ȱ��ȭ�Ȱ� Ȱ��ȭ ���Ѽ� ������ ������ ����
        // ��ġ, ȸ�� �� ����
        item.transform.position = pos;
        item.transform.rotation = rot;
    }

    public void DeactivateMagnetBall(GameObject magnetBall)
    {
        magnetBallMemoryPool.DeactivatePoolItem(magnetBall);
    }

    public void DeactivateAllMagnetBall()
    {
        magnetBallMemoryPool.DeactivateAllPoolItems();
    }

    // ���� �ɼ��� �����ϰ� �ִ� �ǽ� ������ �������� ���� ���Ϳ��� ü�� �ǽ����� �̸� ����(Ǯ��)�ϱ� ���� �� �Լ��� ȣ����
    public void InstantiateMagnetBall(int magnetBallCount)
    {
        // Ǯ ������ ����Ʈ ������ ���� ���ں��� ũ�� ���� x
        if (magnetBallMemoryPool.PoolItemList.Count >= magnetBallCount)
        {
            return;
        }
        // Ǯ ������ ����Ʈ ������ ���� ���ں��� ������ �� ���� ��ŭ ����
        else
        {
            magnetBallCount -= magnetBallMemoryPool.PoolItemList.Count;
        }

        magnetBallMemoryPool.InstantiateObjects(magnetBallCount);
    }

    public List<MemoryPool.PoolItem> GetPoolItemList()
    {
        if (magnetBallMemoryPool == null)
        {
            Debug.Log("GetPoolItemList() : magnetBallMemoryPool is null!!");
            return null;
        }
        return magnetBallMemoryPool.PoolItemList;
    }
}
