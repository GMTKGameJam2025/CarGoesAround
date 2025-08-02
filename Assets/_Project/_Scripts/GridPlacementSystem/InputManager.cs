using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;
    private Vector3 _lastPosition;

    [SerializeField]
    private LayerMask placementLayerMask;

    public event Action OnClick, OnExit, OnRotate;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnClick?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExit?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            OnRotate?.Invoke();
        }
    }

    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, placementLayerMask))
        {
            _lastPosition = hit.point;

        }
        return _lastPosition;
    }
}
