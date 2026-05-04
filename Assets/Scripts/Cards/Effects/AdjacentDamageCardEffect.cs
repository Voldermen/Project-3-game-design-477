using UnityEngine;

[CreateAssetMenu(menuName = "Strategy Game/Card Effects/Adjacent Damage")]
public class AdjacentDamageCardEffect : CardEffect
{
    public int Damage = 1;

    public override bool Resolve(CardEffectContext context)
    {
        return context.DoDamageAt(context.TargetPosition, Damage);
    }
}