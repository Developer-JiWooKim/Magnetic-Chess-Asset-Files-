using System;
using System.Collections;
using UnityEngine;

public class CameraView : MonoBehaviour
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

    /// <summary>
    /// 부드러운 카메라 이동 코루틴
    /// </summary>
    private IEnumerator SmoothMoveCamera(Transform target, Action action = null)
    {
        // 현재 위치와 목표 위치의 거리가 0.05f이하면 이동을 종료
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
    /// <summary>
    /// 카메라 시점 변경 함수
    /// </summary>
    public void ChangeCameraView(Transform target, Action action = null)
    {
        StartCoroutine(SmoothMoveCamera(target, action));
    }
}

