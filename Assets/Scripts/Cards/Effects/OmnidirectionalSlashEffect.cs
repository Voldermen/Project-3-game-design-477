using UnityEngine;

[CreateAssetMenu(menuName= "Strategy Game/Card Effects/Omnidirectional Slash")]

public class OmnidirectionalSlashEffect : CardEffect
{
   public int Damage=4;

   public override bool Resolve(CardEffectContext context)
    {
        if (context.TargetUnit == null)
        {
            return false;
        }

        return context.DoDamage(context.TargetUnit, Damage);
    }
}
