using System.Collections.Generic;
using UnityEngine;
using Assets.MyAssets.Scripts.Magnet;

namespace Assets.MyAssets.Scripts.AI
{
    public sealed class AIFSM : MonoBehaviour
    {
        [SerializeField] private GameObject _magnetBallSpawnPoints;

        private MagnetBallSpawnPoint[] _spawnPointList;

        private List<Transform> _emptyPointsTransform = new();
        private List<Transform> _notEmptyPointsTransform = new();

        private List<int> _finalIndex = new();

        private void Awake() => Setup();
        private void Setup()
        {
            _spawnPointList = _magnetBallSpawnPoints.GetComponentsInChildren<MagnetBallSpawnPoint>();
        }

        private void CheckSpawnPoints()
        {
            _emptyPointsTransform.Clear();
            _notEmptyPointsTransform.Clear();

            for (int i = 0; i < _spawnPointList.Length; i++)
            {
                MagnetBallSpawnPoint point = _spawnPointList[i];

                // 예전에는 point.gameObject.GetComponent<Transform>().transform 이었는데,
                // GetComponent<Transform>()도 .transform도 같은 것을 가리키므로 point.transform이면 된다.
                if (point.IsEmpty)
                {
                    _emptyPointsTransform.Add(point.transform);
                }
                else
                {
                    _notEmptyPointsTransform.Add(point.transform);
                }
            }
        }

        private Vector3 DecideSpawnPoint()
        {
            float range = 2f;
            bool nonExistent = false;

            _finalIndex.Clear();
            SearchCloseMagnetBall(range);

            while (_finalIndex.Count == 0)
            {

                if (range < 0.5f)
                {
                    nonExistent = true;
                    break;
                }
                range -= 0.2f;
                SearchCloseMagnetBall(range);
            }

            int index = 0;


            if (nonExistent)
            {
                index = Random.Range(0, _emptyPointsTransform.Count);
            }

            else
            {
                index = _finalIndex[Random.Range(0, _finalIndex.Count)];
            }

            return _emptyPointsTransform[index].position;
        }

        private void SearchCloseMagnetBall(float range)
        {
            _finalIndex.Clear();

            float searchRange = range;
            bool isOk;

            for (int i = 0; i < _emptyPointsTransform.Count; i++)
            {
                isOk = true;
                for (int j = 0; j < _notEmptyPointsTransform.Count; j++)
                {
                    float distance = Vector3.Distance(_emptyPointsTransform[i].position, _notEmptyPointsTransform[j].position);
                    if (distance < searchRange)
                    {
                        isOk = false;
                    }
                }
                if (isOk)
                {
                    _finalIndex.Add(i);
                }
            }
        }

        private Vector3 RandomSpawnPoint()
        {
            int randomIndex = Random.Range(0, _emptyPointsTransform.Count);

            return _emptyPointsTransform[randomIndex].position;
        }

        public void SpawnPointInitialize()
        {
            for (int i = 0; i < _spawnPointList.Length; i++)
            {
                _spawnPointList[i].Initialize();
            }
        }

        public Vector3 AIMagnetBallSpawnPoint()
        {
            CheckSpawnPoints();

            if (_notEmptyPointsTransform.Count > 0)
            {
                return DecideSpawnPoint();
            }
            else
            {
                return RandomSpawnPoint();
            }
        }
    }
}
