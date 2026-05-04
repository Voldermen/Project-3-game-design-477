using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIRaycastDebugger : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(data, results);

        Debug.Log($"UI raycast hit count: {results.Count}");

        for (int i = 0; i < results.Count; i++)
        {
            Debug.Log($"UI hit {i}: {results[i].gameObject.name}", results[i].gameObject);
        }
    }
}