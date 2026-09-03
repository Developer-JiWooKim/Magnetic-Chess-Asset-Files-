using System.Collections.Generic;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Magnet
{
    public sealed class Magnet : MonoBehaviour
    {
        /// <summary>
        /// 활성화된 자석 목록. MagnetWorld가 FixedUpdate마다 씬 전체를 검색하지 않도록
        /// 각 Magnet이 스스로 등록/해제한다. 비활성 오브젝트는 들어가지 않으므로
        /// 기존 FindObjectsOfType&lt;Magnet&gt;()과 동일한 집합이 된다.
        /// </summary>
        private static readonly List<Magnet> activeMagnets = new List<Magnet>();
        public static IReadOnlyList<Magnet> ActiveMagnets => activeMagnets;

        public enum Pole
        {
            North,
            South
        }

        public float MagnetForce;
        public Pole MagneticPole;
        public Rigidbody RigidBody;

        private void OnEnable() => activeMagnets.Add(this);
        private void OnDisable() => activeMagnets.Remove(this);

        void OnDrawGizmos()
        {

        }
    }
}
