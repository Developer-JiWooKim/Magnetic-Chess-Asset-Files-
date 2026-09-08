using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.MyAssets.Scripts.Magnet
{
    public sealed class MagnetBallSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _magnetBallPrefab;

        private ObjectPool<GameObject> _pool;

        /// <summary>
        /// 현재 판에 나와 있는 자석볼. ObjectPool&lt;T&gt;는 활성 객체를 열거하는 API가 없어서
        /// Get/Release 콜백에서 직접 관리한다.
        /// </summary>
        private readonly List<GameObject> _activeMagnetBalls = new();
        public IReadOnlyList<GameObject> ActiveMagnetBalls => _activeMagnetBalls;

        /// <summary>
        /// 위 목록과 같은 순서로 유지되는 MagnetContact 캐시. 턴이 끝날 때마다 자석볼 전체를
        /// 훑으며 GetComponent를 부르지 않도록, 풀에서 꺼낼 때 한 번만 찾아 둔다.
        /// </summary>
        private readonly List<MagnetContact> _activeMagnetContacts = new();
        public IReadOnlyList<MagnetContact> ActiveMagnetContacts => _activeMagnetContacts;

        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_magnetBallPrefab),
                actionOnGet: OnGetMagnetBall,
                actionOnRelease: OnReleaseMagnetBall,
                actionOnDestroy: magnetBall => Destroy(magnetBall));
        }

        private void OnGetMagnetBall(GameObject magnetBall)
        {
            _activeMagnetBalls.Add(magnetBall);
            _activeMagnetContacts.Add(magnetBall.GetComponent<MagnetContact>());

            magnetBall.SetActive(true);
        }

        private void OnReleaseMagnetBall(GameObject magnetBall)
        {
            // 두 목록은 같은 순서를 유지해야 하므로 인덱스를 찾아 함께 지운다.
            int index = _activeMagnetBalls.IndexOf(magnetBall);
            if (index >= 0)
            {
                _activeMagnetBalls.RemoveAt(index);
                _activeMagnetContacts.RemoveAt(index);
            }

            magnetBall.transform.position = Vector3.zero;
            magnetBall.SetActive(false);
        }

        public void SpawnMagnetBall(Vector3 pos, Quaternion rot)
        {
            GameObject magnetBall = _pool.Get();
            magnetBall.transform.SetPositionAndRotation(pos, rot);
        }

        public void DeactivateMagnetBall(GameObject magnetBall)
        {
            if (magnetBall == null)
            {
                return;
            }
            _pool.Release(magnetBall);
        }

        public void DeactivateAllMagnetBall()
        {
            // Release 콜백이 _activeMagnetBalls를 수정하므로 뒤에서부터 순회한다.
            for (int i = _activeMagnetBalls.Count - 1; i >= 0; i--)
            {
                _pool.Release(_activeMagnetBalls[i]);
            }
        }

        /// <summary>
        /// 판 시작 전, 필요한 개수만큼 자석볼을 미리 만들어 둔다.
        /// </summary>
        public void InstantiateMagnetBall(int magnetBallCount)
        {
            int shortage = magnetBallCount - _pool.CountAll;
            if (shortage <= 0)
            {
                return;
            }

            // ObjectPool에는 사전 생성 API가 없어서, 필요한 만큼 Get 했다가 곧바로 Release 한다.
            List<GameObject> warmedUp = new List<GameObject>(shortage);
            for (int i = 0; i < shortage; i++)
            {
                warmedUp.Add(_pool.Get());
            }
            foreach (GameObject magnetBall in warmedUp)
            {
                _pool.Release(magnetBall);
            }
        }
    }
}
