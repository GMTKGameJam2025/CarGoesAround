using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Camera sceneCamera;
    private Vector3 lastPosition;

    [SerializeField]
    private LayerMask placementLayerMask;

    public event Action OnClick, OnExit;

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
            lastPosition = hit.point;

        }
        return lastPosition;
    }

}
