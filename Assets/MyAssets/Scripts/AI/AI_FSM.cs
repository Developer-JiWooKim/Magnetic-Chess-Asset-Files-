using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.MyAssets.Scripts.Magnet;

namespace Assets.MyAssets.Scripts.AI
{
    public sealed class AI_FSM : MonoBehaviour
    {
        [SerializeField]
        private GameObject magnetBallSpawnPoints;

        private List<MagnetBallSpawnPoint> spawnPointList;

        private List<Transform> emptyPointsTransform = new();
        private List<Transform> notEmptyPointsTransform = new();

        private List<int> final_index = new();

        private void Awake()
        {
            Setup();
        }
        private void Setup()
        {
            spawnPointList = magnetBallSpawnPoints.GetComponentsInChildren<MagnetBallSpawnPoint>().ToList();
        }
        private void CheckSpawnPoints()
        {
            emptyPointsTransform.Clear();
            notEmptyPointsTransform.Clear();
            spawnPointList.ForEach(point =>
            {
                if (point.IsEmpty)
                {
                    emptyPointsTransform.Add(point.gameObject.GetComponent<Transform>().transform);
                }
                else
                {
                    notEmptyPointsTransform.Add(point.gameObject.GetComponent<Transform>().transform);
                }
            });
        }

        private Vector3 DecideSpawnPoint()
        {
            float range = 2f;
            bool non_existent = false;

            final_index.Clear();
            Search_Close_MagnetBall(range);

            while (final_index.Count == 0)
            {

                if (range < 0.5f)
                {
                    non_existent = true;
                    break;
                }
                range -= 0.2f;
                Search_Close_MagnetBall(range);
            }

            int index = 0;


            if (non_existent)
            {
                index = Random.Range(0, emptyPointsTransform.Count);
            }

            else
            {
                index = final_index[Random.Range(0, final_index.Count)];
            }

            return emptyPointsTransform[index].position;
        }
        private void Search_Close_MagnetBall(float range)
        {
            final_index.Clear();

            float search_Range = range;
            bool isOk;

            for (int i = 0; i < emptyPointsTransform.Count; i++)
            {
                isOk = true;
                for (int j = 0; j < notEmptyPointsTransform.Count; j++)
                {
                    float distance = Vector3.Distance(emptyPointsTransform[i].position, notEmptyPointsTransform[j].position);
                    if (distance < search_Range)
                    {
                        isOk = false;
                    }
                }
                if (isOk)
                {
                    final_index.Add(i);
                }
            }
        }

        private Vector3 RandomSpawnPoint()
        {
            int randomIndex = Random.Range(0, emptyPointsTransform.Count);

            return emptyPointsTransform[randomIndex].position;
        }
        public void SpawnPoint_Initialize()
        {
            spawnPointList.ForEach(point => point.Initialize());
        }
        public Vector3 AIMagnetBallSpawnPoint()
        {
            CheckSpawnPoints();

            if (notEmptyPointsTransform.Count > 0)
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
