using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Magnet
{
    public sealed class MagnetBallSpawner : MonoBehaviour
    {
        private MagnetBallMemoryPool magnetBallMemoryPool;


        private void Awake()
        {
            magnetBallMemoryPool = GetComponent<MagnetBallMemoryPool>();
        }

        public void SpawnMagnetBall(Vector3 pos, Quaternion rot)
        {
            magnetBallMemoryPool.ActivateMagnetBall(pos, rot);
        }

        public void DeactivateAllMagnetBall()
        {
            magnetBallMemoryPool.DeactivateAllMagnetBall();
        }

    }
}
