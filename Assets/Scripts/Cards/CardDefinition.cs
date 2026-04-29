using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Card")]
public class CardDefinition : ScriptableObject
{
    public string CardId;
    public string DisplayName;
    public int Cost;
    public CardTargetType TargetType;
    public CardEffect CardEffect;
}