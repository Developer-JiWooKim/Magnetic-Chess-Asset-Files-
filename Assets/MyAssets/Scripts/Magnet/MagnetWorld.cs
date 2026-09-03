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

        private void Start()
        {
            Setup();
        }
        private void Setup()
        {
            IsActive = false;
        }
        Vector3 CalculateGilbertForce(Magnet magnet1, Magnet magnet2)
        {
            Vector3 m1_Pos = magnet1.transform.position;
            Vector3 m2_Pos = magnet2.transform.position;

            Vector3 dir = m2_Pos - m1_Pos;

            float distance = dir.magnitude;


            float part0 = Permeability * magnet1.MagnetForce * magnet2.MagnetForce;
            float part1 = 4 * Mathf.PI * distance;


            float force = (part0 / part1);

            if (magnet1.MagneticPole == magnet2.MagneticPole)
                force = -force;

            return force * dir.normalized;
        }

        private void FixedUpdate()
        {
            if (IsActive == false)
            {
                return;
            }

            IReadOnlyList<Magnet> magnets = Magnet.ActiveMagnets;

            for (int i = 0; i < magnets.Count; i++)
            {
                Magnet magnet_1 = magnets[i];
                if (magnet_1.RigidBody == null)
                {
                    continue;
                }

                Rigidbody magnetRigidbody = magnet_1.RigidBody;

                Vector3 accF = Vector3.zero;

                for (int j = 0; j < magnets.Count; j++)
                {
                    if (i == j)
                        continue;

                    Magnet magnet_2 = magnets[j];

                    if (magnet_2.MagnetForce < minMagnetForce)
                        continue;

                    if (magnet_1.transform.parent == magnet_2.transform.parent)
                        continue;

                    Vector3 force = CalculateGilbertForce(magnet_1, magnet_2);
                    float magnetForce = magnet_1.MagnetForce * magnet_2.MagnetForce;

                    accF += force * magnetForce;
                }

                if (accF.magnitude > MaxForce)
                {
                    accF = accF.normalized * MaxForce;
                }
                magnetRigidbody.AddForceAtPosition(accF, magnet_1.transform.position);
            }
        }
    }
}
