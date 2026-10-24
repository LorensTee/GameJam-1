using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -5f);
    [SerializeField] private bool useInitialOffset = false;
    [SerializeField] private float smoothSpeed = 10f;

    private void Awake()
    {
        if (useInitialOffset && target != null)
        {
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;
        
        if (smoothSpeed > 0f)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }
    }
}
