using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Unit Definition")]
public class UnitDefinition : ScriptableObject
{
    public string UnitDefinitionId;
    public string DisplayName;

    public UnitTeam Team;
    public bool IsBase;

    public int MaxHealth = 10;

    public GameObject ModelPrefab;

    public EnemyBehavior EnemyBehavior;
}