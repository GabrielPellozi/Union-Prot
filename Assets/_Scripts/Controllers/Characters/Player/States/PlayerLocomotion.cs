using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerLocomotion : PlayerStates
{
    private readonly float dampTime = 0.15f;
    
    private readonly int startJumpHash = Animator.StringToHash("Base Layer.Jump.Start Jump");
    
    private readonly int movementHash = Animator.StringToHash("Base Layer.Walk/Running");
    
    private readonly int onGroundHash = Animator.StringToHash("On Ground");
    
    private readonly int movementSpeedHash = Animator.StringToHash("Movement Speed");
    
    private readonly float cooldownEvading = Time.time + 0.1f;

    private float xMovement, zMovement;

    public override void OnStateEnter()
    {
        if (Controller.OnGround())
            Controller.Model.position = Controller.Model.position
                .With(y: Controller.StickToGroundPosition(Controller.Model.position.y));

        base.OnStateEnter();
    }

    public override void OnStateExit()
    {

    }

    public override void OnStateUpdate()
    {
        if (Controller.Input.PrimaryAttack || Controller.Input.SecondaryAttack)
        {
            Controller.CurrentAttackType = (Controller.Input.PrimaryAttack) ? AttackType.Primary : AttackType.Secondary;
            Controller.ChangeState(Controller.States[typeof(PlayerAttack)], Controller, true);
        }

        if (Controller.Input.Dodge && Time.time > cooldownEvading)
            Controller.ChangeState(Controller.States[typeof(PlayerEvasion)], Controller, true);

        UpdateAnimator();
    }

    private void Rotate()
    {
        Vector3 lookPosX =
            (Controller.MyCamera.transform.right.With(y: 0).normalized * Controller.Input.HorizontalAxis);

        Vector3 lookPosZ =
            (Controller.MyCamera.transform.forward.With(y: 0).normalized * Controller.Input.VerticalAxis);

        Vector3 finalLook = lookPosX.With(y: 0f) + lookPosZ.With(y: 0f);

        if (Mathf.Abs(finalLook.magnitude) > 0f)
            Controller.Model.transform.rotation = Quaternion.RotateTowards
                    (Controller.Model.transform.rotation,
                    Quaternion.LookRotation(finalLook),
                    Controller.MovementData.RotateSpeed * Time.deltaTime);
    }

    private void Move()
    {
        Vector3 horizontal = xMovement *
            (Controller.MyCamera.transform.right.With(y: 0).normalized * Controller.Input.HorizontalAxis);

        Vector3 forward = zMovement *
            (Controller.MyCamera.transform.forward.With(y: 0).normalized * Controller.Input.VerticalAxis);

        Vector3 movement = Vector3.ClampMagnitude(
            (horizontal + forward) * Controller.MovementData.MovementSpeed, Controller.MovementData.MovementSpeed);

        Vector3 final = Controller.Model.position + movement * Time.fixedDeltaTime;

        Controller.FeetIK.enabled = Controller.OnGround() && 
            Controller.Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == movementHash;

        Controller.Model.position = (Controller.OnGround() && movement.magnitude.Round().Abs() > 0f) ?
            final.With(y: Controller.StickToGroundPosition(Controller.Model.position.y)) : final;
        //Controller.Rigid.velocity = movement.With(y: Controller.Rigid.velocity.y);
        //Controller.Rigid.MovePosition(final);
    }

    private void CheckForWall()
    {
        if (!Controller.OnGround())
        {
            if (Controller.Input.VerticalAxisRaw.Abs() > 0f)
            {
                Vector3 startLocation = Controller.Model.position +
                    ((Controller.MyCamera.transform.forward.With(y: 0f) * .25f) *
                    Controller.Input.VerticalAxisRaw);

                zMovement =
                    (Physics.CheckCapsule(startLocation, startLocation + Vector3.up, .15f, Controller.MovementData.GroundLayer)) ?
                    0f : 1f;
            }

            if (Controller.Input.HorizontalAxisRaw.Abs() > 0f)
            {
                Vector3 startLocation = Controller.Model.position +
                    ((Controller.MyCamera.transform.right.With(y: 0f) * .25f) *
                    Controller.Input.HorizontalAxisRaw);

                xMovement =
                    (Physics.CheckCapsule(startLocation, startLocation + Vector3.up, .15f, Controller.MovementData.GroundLayer)) ?
                    0f : 1f;
            }
        }
        else
        {
            if (xMovement.Abs() <= 0f || zMovement.Abs() <= 0f)
                xMovement = zMovement = 1f;
        }
    }

    private void Jump()
    {
        Controller.FeetIK.enabled = false;
        Controller.Animator.Play(startJumpHash);
        Controller.Rigid.velocity = Vector3.up * Controller.MovementData.JumpPower;
    }

    private void UpdateAnimator()
    {
        float final = Controller.Input.HorizontalAxis.Abs() + Controller.Input.VerticalAxis.Abs();
        final = Mathf.InverseLerp(0, 1, final);

        Controller.Animator.SetFloat(movementSpeedHash, final, dampTime, Time.deltaTime);

        Controller.Animator.SetBool(onGroundHash, Controller.OnGround());
    }

    public override void OnStateFixedUpdate()
    {
        Rotate();

        Move();

        CheckForWall();

        if (Controller.Input.JumpDown && Controller.OnGround())
            Jump();
    }
}