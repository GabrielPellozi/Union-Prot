using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPause : PlayerStates
{
    private readonly int useIdleVariantHash = Animator.StringToHash("Use Idle Variant");

    private readonly int movementSpeedHash = Animator.StringToHash("Movement Speed");

    private readonly int onGroundHash = Animator.StringToHash("On Ground");

    public override void OnStateEnter()
    {
        Controller.Animator.SetBool(useIdleVariantHash, true);

        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        Controller.Animator.SetBool(useIdleVariantHash, false);

        Controller.Animator.SetFloat(movementSpeedHash, 0f);
    }

    public override void OnStateFixedUpdate()
    {
    }

    public override void OnStateUpdate()
    {
        Controller.Animator.SetBool(onGroundHash, Controller.OnGround());
    }
}
