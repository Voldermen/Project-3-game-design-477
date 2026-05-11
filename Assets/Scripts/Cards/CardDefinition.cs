using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Card")]
public class CardDefinition : ScriptableObject
{
    public string CardId;
    public string DisplayName;
    public int Cost;

    [TextArea(3,6)]
    public string Description;
    public Sprite CardImage;
    public CardPlayType PlayType;
    public UnitTeam RequiredActingUnitTeam;
    public CardTargetType TargetType;
    public CardTargetPattern TargetPattern;
    public CardEffect CardEffect;
    public int TargetRange = 0;
}