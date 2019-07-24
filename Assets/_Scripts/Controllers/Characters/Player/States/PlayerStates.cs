//#define DEBUG_STATE

using UnityEngine;

public abstract class PlayerStates : CharacterStates<PlayerStates, PlayerController>
{
    public override void OnStateEnter()
    {
#if DEBUG_STATE
        Debug.Log(GetType().ToString());
#endif
    }
}