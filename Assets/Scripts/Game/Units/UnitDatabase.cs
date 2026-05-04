using System.Collections.Generic;
using UnityEngine;

public class UnitDatabase : MonoBehaviour
{
    [SerializeField] private List<UnitDefinition> unitDefinitions = new();

    private readonly Dictionary<string, UnitDefinition> definitionsById = new();

    private void Awake()
    {
        definitionsById.Clear();

        for (int i = 0; i < unitDefinitions.Count; i++)
        {
            UnitDefinition def = unitDefinitions[i];

            if (def == null || string.IsNullOrEmpty(def.UnitDefinitionId))
            {
                continue;
            }

            definitionsById[def.UnitDefinitionId] = def;
        }
    }

    public UnitDefinition GetDefinition(string id)
    {
        if (definitionsById.TryGetValue(id, out UnitDefinition def))
        {
            return def;
        }

        return null;
    }
}