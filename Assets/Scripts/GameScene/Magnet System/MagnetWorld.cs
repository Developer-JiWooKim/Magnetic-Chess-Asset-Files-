using UnityEngine;

public class MagnetWorld : MonoBehaviour
{
    public float Permeability = 0.3f;
    public float MaxForce = 10000.0f;

    public bool IsActive;               // 침투력 활성화 여부

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
        Vector3 m1_Pos = magnet1.transform.position;    // 자석 1의 위치
        Vector3 m2_Pos = magnet2.transform.position;    // 자석 2의 위치

        Vector3 dir = m2_Pos - m1_Pos;                  // 방향(direction) 구함

        float distance = dir.magnitude;                 // 거리

        // 침투력 * 자석1 전하의 힘 * 자석2의 전하의 힘
        float part0 = Permeability * magnet1.MagnetForce * magnet2.MagnetForce; 
        float part1 = 4 * Mathf.PI * distance;          // 4 * PI * 방향 벡터의 길이

        // 적용될 힘
        float force = (part0 / part1);

        if (magnet1.MagneticPole == magnet2.MagneticPole) // 전하가 동일하면(N/N or S/S)
            force = -force; // 힘의 작용을 반대로(같을 경우 서로 밀어냄, 전하가 다르면 서로 끌어당기게)

        return force * dir.normalized; // 힘 * 방향
    }

    private void FixedUpdate()
    {
        if (IsActive == false)
        {
            return;
        }

        Magnet[] magnets = FindObjectsOfType<Magnet>();

        for (int i = 0; i < magnets.Length; i++)
        {
            Magnet magnet_1 = magnets[i];
            if (magnet_1.RigidBody == null)
            {
                continue;
            }

            Rigidbody magnetRigidbody = magnet_1.RigidBody;

            Vector3 accF = Vector3.zero;

            for (int j = 0; j < magnets.Length; j++)
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