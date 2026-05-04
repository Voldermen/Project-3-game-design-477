using TMPro;
using UnityEngine;

public class EnergyWidget : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text energyText;

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void Show(EnergyState energyState)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        Refresh(energyState);
    }

    public void Refresh(EnergyState energyState)
    {
        if (energyText == null) return;
        if (energyState == null)
        {
            energyText.text = "0 / 0";
            return;
        }
        energyText.text = $"{energyState.CurrentEnergy} / {energyState.MaxEnergy}";
    }
}