using UnityEngine;

public class WarperGhostRepresentative : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Material ghostMaterial;

    private GameObject currentModel;

    public void Render(WarperGhostVisualState ghost, UnitDatabase unitDatabase)
    {
        if (ghost == null || unitDatabase == null)
        {
            return;
        }

        UnitDefinition definition = unitDatabase.GetDefinition(ghost.UnitDefinitionId);

        if (definition == null || definition.ModelPrefab == null)
        {
            return;
        }

        if (currentModel != null)
        {
            Destroy(currentModel);
        }

        currentModel = Instantiate(definition.ModelPrefab, modelRoot);
        currentModel.transform.localPosition = Vector3.zero;

        ApplyGhostMaterial();
    }

    private void ApplyGhostMaterial()
    {
        if (currentModel == null || ghostMaterial == null)
        {
            return;
        }

        Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = new Material[renderers[i].materials.Length];

            for (int j = 0; j < materials.Length; j++)
            {
                materials[j] = ghostMaterial;
            }

            renderers[i].materials = materials;
        }
    }
}

