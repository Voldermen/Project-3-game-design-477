using TMPro;
using UnityEngine;

public class UnitHealthHoverWidget : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Camera targetCamera;

    public void Show(BoardUnitState unitState, Vector3 worldPosition)
    {
        gameObject.SetActive(true);
        transform.position = worldPosition;

        if (healthText != null)
        {
            healthText.text = $"{unitState.Health} / {unitState.MaxHealth}, {unitState.UnitDefinitionId}";
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            transform.forward = targetCamera.transform.forward;
        }
    }
}