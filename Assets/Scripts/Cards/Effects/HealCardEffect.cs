using UnityEngine;

[CreateAssetMenu(menuName= "Strategy Game/Card Effects/Heal Unit")]

public class HealCardEffect : CardEffect
{
   public int Healing= 3;

   public override bool Resolve(CardEffectContext context)
    {
        BoardUnitState targetUnit= context.TargetUnit; // selects the unit that is clicked on.

        if (targetUnit == null) // if there is not a unit then do not heal.
        {
            return false;
        }
        return context.DoHeal(targetUnit, Healing); // if there is a unit then heal it. it uses DoHeal from CardEffectContext.
    }
}
