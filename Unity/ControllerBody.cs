using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ControllerBody : MonoBehaviour
{
    public enum State { Await, Idle, Searching, Chasing, Attacking, Stunned, Casting1, Casting2, Casting3, Casting4 }
    public State currentState = State.Idle;

    public Transform target; // Cible actuelle
    private float attackRange;
    private float searchCooldown = 1f; // Temps entre chaque recherche d’ennemis
    private float nextSearchTime = 0f;
    private bool isCCed = false; // Si le perso est contrôlé

    public Animation anim;
    public List<AnimationClip> animAttack1String = new List<AnimationClip>();
    public List<AnimationClip> animAttack2String = new List<AnimationClip>();
    public List<AnimationClip> animAttack3String = new List<AnimationClip>();

    public List<AnimationClip> animSkill1String = new List<AnimationClip>();
    public List<AnimationClip> animSkill2String = new List<AnimationClip>();
    public List<AnimationClip> animSkill3String = new List<AnimationClip>();
    public List<AnimationClip> animSoulWeaponString = new List<AnimationClip>();

    public ControllerHairLightweight controllerHair;
    public ControllerWeapon controllerWeapon;

    public string currentAnimationName { get; private set; }
    private string idleAnim, runAnim;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;
    private bool isCasting = false;

    public Creature creature;

    void Start()
    {
        creature = GetComponentInParent<ControllerCreature>().creatureInstance;
        attackRange = creature.range;

        idleAnim = $"Hero_{creature.Name}@Astand_Astand";
        runAnim = $"Hero_{creature.Name}@Run_Run";

        anim = GetComponent<Animation>();
        foreach (AnimationState state in anim)
        {
            string name = state.name;
            if (name.Contains("@Attack1")) { animAttack1String.Add(state.clip); continue; }
            else if (name.Contains("@Attack2")) { animAttack2String.Add(state.clip); continue; }
            else if (name.Contains("@Attack3")) { animAttack3String.Add(state.clip); continue; }
            else if (name.Contains("@Skill1")) { animSkill1String.Add(state.clip); continue; }
            else if (name.Contains("@Skill2")) { animSkill2String.Add(state.clip); continue; }
            else if (name.Contains("@Skill3")) { animSkill3String.Add(state.clip); continue; }
            else if (name.Contains("@SoulWeapon")) { animSoulWeaponString.Add(state.clip); }
        }
        ChangeState(State.Idle);
    }

    void Update()
    {
        if (isCCed)
        {
            ChangeState(State.Stunned);
            return;
        }

        switch (currentState)
        {
            case State.Idle:
                PlayAnimation(idleAnim);
                if (Time.time >= nextSearchTime)
                {
                    nextSearchTime = Time.time + searchCooldown;
                    SearchForEnemy();
                }
                break;

            case State.Searching:
                if (target != null)
                    ChangeState(State.Chasing);
                else
                    ChangeState(State.Idle);
                break;

            case State.Chasing:
                if (target == null)
                {
                    ChangeState(State.Idle);
                }
                else if (Vector3.Distance(transform.parent.position, target.position) <= attackRange)
                {
                    ChangeState(State.Attacking);
                }
                else
                {
                    PlayAnimation(runAnim);
                    MoveTowardsTarget();
                }
                break;

            case State.Attacking:
                if (target == null)
                {
                    ChangeState(State.Idle);
                }
                else if (Vector3.Distance(transform.parent.position, target.position) > attackRange)
                {
                    ChangeState(State.Chasing);
                }
                else
                {
                    Attack();
                }
                break;

            case State.Stunned:
                // Attend la fin du CC (géré ailleurs)
                break;
            case State.Casting1:
                StartCoroutine(CastSkill(0));
                break;
            case State.Casting2:
                StartCoroutine(CastSkill(1));
                break;
            case State.Casting3:
                StartCoroutine(CastSkill(2));
                break;
            case State.Casting4:
                StartCoroutine(CastSkill(3));
                break;
        }
    }

    public void ChangeState(State newState)
    {
        currentState = newState;
        //Debug.Log($"Body anim played : {anim.clip.name}");
    }

    void SearchForEnemy()
    {
        Creature closestEnemy = null;
        float minDistance = Mathf.Infinity;

        if (Globals.listHeroes.Contains(creature))
        {
            foreach (Creature enemy in Globals.listEnemies)
            {
                float distance = Vector3.Distance(transform.parent.position, enemy.gameObject.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }
            if (closestEnemy != null) creature.EnemyTarget = closestEnemy;
        }
        else
        {
            foreach (Creature enemy in Globals.listHeroes)
            {
                float distance = Vector3.Distance(transform.parent.position, enemy.gameObject.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }
            if (closestEnemy != null) creature.EnemyTarget = closestEnemy;
        }
        if (closestEnemy != null)
        {
            target = closestEnemy.gameObject.transform;
            ChangeState(State.Chasing);
        }
    }

    void MoveTowardsTarget()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.parent.position).normalized;
            transform.parent.position += direction * 5 * Time.deltaTime;
            transform.LookAt(target);
        }
    }

    void Attack()
    {
        transform.LookAt(target);
        if (isAttacking) return; // Si une attaque est déjà en cours, ne pas en commencer une nouvelle
        isAttacking = true; // Marquer comme étant en train d'attaquer

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + (1000f / (float)creature.AtkSpd);
            StartCoroutine(AnimationAutoAttackString(animAttack1String));
        }
    }

    public void ApplyCrowdControl(float duration)
    {
        isCCed = true;
        ChangeState(State.Stunned);
        Invoke(nameof(RemoveCrowdControl), duration);
    }

    void RemoveCrowdControl()
    {
        isCCed = false;
        ChangeState(State.Idle);
    }
    IEnumerator CastSkill(int skillIndex)
    {
        // Exit if already casting or invalid skill
        if (isCasting || skillIndex < 0 || skillIndex >= creature.SkillList.Count)
            yield break;
        isCasting = true;

        List<AnimationClip> skillAnimations = GetSkillAnimations(skillIndex);
        Skill skill = creature.SkillList[skillIndex];

        // Play initial cast animation
        if (skillAnimations.Count > 0)
        {
            skill.DeductSkillCost(creature);
            PlayAnimation(skillAnimations[0].name);
            yield return new WaitWhile(() => anim.IsPlaying(skillAnimations[0].name));
        }

        // Execute skill logic
        skill.ActivateDuration(creature);
        skill.Use(creature);

        // Handle channeling
        if (skill.isChanneling && skillAnimations.Count > 1)
        {
            // Play channeling loop (second animation)
            skillAnimations[1].wrapMode = WrapMode.Loop;
            PlayAnimation(skillAnimations[1].name);

            // Wait while channeling AND animation is playing
            while (skill.isChanneling)
            {
                yield return null;
            }
            // If interrupted or channeling ended, play exit animation (third if available)
            if (skillAnimations.Count > 2)
            {
                PlayAnimation(skillAnimations[2].name);
                yield return new WaitWhile(() => anim.IsPlaying(skillAnimations[2].name));
            }
        }
        isCasting = false;
        // Return to idle
        ChangeState(State.Attacking);
    }
    public List<AnimationClip> GetSkillAnimations(int skillIndex)
    {
        switch (skillIndex)
        {
            case 0: return animSkill1String;
            case 1: return animSkill2String;
            case 2: return animSkill3String;
            case 3: return animSoulWeaponString;
            default: return new List<AnimationClip>();
        }
    }
    IEnumerator AnimationAutoAttackString(List<AnimationClip> listAnimation)
    {
        foreach (var clip in listAnimation)
        {
            creature.AutoAttack.Use(creature);
            PlayAnimation(clip.name);
            yield return new WaitWhile(() => anim.IsPlaying(clip.name));
        }
        isAttacking = false;
    }
    void PlayAnimation(string clipName)
    {
        currentAnimationName = clipName;
        anim[clipName].speed = (float)creature.AtkSpd / 1000f;
        anim.CrossFade(clipName, 0.1f);

        if (controllerHair != null)
        {
            controllerHair.anim[clipName].speed = (float)creature.AtkSpd / 1000f;
            controllerHair.anim.CrossFade(clipName, 0.1f);
        }

        if (controllerWeapon != null)
        {
            string weaponClipName = clipName.Replace("@", "_Weapon@");
            controllerWeapon.anim[weaponClipName].speed = (float)creature.AtkSpd / 1000f;
            controllerWeapon.anim.CrossFade(weaponClipName, 0.1f);
        }
    }
}
