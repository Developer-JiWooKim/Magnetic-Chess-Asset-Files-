using System.Collections.Generic;
using UnityEngine;

public class MemoryPool
{
    public class PoolItem
    {
        public bool isActive;
        public GameObject gameObject;
    }

    //private int increaseCount = 5;      
    private int maxCount;
    private int activeCount;

    private GameObject poolObject;
    private List<PoolItem> poolItemList;

    public int MaxCount => maxCount;
    public int ActiveCount => activeCount;

    public List<PoolItem> PoolItemList => poolItemList;



    private Vector3 tempPosition = Vector3.zero;

    public MemoryPool(GameObject poolObject)
    {
        maxCount = 0;
        activeCount = 0;
        this.poolObject = poolObject;

        poolItemList = new List<PoolItem>();

        // InstantiateObjects();
    }

    public void InstantiateObjects(int _maxCount)
    {
        for (int i = 0; i < _maxCount; i++)
        {
            PoolItem poolItem = new PoolItem();

            poolItem.isActive = false;
            poolItem.gameObject = GameObject.Instantiate(poolObject);
            poolItem.gameObject.transform.position = tempPosition;
            poolItem.gameObject.SetActive(false);

            poolItemList.Add(poolItem);
        }
    }

    public void DestroyObjects()
    {
        if (poolItemList == null)
        {
            Debug.Log("DestroyObjects() : poolItemList is null!!");
            return;
        }

        poolItemList.ForEach(poolItem => GameObject.Destroy(poolItem.gameObject));
        poolItemList.Clear(); // ����Ʈ �ʱ�ȭ
    }

    public GameObject ActivatePoolItem()
    {
        if (poolItemList == null)
        {
            Debug.Log("ActivatePoolItem() : poolItemList is null!!");
            return null;
        }


        //if (maxCount == activeCount)
        //{
        //    InstantiateObjects();
        //}

        PoolItem item = poolItemList.Find(poolItem => poolItem.isActive == false);
        if (item == null)
        {
            Debug.Log("ActivatePoolItem() : ActiveItem is null!!");
            return null;
        }
        activeCount++;
        item.isActive = true;
        item.gameObject.SetActive(true);
        return item.gameObject;
    }


    public void DeactivatePoolItem(GameObject removeObject)
    {
        if (poolItemList == null || removeObject == null)
        {
            Debug.Log("DeactivatePoolItem() : poolItemList is null or removeObject is null!!");
            return;
        }

        PoolItem poolItem = poolItemList.Find(poolItem => poolItem.gameObject == removeObject);
        if (poolItem == null)
        {
            Debug.Log("DeactivatePoolItem() : ActiveItem is null!!");
            return;
        }
        activeCount--;
        poolItem.gameObject.transform.position = tempPosition;
        poolItem.isActive = false;
        poolItem.gameObject.SetActive(false);
    }

    public void DeactivateAllPoolItems()
    {
        if (poolItemList == null)
        {
            Debug.Log("DeactivateAllPoolItems() : poolItemList is null!!");
            return;
        }
        poolItemList.ForEach(poolItem => DeactivatePoolItem(poolItem.gameObject));
        activeCount = 0;
    }
}