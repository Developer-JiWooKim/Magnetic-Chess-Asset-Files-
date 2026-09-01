using UnityEngine;

public class MagnetBallSpawnPoint : MonoBehaviour
{
    [SerializeField]
    private bool isEmpty = true;
    public bool IsEmpty { get { return isEmpty; } }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Magnet")
        {
            isEmpty = false;
            other.gameObject.GetComponent<MagnetContact>().spawnPoint = this.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {

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
