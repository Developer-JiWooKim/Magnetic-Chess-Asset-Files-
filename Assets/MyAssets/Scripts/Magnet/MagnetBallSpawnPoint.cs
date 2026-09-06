using UnityEngine;

namespace Assets.MyAssets.Scripts.Magnet
{
    public sealed class MagnetBallSpawnPoint : MonoBehaviour
    {
        [SerializeField] private bool isEmpty = true;

        public bool IsEmpty => isEmpty;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Magnet"))
            {
                isEmpty = false;

                if (other.TryGetComponent(out MagnetContact magnetContact))
                {
                    magnetContact.spawnPoint = this;
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Magnet"))
            {
                ChangeIsEmpty();
            }
        }
        public void ChangeIsEmpty() => isEmpty = true;
        public void Initialize() => isEmpty = true;
    }
}
