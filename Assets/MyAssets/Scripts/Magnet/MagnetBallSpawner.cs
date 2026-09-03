using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.MyAssets.Scripts.Magnet
{
    public sealed class MagnetBallSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject magnetBallPrefab;

        private ObjectPool<GameObject> pool;

        /// <summary>
        /// 현재 판에 나와 있는 자석볼. ObjectPool&lt;T&gt;는 활성 객체를 열거하는 API가 없어서
        /// Get/Release 콜백에서 직접 관리한다.
        /// </summary>
        private readonly List<GameObject> activeMagnetBalls = new();
        public IReadOnlyList<GameObject> ActiveMagnetBalls => activeMagnetBalls;

        private void Awake()
        {
            pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(magnetBallPrefab),
                actionOnGet: OnGetMagnetBall,
                actionOnRelease: OnReleaseMagnetBall,
                actionOnDestroy: magnetBall => Destroy(magnetBall));
        }

        private void OnGetMagnetBall(GameObject magnetBall)
        {
            activeMagnetBalls.Add(magnetBall);
            magnetBall.SetActive(true);
        }

        private void OnReleaseMagnetBall(GameObject magnetBall)
        {
            activeMagnetBalls.Remove(magnetBall);

            magnetBall.transform.position = Vector3.zero;
            magnetBall.SetActive(false);
        }

        public void SpawnMagnetBall(Vector3 pos, Quaternion rot)
        {
            GameObject magnetBall = pool.Get();
            magnetBall.transform.SetPositionAndRotation(pos, rot);
        }

        public void DeactivateMagnetBall(GameObject magnetBall)
        {
            if (magnetBall == null)
            {
                return;
            }
            pool.Release(magnetBall);
        }

        public void DeactivateAllMagnetBall()
        {
            // Release 콜백이 activeMagnetBalls를 수정하므로 뒤에서부터 순회한다.
            for (int i = activeMagnetBalls.Count - 1; i >= 0; i--)
            {
                pool.Release(activeMagnetBalls[i]);
            }
        }

        /// <summary>
        /// 판 시작 전, 필요한 개수만큼 자석볼을 미리 만들어 둔다.
        /// </summary>
        public void InstantiateMagnetBall(int magnetBallCount)
        {
            int shortage = magnetBallCount - pool.CountAll;
            if (shortage <= 0)
            {
                return;
            }

            // ObjectPool에는 사전 생성 API가 없어서, 필요한 만큼 Get 했다가 곧바로 Release 한다.
            List<GameObject> warmedUp = new List<GameObject>(shortage);
            for (int i = 0; i < shortage; i++)
            {
                warmedUp.Add(pool.Get());
            }
            foreach (GameObject magnetBall in warmedUp)
            {
                pool.Release(magnetBall);
            }
        }
    }
}
