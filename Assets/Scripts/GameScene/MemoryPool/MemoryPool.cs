using System.Collections.Generic;
using UnityEngine;

public class MemoryPool
{
    public class PoolItem
    {
        public bool isActive;       // "gameObject"의 활성 / 비활성화 정보
        public GameObject gameObject;     // 화면에 보이는 실제 오브젝트
    }

    //private int increaseCount = 5;          // 오브젝트가 부족할 때 Instantiate()로 추가 생성되는 오브젝트 개수
    private int maxCount;                   // 현재 리스트에 등록되어 있는 오브젝트 개수
    private int activeCount;                // 현재 게임에 사용되고 있는 오브젝트 개수

    private GameObject poolObject;          // 오브젝트 풀링에서 관리하는 게임 오브젝트 프리팹
    private List<PoolItem> poolItemList;    // 관리되는 모든 오브젝트를 저장하는 리스트

    public int MaxCount => maxCount;        // 외부에서 현재 리스트에 등록되어 있는 오브젝트 개수 확인을 위한 프로퍼티
    public int ActiveCount => activeCount;  // 외부에서 현재 활성화 되어 있는 오브젝트 개수 확인을 위한 프로퍼티

    public List<PoolItem> PoolItemList => poolItemList; // 외부에서 읽는 용도로 사용할 풀링 오브젝트 리스트


    // 오브젝트가 임시 보관되는 위치
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
    /// increaseCount 단위로 오브젝트를 생성
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
    /// 현재 관리중(활성 / 비활성)인 모든 오브젝트를 삭제
    /// </summary>
    public void DestroyObjects()
    {
        if (poolItemList == null)
        {
            Debug.Log("DestroyObjects() : poolItemList is null!!");
            return;
        }

        poolItemList.ForEach(poolItem => GameObject.Destroy(poolItem.gameObject));
        poolItemList.Clear(); // 리스트 초기화
    }

    /// <summary>
    /// poolItemList에 저장되어 있는 오브젝트를 활성화 해서 사용
    /// 현재 모든 오브젝트가 사용 중이면 InstantiateObjects()로 추가 생성
    /// </summary>
    public GameObject ActivatePoolItem()
    {
        if (poolItemList == null)
        {
            Debug.Log("ActivatePoolItem() : poolItemList is null!!");
            return null;
        }

        // 현재 생성해서 관리하는 모든 오브젝트 개수와 현재 활성화 상태인 오브젝트 개수 비교
        // 모든 오브젝트가 활성화 상태 -> 새로운 오브젝트 필요
        // TODO#: 이 부분이 필요 없을 수도 있음 게임 세팅에서 설정한 값으로 한번에 다 생성 후 비활성화 하므로 체스 피스가 더 필요해지는 경우 발생 x
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
    /// 현재 사용이 완료된 오브젝트를 비활성화 상태로 설정
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
    /// 게임에 사용된 모든 오브젝트를 비활성화 상태로 설정
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