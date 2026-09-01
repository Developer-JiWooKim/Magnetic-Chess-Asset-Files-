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


    private void OnCollisionEnter(Collision collision)
    {
        if (isContact)
        {
            return;
        }
        if (collision.collider.tag == "Magnet")
        {

            GameDirector director = FindObjectOfType<GameDirector>();
            director.confirmTime += (GameManager.Instance.gameSetting.waitingTime / 2);
            Debug.Log("OnCollisionEnter()���� ���ӵ��͸� ã�� confirmTime�� " + (GameManager.Instance.gameSetting.waitingTime / 2) + "��ŭ �߰�");


            isContact = true;
        }
    }

    private void OnDisable()
    {
        if (spawnPoint != null)
        {
            spawnPoint.GetComponent<MagnetBallSpawnPoint>().ChangeIsEmpty();
        }
        isContact = false;
    }

    private void OnEnable()
    {
        isContact = false;
    }
}