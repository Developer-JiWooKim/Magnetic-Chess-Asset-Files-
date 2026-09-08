using UnityEngine;

namespace Assets.MyAssets.Scripts.Magnet
{
    public sealed class MagnetBallSpawnPoint : MonoBehaviour
    {
        [SerializeField] private bool _isEmpty = true;

        public bool IsEmpty => _isEmpty;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Magnet"))
            {
                _isEmpty = false;

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
        public void ChangeIsEmpty() => _isEmpty = true;
        public void Initialize() => _isEmpty = true;
    }
}
