using System;

[Serializable]
public class EnergyState
{
    public int CurrentEnergy;
    public int MaxEnergy;

    public void SetEnergy(int amount)
    {
        MaxEnergy = amount;
        CurrentEnergy = amount;
    }

    public bool CanSpend(int amount)
    {
        return CurrentEnergy >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (!CanSpend(amount)) return false;
        
        CurrentEnergy -= amount;
        return true;
    }

    public void Refund(int amount)
    {
        CurrentEnergy += amount;

        if (CurrentEnergy > MaxEnergy) CurrentEnergy = MaxEnergy;
        
    }

    public EnergyState Clone()
    {
        return new EnergyState
        {
            CurrentEnergy = CurrentEnergy,
            MaxEnergy = MaxEnergy
        };
    }
}