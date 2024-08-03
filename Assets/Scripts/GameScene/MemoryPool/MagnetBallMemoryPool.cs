using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetBallMemoryPool : MonoBehaviour
{
    [SerializeField]
    private GameObject magnetBallPrefab; // 마그넷볼 오브젝트

    private MemoryPool magnetBallMemoryPool; // 마그넷볼 메모리풀, 생성, 활성 / 비활성 관리(풀링)

    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        magnetBallMemoryPool = new MemoryPool(magnetBallPrefab);
    }

    // 위치, 회전 값을 매개변수로 받아 해당 위치에 마그넷 볼을 생성
    public void ActivateMagnetBall(Vector3 pos, Quaternion rot)
    {
        // GameObject item = memoryPool.ActivatePoolItem();
        //item.transform.position = pos;
        //item.transform.rotation = rot;

        //item.GetComponent<Magnet>();

        GameObject item = magnetBallMemoryPool.ActivatePoolItem(); // 풀 아이템 리스트에 들어있는 오브젝트 중 비활성화된걸 활성화 시켜서 아이템 변수에 저장
        // 위치, 회전 값 설정
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

    // 게임 옵션을 지정하고 최대 피스 개수가 정해지면 게임 디렉터에서 체스 피스들을 미리 생성(풀링)하기 위해 이 함수를 호출함
    public void InstantiateMagnetBall(int magnetBallCount)
    {
        // 풀 아이템 리스트 개수가 얻어온 숫자보다 크면 생성 x
        if (magnetBallMemoryPool.PoolItemList.Count >= magnetBallCount)
        {
            return;
        }
        // 풀 아이템 리스트 개수가 얻어온 숫자보다 작으면 그 차이 만큼 생성
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
