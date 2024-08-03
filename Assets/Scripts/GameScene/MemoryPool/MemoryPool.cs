using System.Collections.Generic;
using UnityEngine;

public class MemoryPool
{
    public class PoolItem
    {
        public bool isActive;       // "gameObject"�� Ȱ�� / ��Ȱ��ȭ ����
        public GameObject gameObject;     // ȭ�鿡 ���̴� ���� ������Ʈ
    }

    //private int increaseCount = 5;          // ������Ʈ�� ������ �� Instantiate()�� �߰� �����Ǵ� ������Ʈ ����
    private int maxCount;                   // ���� ����Ʈ�� ��ϵǾ� �ִ� ������Ʈ ����
    private int activeCount;                // ���� ���ӿ� ���ǰ� �ִ� ������Ʈ ����

    private GameObject poolObject;          // ������Ʈ Ǯ������ �����ϴ� ���� ������Ʈ ������
    private List<PoolItem> poolItemList;    // �����Ǵ� ��� ������Ʈ�� �����ϴ� ����Ʈ

    public int MaxCount => maxCount;        // �ܺο��� ���� ����Ʈ�� ��ϵǾ� �ִ� ������Ʈ ���� Ȯ���� ���� ������Ƽ
    public int ActiveCount => activeCount;  // �ܺο��� ���� Ȱ��ȭ �Ǿ� �ִ� ������Ʈ ���� Ȯ���� ���� ������Ƽ

    public List<PoolItem> PoolItemList => poolItemList; // �ܺο��� �д� �뵵�� ����� Ǯ�� ������Ʈ ����Ʈ


    // ������Ʈ�� �ӽ� �����Ǵ� ��ġ
    private Vector3 tempPosition = Vector3.zero;

    public MemoryPool(GameObject poolObject)
    {
        maxCount = 0;
        activeCount = 0;
        this.poolObject = poolObject;

        poolItemList = new List<PoolItem>();

        // InstantiateObjects();
    }

    /// <summary>
    /// increaseCount ������ ������Ʈ�� ����
    /// </summary>
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

    /// <summary>
    /// ���� ������(Ȱ�� / ��Ȱ��)�� ��� ������Ʈ�� ����
    /// </summary>
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

    /// <summary>
    /// poolItemList�� ����Ǿ� �ִ� ������Ʈ�� Ȱ��ȭ �ؼ� ���
    /// ���� ��� ������Ʈ�� ��� ���̸� InstantiateObjects()�� �߰� ����
    /// </summary>
    public GameObject ActivatePoolItem()
    {
        if (poolItemList == null)
        {
            Debug.Log("ActivatePoolItem() : poolItemList is null!!");
            return null;
        }

        // ���� �����ؼ� �����ϴ� ��� ������Ʈ ������ ���� Ȱ��ȭ ������ ������Ʈ ���� ��
        // ��� ������Ʈ�� Ȱ��ȭ ���� -> ���ο� ������Ʈ �ʿ�
        // TODO#: �� �κ��� �ʿ� ���� ���� ���� ���� ���ÿ��� ������ ������ �ѹ��� �� ���� �� ��Ȱ��ȭ �ϹǷ� ü�� �ǽ��� �� �ʿ������� ��� �߻� x
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


    /// <summary>
    /// ���� ����� �Ϸ�� ������Ʈ�� ��Ȱ��ȭ ���·� ����
    /// </summary>
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

    /// <summary>
    /// ���ӿ� ���� ��� ������Ʈ�� ��Ȱ��ȭ ���·� ����
    /// </summary>
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