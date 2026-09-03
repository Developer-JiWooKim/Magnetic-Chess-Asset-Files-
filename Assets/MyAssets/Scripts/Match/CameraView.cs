using System;
using System.Collections;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class CameraView : MonoBehaviour
    {
        [SerializeField]
        private Transform quarterView;
        [SerializeField]
        private Transform topView;
        [SerializeField]
        private Transform startPointView;

        private const float __MOVE_SPEED = 3f;

        public Transform QuarterView_tr
        {
            get
            {
                if (quarterView != null)
                {
                    return quarterView;
                }
                else
                {
                    Debug.Log("QuarterView Transform is null");
                    return null;
                }
            }
        }
        public Transform TopView_tr
        {
            get
            {
                if (topView != null)
                {
                    return topView;
                }
                else
                {
                    Debug.Log("topView Transform is null");
                    return null;
                }
            }
        }
        public Transform StartPointView_tr
        {
            get
            {
                if (startPointView != null)
                {
                    return startPointView;
                }
                else
                {
                    Debug.Log("startPointView Transform is null");
                    return null;
                }
            }
        }

        public enum E_CameraView
        {
            QuarterView,
            TopView,
        }

        private IEnumerator SmoothMoveCamera(Transform target, Action action = null)
        {
            while ((transform.position - target.position).magnitude > 0.05f)
            {
                transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * __MOVE_SPEED);
                transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * __MOVE_SPEED);
                yield return null;
            }
            if (action != null)
            {
                action();
            }
        }
        public void ChangeCameraView(Transform target, Action action = null)
        {
            StartCoroutine(SmoothMoveCamera(target, action));
        }
    }
}
