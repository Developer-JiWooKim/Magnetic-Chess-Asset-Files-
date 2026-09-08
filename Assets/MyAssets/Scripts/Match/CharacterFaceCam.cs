using UnityEngine;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class CharacterFaceCam : MonoBehaviour
    {
        [SerializeField] private Camera[] _cameras = new Camera[2];
        [SerializeField] private Transform[] _faceCharacters;

        // consts
        private const int FIRST_CAMERA_INDEX = 0;
        private const int SECOND_CAMERA_INDEX = 1;

        private void RandomTargetFollow()
        {
            int[] index = new int[2];
            index[FIRST_CAMERA_INDEX] = Random.Range(0, _faceCharacters.Length);
            index[SECOND_CAMERA_INDEX] = Random.Range(0, _faceCharacters.Length);

            while (index[FIRST_CAMERA_INDEX] == index[SECOND_CAMERA_INDEX])
            {
                index[SECOND_CAMERA_INDEX] = Random.Range(0, _faceCharacters.Length);
            }

            Vector3 position = _cameras[FIRST_CAMERA_INDEX].transform.position;
            position.x = _faceCharacters[index[FIRST_CAMERA_INDEX]].position.x;
            position.y = _faceCharacters[index[FIRST_CAMERA_INDEX]].position.y;

            _cameras[FIRST_CAMERA_INDEX].transform.position = position;

            position = _cameras[SECOND_CAMERA_INDEX].transform.position;
            position.x = _faceCharacters[index[SECOND_CAMERA_INDEX]].position.x;
            position.y = _faceCharacters[index[SECOND_CAMERA_INDEX]].position.y;

            _cameras[SECOND_CAMERA_INDEX].transform.position = position;
        }
        public void Initialize()
        {
            RandomTargetFollow();
        }
    }
}
