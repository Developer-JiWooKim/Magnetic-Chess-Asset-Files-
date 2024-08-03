using System.Collections.Generic;
using UnityEngine;

public class MagnetBallSpawner : MonoBehaviour
{
    private MagnetBallMemoryPool magnetBallMemoryPool;


    private void Awake()
    {
        magnetBallMemoryPool = GetComponent<MagnetBallMemoryPool>();
    }

    /// <summary>
    /// 위치와 회전값을 받아 해당 위치에 마그넷볼을 스폰하는 함수
    /// </summary>
    public void SpawnMagnetBall(Vector3 pos, Quaternion rot)
    {
        magnetBallMemoryPool.ActivateMagnetBall(pos, rot);
    }

    public void DeactivateAllMagnetBall()
    {
        magnetBallMemoryPool.DeactivateAllMagnetBall();
    }

}
