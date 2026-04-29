using UnityEngine;

public abstract class CardEffect : ScriptableObject
{
    public abstract bool Resolve(CardEffectContext context);
}