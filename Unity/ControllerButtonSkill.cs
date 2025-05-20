using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ControllerButtonSkill : MonoBehaviour
{
    [Flags]
    enum State
    {
        Await = 1 << 0,
        Ready = 1 << 1,
        Casting = 1 << 2,
        Channeling = 1 << 3,
        ExternalCasting = 1 << 4,
        Reserved = 1 << 5,
    }
    State currentState;

    int buttonOrder;

    ControllerBody creatureBody;
    ControllerHair creatureHair;

    Creature creature;
    Skill skill;

    ControllerBody.State castingStateBody;
    ControllerHair.State castingStateHair;

    float animationTime = 0f;
    float animationCooldownLength;
    AnimationState animationCooldownSpeed;
    float channelingDelay = 0f;
    int referenceListCount;

    public ControllerButtonSkill[] otherSkillButtons;

    private bool onPress = false;

    void Start()
    {
        buttonOrder = gameObject.name[gameObject.name.Length - 1] - '0' - 1;

        creature = GetComponentInParent<ContainerHero>().hero;
        creatureBody = creature.gameObject.GetComponentInChildren<ControllerBody>();
        creatureHair = creature.gameObject.GetComponentInChildren<ControllerHair>();

        skill = creature.SkillList[buttonOrder];

        castingStateBody = (ControllerBody.State)CastingStateNumber(buttonOrder);
        castingStateHair = (ControllerHair.State)CastingStateNumber(buttonOrder);

        animationCooldownLength = creatureBody.GetSkillAnimations(buttonOrder)[0].length;
        animationCooldownSpeed = creatureBody.anim[creatureBody.GetSkillAnimations(buttonOrder)[0].name];
        referenceListCount = creatureBody.GetSkillAnimations(buttonOrder).Count;

        ControllerButtonSkill controllerButtonSkill = gameObject.GetComponentInParent<ControllerButtonSkill>();

        currentState = State.Await;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Await:
                if (onPress)
                {
                    ChangeReserved();
                    ChangeState(State.Reserved);
                    break;
                }
                if (skill.isReadySkill(creature)) ChangeState(State.Ready);
                break;
            case State.Ready:
                if (onPress && AllButtonsFree())
                {
                    ChangeReserved();
                    ChangeState(State.Casting);
                    SetSiblingsToExternalCasting();
                }
                else if (onPress && !AllButtonsFree())
                {
                    ChangeReserved();
                    ChangeState(State.Reserved);
                }
                break;
            case State.Casting:
                if (onPress && referenceListCount < 2)
                {
                    ChangeState(State.Reserved);
                    break;
                }
                if (animationTime < animationCooldownLength / animationCooldownSpeed.speed)
                {
                    creatureBody.ChangeState(castingStateBody);
                    creatureHair.ChangeState(castingStateHair);
                    animationTime += Time.deltaTime;
                    break;
                }
                if (referenceListCount > 1)
                {
                    ChangeState(State.Channeling);
                    animationTime = 0;
                    break;
                }
                else CompleteCasting();
                break;
            case State.Channeling:
                if (skill.isChanneling) channelingDelay += Time.deltaTime;
                if (onPress && channelingDelay > 0)
                {
                    skill.isChanneling = false;
                    ChangeState(State.Await);
                    ReleaseSiblings();
                    channelingDelay = 0;
                    break;
                }
                if (!skill.isChanneling && channelingDelay > 1f)
                {
                    skill.isChanneling = false;
                    ChangeState(State.Await);
                    ReleaseSiblings();
                    channelingDelay = 0;
                    break;
                }
                break;
            case State.ExternalCasting:
                if (onPress)
                {
                    ChangeReserved();
                    ChangeState(State.Reserved);
                }
                break;
            case State.Reserved:
                if (onPress) { ChangeState(State.Await); break; }
                if (skill.isReadySkill(creature) && AllButtonsFree())
                {
                    ChangeState(State.Casting);
                    SetSiblingsToExternalCasting();
                }
                break;
        }
    }
    public void ButtonPress()
    {
        onPress = true;
    }
    void ChangeState(State newState)
    {
        currentState = newState;
        onPress = false;
    }
    int CastingStateNumber(int skillNumber)
    {
        switch (skillNumber)
        {
            case 0:
                return (int)ControllerBody.State.Casting1;
            case 1:
                return (int)ControllerBody.State.Casting2;
            case 2:
                return (int)ControllerBody.State.Casting3;
            case 3:
                return (int)ControllerBody.State.Casting4;
        }
        return 0;
    }
    bool AllButtonsFree()
    {
        foreach (var button in otherSkillButtons)
        {
            if (button.currentState == State.Casting || button.currentState == State.Channeling) return false;
        }
        return true;
    }
    void ChangeReserved()
    {
        foreach (var button in otherSkillButtons)
        {
            if (button.currentState == State.Reserved) button.ChangeState(State.Await);
        }
    }
    void SetSiblingsToExternalCasting()
    {
        foreach (var button in otherSkillButtons)
        {
            button.ChangeState(State.ExternalCasting);
        }
    }
    void ReleaseSiblings()
    {
        foreach (var button in otherSkillButtons)
        {
            if (button.currentState != State.Reserved) button.ChangeState(State.Await);
        }
    }
    void CompleteCasting()
    {
        animationTime = 0;
        ChangeState(State.Await);

        foreach (var button in otherSkillButtons)
        {
            if (button.currentState == State.ExternalCasting) button.ChangeState(State.Await);
        }
    }
}
