using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMesh2 : MonoBehaviour
{
    public GameObject playerObject;
    public float huntRadius = 20f;
    public float roamRadius = 15f;
    public float idleTime = 2f;
    public float attackRange = 2f;
    public float attackCooldown = 3f;
    public float fleeDistance = 10f; // Distance to flee from light

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 startPosition;
    private float idleTimer;

    private PlayerControl playerControl;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isFleeing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;

        if (playerObject != null)
            playerControl = playerObject.GetComponent<PlayerControl>();

        GoToNewRoamPoint();
    }

    void Update()
    {
        if (playerObject != null && playerControl != null && ShouldHuntPlayer())
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerObject.transform.position);

            if (distanceToPlayer <= attackRange)
            {
                agent.ResetPath();
                SetWalkingAnimation(false);
                SetAttackAnimation(true);

                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    DealDamageToPlayer(5);
                    lastAttackTime = Time.time;
                }
            }
            else if (distanceToPlayer <= huntRadius)
            {
                agent.SetDestination(playerObject.transform.position);
                SetAttackAnimation(false);
                SetWalkingAnimation(agent.velocity.magnitude > 0.1f);
            }
            else
            {
                ResetBehavior();
            }
        }
        else
        {
            ResetBehavior();
        }
    }

    void ResetBehavior()
    {
        SetAttackAnimation(false);
        SetWalkingAnimation(agent.velocity.magnitude > 0.1f);
        Roam();
    }

    bool ShouldHuntPlayer()
    {
        return playerControl.currentHP < (playerControl.maxHP * 0.5f);
    }

    void Roam()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                GoToNewRoamPoint();
                idleTimer = 0f;
            }
        }
    }

    void GoToNewRoamPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += startPosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null)
            animator.SetBool("walking", isWalking);
    }

    void SetAttackAnimation(bool isAttacking)
    {
        if (animator != null)
            animator.SetBool("attack", isAttacking);
    }

    void DealDamageToPlayer(int damage)
    {
        playerControl.currentHP = Mathf.Max(0, playerControl.currentHP - damage);
        playerControl.healthBar.SetHealth(playerControl.currentHP);
    }
    public void FleeFromLight(Vector3 lightSource)
    {
        isFleeing = true;
        Vector3 fleeDirection = (transform.position - lightSource).normalized * fleeDistance;
        Vector3 fleePosition = transform.position + fleeDirection;

        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            SetWalkingAnimation(true);
            SetAttackAnimation(false);
        }

        Debug.Log(gameObject.name + " is fleeing from the light!");
    }

}
