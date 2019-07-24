using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController<S, C> : MonoBehaviour where S : BaseState<S, C>
{
    protected S State { get; set; }

    public Dictionary<Type, S> States { get; protected set; }

    protected virtual void Update()
    {
        if (State != null)
            State.OnStateUpdate();
    }

    protected virtual void FixedUpdate()
    {
        if (State != null)
            State.OnStateFixedUpdate();
    }

    public virtual void ChangeState(S newState, C newController, bool checkIfSameState = true)
    {
        if (State != null)
            if (State.GetType() == newState.GetType() && checkIfSameState)
                return;

        if (newState.Controller == null)
            newState.Controller = newController;

        if (State != null)
            State.OnStateExit();

        State = newState;

        State.OnStateEnter();
    }
}