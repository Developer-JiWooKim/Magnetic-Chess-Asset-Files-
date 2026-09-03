using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.Match;

namespace Assets.MyAssets.Scripts.Magnet
{
    public sealed class MagnetContact : MonoBehaviour
    {
        [SerializeField] private bool isContact;

        public bool IsContact => isContact;

        public GameObject spawnPoint;

        private void Awake() => Setup();
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
            if (collision.collider.CompareTag("Magnet"))
            {
                // 자석볼끼리 붙으면 그만큼 확정 대기 시간을 늘려준다.
                // TODO#: / 2 보다는 * 0.5f 가 더 낫지 않나?
                GameDirector.Instance.ExtendConfirmTime(GameManager.Instance.CurrentSetting.waitingTime / 2);


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
}
