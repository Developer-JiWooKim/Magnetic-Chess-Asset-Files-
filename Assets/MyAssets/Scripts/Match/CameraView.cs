using System;
using System.Collections;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Match
{
    public sealed class CameraView : MonoBehaviour
    {
        [SerializeField] private Transform _quarterView;
        [SerializeField] private Transform _topView;
        [SerializeField] private Transform _startPointView;

        // const
        private const float MOVE_SPEED = 3f;

        public Transform QuarterView
        {
            get
            {
                if (_quarterView != null)
                {
                    return _quarterView;
                }
                else
                {
                    Debug.Log("QuarterView Transform is null");
                    return null;
                }
            }
        }
        public Transform TopView
        {
            get
            {
                if (_topView != null)
                {
                    return _topView;
                }
                else
                {
                    Debug.Log("_topView Transform is null");
                    return null;
                }
            }
        }
        public Transform StartPointView
        {
            get
            {
                if (_startPointView != null)
                {
                    return _startPointView;
                }
                else
                {
                    Debug.Log("_startPointView Transform is null");
                    return null;
                }
            }
        }

        private IEnumerator SmoothMoveCamera(Transform target, Action action = null)
        {
            while ((transform.position - target.position).magnitude > 0.05f)
            {
                transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * MOVE_SPEED);
                transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * MOVE_SPEED);
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
