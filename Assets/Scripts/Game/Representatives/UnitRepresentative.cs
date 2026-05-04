using UnityEngine;

public class BoardUnitRepresentative : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;

    private UnitDatabase unitDatabase;
    private string currentDefinitionId;
    private GameObject currentModel;

    public void Initialize(UnitDatabase db)
    {
        unitDatabase = db;
    }

    public void Render(BoardUnitState state)
    {
        if (currentDefinitionId == state.UnitDefinitionId)
        {
            return;
        }

        currentDefinitionId = state.UnitDefinitionId;

        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        UnitDefinition def = unitDatabase.GetDefinition(state.UnitDefinitionId);

        if (def == null || def.ModelPrefab == null)
        {
            return;
        }

        currentModel = Instantiate(def.ModelPrefab, modelRoot);
        currentModel.transform.localPosition = Vector3.zero;
    }
}