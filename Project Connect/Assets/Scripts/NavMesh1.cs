using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavMesh1 : MonoBehaviour
{
    public GameObject playerObject;
    public Transform player;
    public float followRadius = 10f;
    public float attackRange = 2f;
    public float roamRadius = 15f;
    public float idleTime = 2f;
    public float attackCooldown = 3f; 
    public float fleeDistance = 10f; 

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 startPosition;
    private float idleTimer;
    private bool isFleeing = false;

    private float lastAttackTime = -Mathf.Infinity; // Tracks last attack time

    private PlayerControl playerControl;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;

        if (playerObject != null)
            playerControl = playerObject.GetComponent<PlayerControl>();
    }

    void Update()
    {
        if (player == null)
        {
            Roam();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isFleeing)
        {
            // If fleeing, keep running for a while before returning to normal behavior
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                isFleeing = false;
            }
        }
        else if (distanceToPlayer <= attackRange)
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
        else if (distanceToPlayer <= followRadius)
        {
            agent.SetDestination(player.position);
            SetAttackAnimation(false);
            SetWalkingAnimation(agent.velocity.magnitude > 0.1f);
        }
        else
        {
            SetAttackAnimation(false);
            Roam();
        }
    }

    void Roam()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
                randomDirection += startPosition;

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }

                idleTimer = 0f;
            }
        }

        SetWalkingAnimation(agent.velocity.magnitude > 0.1f);
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null)
            animator.SetBool("walking", isWalking);
    }

    private void SetAttackAnimation(bool isAttacking)
    {
        if (animator != null)
            animator.SetBool("attack", isAttacking);
    }

    void DealDamageToPlayer(int damage)
    {
        if (playerControl != null)
        {
            playerControl.currentHP = Mathf.Max(0, playerControl.currentHP - damage);
            playerControl.healthBar.SetHealth(playerControl.currentHP);
        }
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
