using UnityEngine;

public class BoardUnitRepresentative : MonoBehaviour
{
     [SerializeField] private Transform modelRoot;

    [Header("Buff Visual")]
    [SerializeField] private Material strengthGlowMaterial;
    [SerializeField] private float buffFlashSpeed= 4f; 

    private UnitDatabase unitDatabase;
    private string currentDefinitionId;
    private GameObject currentModel;

    private Renderer[] currentRenderers;
    private Material[][] originalMaterials;
    private bool strengthVisualActive;
    private BoardUnitState lastState;

    private void Update()
    {
        if (lastState != null)
        {
            UpdateStrengthVisual(lastState);
        }
    }

    public void Initialize(UnitDatabase database)
    {
        unitDatabase= database;
    }

    public void Render(BoardUnitState state)
    {
         lastState= state;
        if (currentDefinitionId!= state.UnitDefinitionId)
        {
           

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

            CacheOriginalMaterials();

            strengthVisualActive = false;
        }

        UpdateStrengthVisual(state);
    }

    private void CacheOriginalMaterials()
    {
        if (currentModel == null)
        {
            currentRenderers = null;
            originalMaterials = null;
            return;
        }

        currentRenderers = currentModel.GetComponentsInChildren<Renderer>(true);
        originalMaterials = new Material[currentRenderers.Length][];

        for (int i = 0; i < currentRenderers.Length; i++)
        {
            originalMaterials[i] = currentRenderers[i].materials;
        }
    }

    private void UpdateStrengthVisual(BoardUnitState state)
    {
        if (currentRenderers == null || originalMaterials == null)
        {
            return;
        }

        bool shouldShowStrength = state.Team == UnitTeam.Friendly && state.strengthTurnsRemaining > 0;

        if (shouldShowStrength)
        {
            strengthVisualActive = true;
            FlashStrengthMaterial();
        }
        else if (strengthVisualActive)
        {
            RestoreOriginalMaterials();
            strengthVisualActive = false;
        }
    }

    private void ApplyStrengthGlowMaterial()
    {
        if (strengthGlowMaterial== null)
        {
            return;
        }

        for (int i = 0; i < currentRenderers.Length; i++)
        {
            Material[] glowMaterials = new Material[currentRenderers[i].materials.Length];

            for (int j = 0; j < glowMaterials.Length; j++)
            {
                glowMaterials[j] = strengthGlowMaterial;
            }

            currentRenderers[i].materials = glowMaterials;
        }
    }

    private void RestoreOriginalMaterials()
    {
        if (currentRenderers ==null || originalMaterials == null)
        {
            return;
        }

        for (int i = 0; i< currentRenderers.Length; i++)
        {
            if (currentRenderers[i]!= null && originalMaterials[i] != null)
            {
                currentRenderers[i].materials = originalMaterials[i];
            }
        }
    }

    private void FlashStrengthMaterial()
    {
        if (strengthGlowMaterial== null)
        {
            return;
        }
        float flash= Mathf.PingPong(Time.time* buffFlashSpeed, 1f);

        bool useGlow= flash >0.5f;

        for (int i=0; i< currentRenderers.Length; i++)
        {
            if (currentRenderers[i] == null)
            {
                continue;
            }

            if (useGlow)
            {
                Material[] glowMaterials= new Material[currentRenderers[i].materials.Length];

                for (int j=0; j< glowMaterials.Length; j++)
                {
                    glowMaterials[j]= strengthGlowMaterial;
                }

                currentRenderers[i].materials= glowMaterials;
            }
            else
            {
                currentRenderers[i].materials= originalMaterials[i];
            }
        }
    }
}