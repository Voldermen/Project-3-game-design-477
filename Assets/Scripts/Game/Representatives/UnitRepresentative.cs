using UnityEngine;

public class BoardUnitRepresentative : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;

    private UnitDatabase unitDatabase;
    private string currentDefinitionId;
    private GameObject currentModel;
    [SerializeField] private float modelYawOffset;

    public void Initialize(UnitDatabase db)
    {
        unitDatabase = db;
    }

    public void Render(BoardUnitState state)
    {
        Debug.Log($"Rendering unit {state.UnitId}, def={state.UnitDefinitionId}, facing={state.FacingDirection}");
        if (currentDefinitionId != state.UnitDefinitionId)
        {
            currentDefinitionId = state.UnitDefinitionId;

            if (currentModel != null)
            {
                Destroy(currentModel);
            }

            UnitDefinition def = unitDatabase.GetDefinition(state.UnitDefinitionId);

            if (def != null && def.ModelPrefab != null)
            {
                currentModel = Instantiate(def.ModelPrefab, modelRoot);
                currentModel.transform.localPosition = Vector3.zero;
                //currentModel.transform.localRotation = Quaternion.identity;
            }
        }

        ApplyFacingRotation(state);
    }

    private void ApplyFacingRotation(BoardUnitState state)
    {
        if (modelRoot == null)
        {
            return;
        }

        Vector3 forward = DirectionToWorldForward(state.FacingDirection);

        if (forward == Vector3.zero)
        {
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(forward, Vector3.up);
        Quaternion offsetRotation = Quaternion.Euler(0f, modelYawOffset, 0f);

        modelRoot.localRotation = lookRotation * offsetRotation;
    }

    private Vector3 DirectionToWorldForward(Vector2Int direction)
    {
        if (direction == Vector2Int.up)
        {
            return Vector3.forward;
        }

        if (direction == Vector2Int.down)
        {
            return Vector3.back;
        }

        if (direction == Vector2Int.left)
        {
            return Vector3.left;
        }

        if (direction == Vector2Int.right)
        {
            return Vector3.right;
        }

        return Vector3.zero;
    }
}