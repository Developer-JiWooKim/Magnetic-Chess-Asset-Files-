using UnityEngine;
using Assets.MyAssets.Scripts.Core;
using Assets.MyAssets.Scripts.Match;

namespace Assets.MyAssets.Scripts.Magnet
{
    public sealed class MagnetContact : MonoBehaviour
    {
        [SerializeField] private bool isContact;

        public bool IsContact => isContact;

        /// <summary>
        /// 이 자석볼이 올라가 있는 스폰 지점. GameObject로 들고 있으면 해제할 때마다
        /// GetComponent가 필요해서 컴포넌트 타입 그대로 참조한다.
        /// </summary>
        public MagnetBallSpawnPoint spawnPoint;

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
                GameDirector.Instance.ExtendConfirmTime(GameManager.Instance.CurrentSetting.waitingTime * 0.5f);

                isContact = true;
            }
        }

        private void OnDisable()
        {
            if (spawnPoint != null)
            {
                spawnPoint.ChangeIsEmpty();
            }
            isContact = false;
        }

        private void OnEnable()
        {
            isContact = false;
        }
    }
}
