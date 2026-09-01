using System.Collections.Generic;
using UnityEngine;

public class MagnetBallSpawner : MonoBehaviour
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
