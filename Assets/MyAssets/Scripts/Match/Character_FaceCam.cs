using UnityEngine;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class Character_FaceCam : MonoBehaviour
    {
        [SerializeField]
        private Camera[] cameras = new Camera[2];
        [SerializeField]
        private Transform[] face_Characters;


        private const int __INDEX_0 = 0;
        private const int __INDEX_1 = 1;

        private void Random_Target_Follow()
        {
            int[] index = new int[2];
            index[__INDEX_0] = Random.Range(0, face_Characters.Length);
            index[__INDEX_1] = Random.Range(0, face_Characters.Length);

            while (index[__INDEX_0] == index[__INDEX_1])
            {
                index[__INDEX_1] = Random.Range(0, face_Characters.Length);
            }

            Vector3 position = cameras[__INDEX_0].transform.position;
            position.x = face_Characters[index[__INDEX_0]].position.x;
            position.y = face_Characters[index[__INDEX_0]].position.y;

            cameras[__INDEX_0].transform.position = position;

            position = cameras[__INDEX_1].transform.position;
            position.x = face_Characters[index[__INDEX_1]].position.x;
            position.y = face_Characters[index[__INDEX_1]].position.y;

            cameras[__INDEX_1].transform.position = position;
        }
        public void Initialize()
        {
            Random_Target_Follow();
        }
    }
}
