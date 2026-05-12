using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName= "Strategy Game/Card Effects/Black Hole")]
public class BlackHoleEffect : CardEffect
{
   public int Damage=10;

   [SerializeField] private GameObject blackHolePrefab;
   [SerializeField] private float damageDelay= 0.35f;
   [SerializeField] private float effectLifetime= 1.3f;
   [SerializeField] private float effectHeightOffset= 0.1f;

   public override bool Resolve(CardEffectContext context)
    {
        if (context==null || context.BoardState== null || context.GameManager == null)
        {
            return false;
        }

        BoardUnitState targetUnit= context.TargetUnit;

        if (targetUnit==null || targetUnit.Team != UnitTeam.Enemy)
        {
            return false;
        }

        context.GameManager.StartCoroutine(blackHoleDamage(context, targetUnit));
        return true;
    }

    private IEnumerator blackHoleDamage(CardEffectContext context, BoardUnitState targetUnit)
    {
        GameObject spawnedEffect= null;
        if (blackHolePrefab != null)
        {
            Vector3 effectPosition= context.GameManager.WorldPosition(context.TargetPosition);
            effectPosition += Vector3.up*effectHeightOffset;

            spawnedEffect= Object.Instantiate(blackHolePrefab, effectPosition, Quaternion.identity);
        }
        yield return new WaitForSeconds(damageDelay);

        if(context.BoardState != null && context.BoardState.UnitsById.ContainsKey(targetUnit.UnitId))
        {
            context.DoDamage(targetUnit, Damage);
        }
        yield return new WaitForSeconds(effectLifetime - damageDelay);

        if (spawnedEffect != null)
        {
            Object.Destroy(spawnedEffect);
        }
    }
}
