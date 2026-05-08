using UnityEngine;

[CreateAssetMenu(menuName= "Strategy Game/Card Effects/Diagonal Attack")]

public class DiagonalAttack : CardEffect
{
    public int Damage= 6;

    public override bool Resolve(CardEffectContext context)
    {
        if (context.TargetUnit == null)
        {
            return false;
        }

        return context.DoDamage(context.TargetUnit, Damage);
    }
}
