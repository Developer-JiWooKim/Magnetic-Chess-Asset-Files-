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
    /// MagnetBall���� �ε����� ���� ���·� ����
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (isContact)
        {
            // �̹� ���˵� ���¸� �ð� �߰� X
            return;
        }
        if (collision.collider.tag == "Magnet")
        {
            //���� �����ڸ� ã�� Ȯ�� �ð��� �߰� -�ΰ��� ���ÿ� ȣ��ǹǷ� �ð��� ������ ����
           GameDirector director = FindObjectOfType<GameDirector>();
           director.confirmTime += (GameManager.Instance.gameSetting.waitingTime / 2);
            Debug.Log("OnCollisionEnter()���� ���ӵ��͸� ã�� confirmTime�� " + (GameManager.Instance.gameSetting.waitingTime / 2) + "��ŭ �߰�");


            isContact = true;
        }
    }

    // ��Ȱ��ȭ �� �� isContact ������ �ʱ�ȭ 
    private void OnDisable()
    {
        if (spawnPoint != null) 
        {
            spawnPoint.GetComponent<MagnetBallSpawnPoint>().ChangeIsEmpty();
        }
        isContact = false;
    }

    // Ȱ��ȭ �� �� isContact ������ �ʱ�ȭ
    private void OnEnable()
    {
        isContact = false;
    }
}