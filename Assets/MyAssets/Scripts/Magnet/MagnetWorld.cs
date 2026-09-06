using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Magnet
{
    public sealed class MagnetWorld : MonoBehaviour
    {
        public float Permeability = 0.3f;
        public float MaxForce = 10000.0f;

        public bool IsActive;

        private float minMagnetForce = 5.0f;

        private const float FourPi = 4.0f * Mathf.PI;

        /// <summary>
        /// 두 자석이 완전히 겹쳤을 때 0으로 나눠 NaN이 물리에 들어가는 것을 막는 최소 거리(제곱).
        /// </summary>
        private const float MinSqrDistance = 0.0001f;

        /// <summary>
        /// transform.position / transform.parent는 네이티브 호출이라 O(n^2) 루프 안에서 읽으면
        /// 쌍마다 비용이 붙는다. FixedUpdate 시작에 한 번만 모아 두고 루프는 이 배열만 본다.
        /// 한 물리 프레임 안에서는 트랜스폼이 움직이지 않으므로 값은 동일하다.
        /// </summary>
        private Vector3[] cachedPositions = Array.Empty<Vector3>();
        private Transform[] cachedParents = Array.Empty<Transform>();

        private void Start() => Setup();
        private void Setup()
        {
            IsActive = false;
        }

        /// <summary>
        /// 길버트 힘. 원래는 |F| = μ·m1·m2 / (4π·d)를 구한 뒤 dir/d로 방향을 곱했는데,
        /// 두 식을 합치면 F = μ·m1·m2·dir / (4π·d²)이 되어 magnitude/normalized의 sqrt 두 번 없이
        /// sqrMagnitude만으로 같은 값을 얻는다.
        /// </summary>
        private Vector3 CalculateGilbertForce(Magnet magnet1, Vector3 m1_Pos, Magnet magnet2, Vector3 m2_Pos)
        {
            Vector3 dir = m2_Pos - m1_Pos;

            float sqrDistance = Mathf.Max(dir.sqrMagnitude, MinSqrDistance);

            float magnetForce = magnet1.MagnetForce * magnet2.MagnetForce;

            // 기존 코드가 (μ·m1·m2 / 4πd)·(dir/d)를 구한 뒤 다시 m1·m2를 곱했기 때문에
            // 자력이 제곱으로 들어간다. 동작을 바꾸지 않으려고 그대로 유지한다.
            float scale = Permeability * magnetForce * magnetForce / (FourPi * sqrDistance);

            if (magnet1.MagneticPole == magnet2.MagneticPole)
            {
                scale = -scale;
            }

            return scale * dir;
        }

        private void FixedUpdate()
        {
            if (IsActive == false)
            {
                return;
            }

            IReadOnlyList<Magnet> magnets = Magnet.ActiveMagnets;
            int count = magnets.Count;

            if (count < 2)
            {
                return;
            }

            if (cachedPositions.Length < count)
            {
                cachedPositions = new Vector3[count];
                cachedParents = new Transform[count];
            }

            for (int i = 0; i < count; i++)
            {
                Transform magnetTransform = magnets[i].transform;

                cachedPositions[i] = magnetTransform.position;
                cachedParents[i] = magnetTransform.parent;
            }

            float maxForceSqr = MaxForce * MaxForce;

            for (int i = 0; i < count; i++)
            {
                Magnet magnet_1 = magnets[i];
                if (magnet_1.RigidBody == null)
                {
                    continue;
                }

                Vector3 magnet_1_Pos = cachedPositions[i];
                Transform magnet_1_Parent = cachedParents[i];

                Vector3 accF = Vector3.zero;

                for (int j = 0; j < count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    Magnet magnet_2 = magnets[j];

                    if (magnet_2.MagnetForce < minMagnetForce)
                    {
                        continue;
                    }

                    if (magnet_1_Parent == cachedParents[j])
                    {
                        continue;
                    }

                    accF += CalculateGilbertForce(magnet_1, magnet_1_Pos, magnet_2, cachedPositions[j]);
                }

                if (accF.sqrMagnitude > maxForceSqr)
                {
                    accF = accF.normalized * MaxForce;
                }

                magnet_1.RigidBody.AddForceAtPosition(accF, magnet_1_Pos);
            }
        }
    }
}
