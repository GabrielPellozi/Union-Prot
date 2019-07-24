using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerEvasion : PlayerStates
{
    readonly int rollHash = Animator.StringToHash("Evasion Layer.Roll");

    readonly int dashHash = Animator.StringToHash("Evasion Layer.Dash");

    readonly int noEvadeHash = Animator.StringToHash("Evasion Layer.No Evading");

    float timeToFinishEvading = Time.time + 1f;

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        Controller.FeetIK.enabled = false;

        Controller.Animator.Play((Controller.OnGround()) ? rollHash : dashHash);

        Rotate();
        Evade();

        timeToFinishEvading = Time.time + Controller.MovementData.EvasionTime;
    }

    public override void OnStateExit()
    {
        Controller.Rigid.velocity = Vector3.zero;

        Controller.FeetIK.enabled = true;

        Controller.Animator.CrossFade(noEvadeHash, 0.5f);
    }

    private void Rotate()
    {
        Vector3 lookPosX = Controller.MyCamera.transform.right * Controller.Input.HorizontalAxis;
        Vector3 lookPosZ = Controller.MyCamera.transform.forward * Controller.Input.VerticalAxis;

        Vector3 finalLook = lookPosX.With(y: 0f) + lookPosZ.With(y: 0f);

        if (finalLook.magnitude.Abs() > 0f)
            Controller.Model.transform.rotation = Quaternion.LookRotation(finalLook);
    }

    private void Evade()
    {
        Controller.Rigid.velocity = (Controller.OnGround()) ?
            (Controller.Model.forward * Controller.MovementData.EvasionSpeed) :
            (Controller.Model.forward * Controller.MovementData.EvasionSpeed) + (Controller.Model.up * 2.5f);
    }

    public override void OnStateFixedUpdate()
    {
        StickToGround();
    }

    public override void OnStateUpdate()
    {
        if (Time.time > timeToFinishEvading)
            Controller.ChangeState(Controller.States[typeof(PlayerLocomotion)], Controller, true);
    }

    private void StickToGround()
    {
        if (Physics.Raycast(Controller.Model.position, -Vector3.up, out RaycastHit hit, .25f, Controller.MovementData.GroundLayer))
        {
            Vector3 stickPosition = (Controller.Model.position + Controller.Rigid.velocity * Time.fixedDeltaTime);

            Controller.Model.position = stickPosition.With(y: Controller.Model.position.y - hit.distance);
        }
    }
}