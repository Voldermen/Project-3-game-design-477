using UnityEngine;

public class inputedActions : MonoBehaviour
{
    public static inputedActions Instance {get; private set;}
    public PlayerInputs.PlayerActionsActions input;
  

    private void Awake()
    {
        Instance = this;
         var inputs= new PlayerInputs();
        inputs.Enable();
        input= inputs.PlayerActions;
        input.Enable();

    } 
    
    public void DisableInput()
    {
        input.Disable();
    }

    public void EnableInput()
    {
        input.Enable();
    }
}