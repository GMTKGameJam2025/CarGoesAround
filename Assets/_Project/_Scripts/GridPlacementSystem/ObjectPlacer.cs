using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> placedGameObjects = new();

    public int PlaceObject(GameObject prefab, Vector3 position, float rotationAngle)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.position = position;

        // Rotate the child object, not the parent
        if (newObject.transform.childCount > 0)
        {
            Transform childTransform = newObject.transform.GetChild(0);
            childTransform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        }
        else
        {
            // If no children, rotate the object itself
            newObject.transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
        }

        placedGameObjects.Add(newObject);
        return placedGameObjects.Count - 1;
    }

    public GameObject GetPlacedObjectAt(int index)
    {
        if (index >= 0 && index < placedGameObjects.Count)
        {
            return placedGameObjects[index];
        }
        return null;
    }

    public void RemoveObjectAt(int gameObjectIndex)
    {
        if (placedGameObjects.Count <= gameObjectIndex || placedGameObjects[gameObjectIndex] == null)
            return;

        Destroy(placedGameObjects[gameObjectIndex]);
        placedGameObjects[gameObjectIndex] = null;
    }
}
