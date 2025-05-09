using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMesh3 : MonoBehaviour
{
    public GameObject playerObject;
    public float baseRoamRadius = 10f;
    public float maxRoamRadius = 25f;
    public float baseHuntRadius = 8f;
    public float maxHuntRadius = 30f;
    public float idleTime = 2f;
    public float baseDamage = 5f;
    public float maxDamage = 20f;
    public float baseSpeed = 2f;
    public float maxSpeed = 6f;
    public float attackRange = 2f;
    public float attackCooldown = 3f; 
    public float fleeDistance = 10f; 

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
        if (playerControl == null) return;

        ApplyFuzzyModifiers();

        float distanceToPlayer = Vector3.Distance(transform.position, playerObject.transform.position);
        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (distanceToPlayer <= attackRange)
        {
            agent.ResetPath();
            SetWalkingAnimation(false);
            SetAttackAnimation(true);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                DealDamageToPlayer(Mathf.RoundToInt(baseDamage));
                lastAttackTime = Time.time;
            }
        }
        else if (distanceToPlayer <= baseHuntRadius)
        {
            agent.SetDestination(playerObject.transform.position);
            SetWalkingAnimation(isMoving);
            SetAttackAnimation(false);
        }
        else
        {
            Roam();
            SetAttackAnimation(false);
            SetWalkingAnimation(isMoving);
        }
    }

    void ApplyFuzzyModifiers()
    {
        float t = Mathf.Clamp01(playerControl.tokens / 24f);

        baseDamage = Mathf.Lerp(5f, maxDamage, t);
        agent.speed = Mathf.Lerp(baseSpeed, maxSpeed, t);
        baseHuntRadius = Mathf.Lerp(8f, maxHuntRadius, t);
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
        float roamRadius = Mathf.Lerp(baseRoamRadius, maxRoamRadius, Mathf.Clamp01(playerControl.tokens / 24f));
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius + startPosition;

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
