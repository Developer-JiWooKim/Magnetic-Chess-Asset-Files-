using System.Collections.Generic;
using UnityEngine;

public class MagnetBallMemoryPool : MonoBehaviour
{
    [SerializeField]
    private GameObject magnetBallPrefab;

    private MemoryPool magnetBallMemoryPool;

    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        magnetBallMemoryPool = new MemoryPool(magnetBallPrefab);
    }


    public void ActivateMagnetBall(Vector3 pos, Quaternion rot)
    {
        // GameObject item = memoryPool.ActivatePoolItem();
        //item.transform.position = pos;
        //item.transform.rotation = rot;

        //item.GetComponent<Magnet>();

        GameObject item = magnetBallMemoryPool.ActivatePoolItem();

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


    public void InstantiateMagnetBall(int magnetBallCount)
    {

        if (magnetBallMemoryPool.PoolItemList.Count >= magnetBallCount)
        {
            return;
        }

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
