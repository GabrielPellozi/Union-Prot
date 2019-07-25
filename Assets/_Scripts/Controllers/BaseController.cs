using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController<S, C> : MonoBehaviour where S : BaseState<S, C>
{
    protected S ActiveState { get; set; }

    public Dictionary<Type, S> States { get; protected set; }

    protected virtual void Update()
    {
        if (ActiveState != null)
            ActiveState.OnStateUpdate();
    }

    protected virtual void FixedUpdate()
    {
        if (ActiveState != null)
            ActiveState.OnStateFixedUpdate();
    }

    protected abstract void SetDefaultActiveState(S defaultState);

    public virtual void ChangeState(S newState, C newController, bool checkIfSameState = true)
    {
        if (ActiveState != null)
            if (ActiveState.GetType() == newState.GetType() && checkIfSameState)
                return;

        if (newState.Controller == null)
            newState.Controller = newController;

        if (ActiveState != null)
            ActiveState.OnStateExit();

        ActiveState = newState;

        ActiveState.OnStateEnter();
    }
}