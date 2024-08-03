using UnityEngine;

public class MagnetContact : MonoBehaviour
{
    [SerializeField]
    private bool isContact;

    public bool IsContact => isContact;

    public GameObject spawnPoint;

    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        isContact = false;
        spawnPoint = null;
    }


    /// <summary>
    /// MagnetBall끼리 부딪히면 접촉 상태로 변경
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (isContact)
        {
            // 이미 접촉된 상태면 시간 추가 X
            return;
        }
        if (collision.collider.tag == "Magnet")
        {
            //게임 관리자를 찾아 확인 시간을 추가 -두개가 동시에 호출되므로 시간을 반으로 나눔
           GameDirector director = FindObjectOfType<GameDirector>();
           director.confirmTime += (GameManager.Instance.gameSetting.waitingTime / 2);
            Debug.Log("OnCollisionEnter()에서 게임디렉터를 찾아 confirmTime을 " + (GameManager.Instance.gameSetting.waitingTime / 2) + "만큼 추가");


            isContact = true;
        }
    }

    // 비활성화 될 때 isContact 변수를 초기화 
    private void OnDisable()
    {
        if (spawnPoint != null) 
        {
            spawnPoint.GetComponent<MagnetBallSpawnPoint>().ChangeIsEmpty();
        }
        isContact = false;
    }

    // 활성화 될 때 isContact 변수를 초기화
    private void OnEnable()
    {
        isContact = false;
    }
}