using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Card")]
public class CardDefinition : ScriptableObject
{
    public string CardId;
    public string DisplayName;
    public int Cost;
    public CardPlayType PlayType;
    public UnitTeam RequiredActingUnitTeam;
    public CardTargetType TargetType;
    public CardTargetPattern TargetPattern;
    public CardEffect CardEffect;
}