using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerAttack : PlayerStates
{
    readonly string lightAttackName = "Attack Layer.Style.Primary Attack #";
    readonly string aerialLightAttackName = "Aerial Attack Layer.Style.Aerial Primary Attack #";

    readonly string heavyAttackName = "Attack Layer.Style.Secondary Attack #";
    readonly string aerialHeavyAttackName = "Aerial Attack Layer.Style.Aerial Secondary Attack #";

    readonly int attackSpeedHash = Animator.StringToHash("Attack Speed");

    readonly int notAttackingHash = Animator.StringToHash("Attack Layer.Not Attacking");
    readonly int notAerialAttackingHash = Animator.StringToHash("Aerial Attack Layer.Not Aerial Attacking");

    private string currentAttackName;

    private int attackLayer;

    private float attackAgainIn;

    private bool automaticAttack;

    private AttackType currentAttackType;

    public override void OnStateEnter()
    {
        Controller.AttackIndex = 0;

        Controller.FeetIK.enabled = false;

        Attack();

        base.OnStateEnter();
    }

    public override void OnStateExit()
    {
        Controller.FeetIK.enabled = true;

        Controller.AnimationsEvents.CloseAllAttacks();

        Controller.Animator.applyRootMotion = false;

        Controller.Animator.CrossFade(notAttackingHash, .25f);
        Controller.Animator.CrossFade(notAerialAttackingHash, .25f);
    }

    private void Rotate()
    {
        Vector3 lookPosX = Controller.MyCamera.transform.right * Controller.Input.HorizontalAxis;
        Vector3 lookPosZ = Controller.MyCamera.transform.forward * Controller.Input.VerticalAxis;

        Vector3 finalLook = lookPosX.With(y: 0f) + lookPosZ.With(y: 0f);

        if (finalLook.magnitude.Abs() > 0f)
            Controller.Model.transform.rotation = Quaternion.LookRotation(finalLook);
    }

    private void Attack()
    {
        Rotate();

        currentAttackType = Controller.CurrentAttackType;

        Controller.AttackIndex++;

        Controller.AnimationsEvents.SetDamageBySequence((AttackSequence)Controller.AttackIndex);
        Controller.AnimationsEvents.CloseAllAttacks();

        attackLayer = (Controller.OnGround()) ? 1 : 2;

        ChangeAttackName(Controller.CurrentAttackStyle, Controller.CurrentAttackType);

        Controller.Animator.SetFloat(attackSpeedHash, Controller.GetCurrentAttackData
            (Controller.CurrentAttackType, Controller.CurrentAttackStyle, Controller.AttackIndex, Controller.OnGround()).AttackSpeed);

        attackAgainIn = Controller.GetCurrentAttackData
            (Controller.CurrentAttackType, Controller.CurrentAttackStyle, Controller.AttackIndex, Controller.OnGround()).AttackAgainIn;

        automaticAttack = false;

        Controller.Animator.CrossFade(AttackHash(), .25f);
    }

    private void ChangeAttackName(AttackStyle attackStyle, AttackType attackType)
    {
        currentAttackName = (Controller.OnGround()) ?
            ((attackType == AttackType.Primary) ? lightAttackName : heavyAttackName) :
            ((attackType == AttackType.Primary) ? aerialLightAttackName : aerialHeavyAttackName);

        string currentStyleName = "Style";

        currentStyleName = attackStyle.ToString() + " " + currentStyleName;

        currentAttackName = currentAttackName.Replace("Style", currentStyleName) + Controller.AttackIndex;
    }

    private int AttackHash()
    {
        return Animator.StringToHash(currentAttackName);
    }

    private bool AttackIsOver()
    {
        return Controller.Animator.GetCurrentAnimatorStateInfo(attackLayer).fullPathHash == AttackHash() &&
            Controller.Animator.GetCurrentAnimatorStateInfo(attackLayer).normalizedTime > 1f;
    }

    private bool CanAttack()
    {
        return Controller.Animator.GetCurrentAnimatorStateInfo(attackLayer).fullPathHash == AttackHash() &&
            Controller.Animator.GetCurrentAnimatorStateInfo(attackLayer).normalizedTime > attackAgainIn &&
            attackAgainIn < 1f;
    }

    public override void OnStateFixedUpdate()
    {
        if (!Controller.OnGround())
            Controller.Rigid.velocity = Controller.Rigid.velocity.With(y: Controller.Rigid.velocity.y / 2f);

        Controller.Animator.rootPosition = Controller.Animator.rootPosition
            .With(y: Controller.StickToGroundPosition(Controller.Animator.rootPosition.y));
    }

    private void InputsForAttack()
    {
        if (Controller.Input.PrimaryAttack || Controller.Input.SecondaryAttack)
        {
            Controller.CurrentAttackType = (Controller.Input.PrimaryAttack) ? AttackType.Primary : AttackType.Secondary;

            if (CanAttack())
                Attack();
            else
                if (!automaticAttack) automaticAttack = true;
        }
    }

    private void AutoAttack()
    {
        if (automaticAttack)
            if (CanAttack())
                Attack();
    }

    public override void OnStateUpdate()
    {
        Controller.Animator.applyRootMotion = Controller.OnGround();

        InputsForAttack();

        AutoAttack();

        if (AttackIsOver())
            Controller.ChangeState(Controller.States[typeof(PlayerLocomotion)], Controller, true);
    }
}