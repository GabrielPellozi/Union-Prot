using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : CharacterController<PlayerStates, PlayerController>
{
    #region Variables
    [SerializeField]
    private PlayerMovementData movementData;

    [SerializeField]
    private Transform model;

    [SerializeField]
    private CameraController myCamera;

    [SerializeField]
    private FeetAnimatorIK feetIK;

    [SerializeField]
    private PlayerAnimationsEvents animationsEvents;

    [SerializeField]
    private PlayerCombosData defaultAttacks;

    [SerializeField]
    private PlayerEmptyAnimationsClipData emptyAttackClips;

    public PlayerMovementData MovementData { get { return movementData; } }

    public Transform Model { get { return model; } }

    public int AttackIndex { get; set; }

    public Dictionary<int, PlayerAnimationClipsData> CurrentAttacksAnimations { get; set; }

    public AttackStyle CurrentAttackStyle { get; private set; }

    public AttackType CurrentAttackType { get; set; }

    public CameraController MyCamera { get { return myCamera; } }

    public Rigidbody Rigid { get; private set; }

    public Animator Animator { get; private set; }
    public AnimatorOverrideController RuntimeAnimator { get; protected set; }

    public AnimationClipOverrides ClipOverrides { get; protected set; }

    public FeetAnimatorIK FeetIK { get { return feetIK; } }

    public PlayerAnimationsEvents AnimationsEvents { get { return animationsEvents; } }

    public PlayerEmptyAnimationsClipData EmptyAnimationsClips { get { return emptyAttackClips; } }

    public PlayerInput Input { get; private set; }

    public static PlayerController Singleton { get; private set; }
    #endregion

    private void Awake()
    {
        Singleton = this;

        CurrentAttackStyle = AttackStyle.Up;

        Rigid = GetComponentInChildren<Rigidbody>();
        Animator = GetComponentInChildren<Animator>();

        Input = GetComponent<PlayerInput>();

        States = new Dictionary<Type, CharacterStates<PlayerStates, PlayerController>>()
        {
            { typeof(PlayerLocomotion), new PlayerLocomotion() },
            { typeof(PlayerPause), new PlayerPause() },
            { typeof(PlayerEvasion), new PlayerEvasion() },
            { typeof(PlayerAttack), new PlayerAttack() }
        };

        ChangeState(States[typeof(PlayerLocomotion)], this);

        EventsManager.RegisterEvent("Change GameState", CheckGameState);
    }

    private void OnDestroy()
    {
        EventsManager.DeleteEvent("Change GameState", CheckGameState);
    }

    private void Start()
    {
        EventsManager.TriggerEvent("Change Attack Style", CurrentAttackStyle);

        SetupAnimator();
    }

    protected override void Update()
    {
        UpdateStyle();
        base.Update();
    }

    private void UpdateStyle()
    {
        if (Union.CURRENT_GAME_STATE != GameState.GAMEPLAY)
            return;

        if (Input.ChangeAttackStyleUp)
            ChangeStyle(AttackStyle.Up);
        else if (Input.ChangeAttackStyleRight)
            ChangeStyle(AttackStyle.Right);
        else if (Input.ChangeAttackStyleDown)
            ChangeStyle(AttackStyle.Down);
        else if (Input.ChangeAttackStyleLeft)
            ChangeStyle(AttackStyle.Left);
    }

    private void CheckGameState(object argument)
    {
        switch (Union.CURRENT_GAME_STATE)
        {
            case GameState.GAMEPLAY:
                ChangeState(States[typeof(PlayerLocomotion)], this);
                break;

            case GameState.PAUSE:
                ChangeState(States[typeof(PlayerPause)], this);
                break;
        }
    }

    private void SetupAnimator()
    {
        if (RuntimeAnimator == null)
        {
            RuntimeAnimator = new AnimatorOverrideController(Animator.runtimeAnimatorController);
            Animator.runtimeAnimatorController = RuntimeAnimator;
        }

        if (ClipOverrides == null)
        {
            ClipOverrides = new AnimationClipOverrides(RuntimeAnimator.overridesCount);
            RuntimeAnimator.GetOverrides(ClipOverrides);
        }

        CurrentAttacksAnimations = defaultAttacks.GenerateDictionary();

        for (int s = (int)AttackStyle.Up; s < (int)AttackStyle.Left + 1; s++)
        {
            for (int i = (int)AttackSequence.Beggining; i < (int)AttackSequence.End + 1; i++)
            {
                OverrideAnimator(
                    CurrentAttacksAnimations
                    [PlayerAttacksKeys.GenerateKeyValue(true, AttackType.Primary, (AttackStyle)s) + i].Clip,
                    emptyAttackClips.GetEmptyAnimationClipName(i, true, AttackType.Primary, (AttackStyle)s));

                OverrideAnimator(
                    CurrentAttacksAnimations
                    [PlayerAttacksKeys.GenerateKeyValue(true, AttackType.Secondary, (AttackStyle)s) + i].Clip,
                    emptyAttackClips.GetEmptyAnimationClipName(i, true, AttackType.Secondary, (AttackStyle)s));

                OverrideAnimator(
                    CurrentAttacksAnimations
                    [PlayerAttacksKeys.GenerateKeyValue(false, AttackType.Primary, (AttackStyle)s) + i].Clip,
                    emptyAttackClips.GetEmptyAnimationClipName(i, false, AttackType.Primary, (AttackStyle)s));

                OverrideAnimator(
                    CurrentAttacksAnimations
                    [PlayerAttacksKeys.GenerateKeyValue(false, AttackType.Secondary, (AttackStyle)s) + i].Clip,
                    emptyAttackClips.GetEmptyAnimationClipName(i, false, AttackType.Secondary, (AttackStyle)s));
            }
        }
    }

    public void ChangeStyle(AttackStyle newStyle)
    {
        if (newStyle == CurrentAttackStyle)
            return;

        CurrentAttackStyle = newStyle;

        EventsManager.TriggerEvent("Change Attack Style", CurrentAttackStyle);
    }

    public PlayerAnimationClipsData GetCurrentAttackData
        (AttackType currentAttack, AttackStyle currentStyle, int attackIndex, bool onGround)
    {
        return CurrentAttacksAnimations[PlayerAttacksKeys.GenerateKeyValue(onGround, currentAttack, currentStyle) + attackIndex];
    }

    public void OverrideAnimator(AnimationClip clip, string animationNameToOverride)
    {
        if (clip != null)
        {
            ClipOverrides[animationNameToOverride] = clip;

            RuntimeAnimator.ApplyOverrides(ClipOverrides);
        }
    }

    public bool OnGround()
    {
        Collider[] groundColliders =
            Physics.OverlapSphere(model.position + (Vector3.up * 0.15f), 0.25f, movementData.GroundLayer);

        return groundColliders.Length > 0;
    }

    public float StickToGroundPosition(float originalDistance)
    {
        if (Physics.Raycast(model.position, -Vector3.up, out RaycastHit hit, .25f, movementData.GroundLayer))
        {
            return model.position.y - hit.distance;
        }

        return originalDistance;
    }
}

public enum AttackStyle
{
    Up = 1,
    Right,
    Down,
    Left
}