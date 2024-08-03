using UnityEngine;

public class MagnetBallSpawnPoint : MonoBehaviour
{
    [SerializeField]
    private bool isEmpty = true;
    public bool IsEmpty { get { return isEmpty; } }

    private void OnTriggerEnter(Collider other)
    {
        // 태그가 마그넷인 콜라이더와 닿으면 현재 비어있지 않은 상태로 바꿈
        if (other.tag == "Magnet")
        {
            isEmpty = false;
            other.gameObject.GetComponent<MagnetContact>().spawnPoint = this.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        // 태그가 마그넷인 콜라이더와 떨어지면 빈 상태로 바꿈
        if (other.tag == "Magnet")
        {
            ChangeIsEmpty();
        }
    }
    public void ChangeIsEmpty()
    {
        isEmpty = true;
    }
    public void Initialize()
    {
        isEmpty = true;
    }
}
