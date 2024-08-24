using players;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.RuleTile.TilingRuleOutput;


// 상태 인터페이스 정의
public interface IState<T>
{
    void Enter(T t);
    void Update(T t);
    void Exit(T t);
}

// 상태 머신 클래스 정의, 현재 상태를 추적하고 상태를 변경하는 데 사용

public class StateMachine<T>
{
    public IState<T> currentState;//이거 디버그 후 private으로 바꿔야함

    public void ChangeState(IState<T> newState, T t)
    {
        if (currentState != null)
        {
            currentState.Exit(t);
        }

        currentState = newState;
        currentState.Enter(t);
    }

    public void Update(T t)
    {
        if (currentState != null)
        {
            currentState.Update(t);
        }
    }
}

// 몬스터 클래스 정의
public class MonsterController : Unit
{
    //몬스터정보
    public int level;
    public float hp;
    public float maxhealth;
    public float str;
    public string name;
    public string rangetype;
    public int range;
    [SerializeField] FloatingHealthBar healthBar;
    public void Init(MonsterData monsterData)
    {
        this.level = monsterData.level;
        this.hp = monsterData.health;
        this.maxhealth = monsterData.health;
        this.str = monsterData.str;
        this.name = monsterData.name;
        this.rangetype = monsterData.rangetype;
    }

    //코루틴 - 1초당 한 번 호출하려고 따로 뺌 
    public bool isCoroutineRunning = false;


    private NavMeshAgent agent;
    public NavMeshAgent Agent { get { return agent; } }
    public Animator animator { get; private set; }
    private Statusinfo statusinfo;
   
    public StateMachine<MonsterController> stateMachine;
    public bool idlestate = false;
    public bool tracking = false; //5초간 추격
    private float updateInterval = 3f; //몬스터 언제 소환시킬건지
    private float timeSinceLastUpdate = 0f;

    private bool stop;
    public bool Stop
    {
        get { return stop; }
        set { stop = value; }
    }
    public bool showPath;
    public bool showAhead;

    float FindRange = 1.5f;
    /*void OnDrawGizmos() // 범위 그리기
    {
            // Draw a green line between the monster and the player
            Gizmos.color = Color.green;
            //Gizmos.DrawLine(transform.position, target.transform.position);
    }*/
    void OnDrawGizmos() // 범위 그리기
    {
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }



    private void Awake()
    {
        healthBar = GetComponentInChildren<FloatingHealthBar>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        //info = GetComponent<monsterinfo>();
        stateMachine = new StateMachine<MonsterController>();
        stateMachine.ChangeState(new IdleState(), this);
        // Debug log to check if agent is initialized properly
        if (agent == null)
        {
            Debug.LogError("네비메시에이전트 초기화 오류", this);
        }

    }
    private void Start()
    {
    }
    public void TakeDamage(float damageAmount)
    {
        hp -= damageAmount;  
        healthBar.UpdateHealthBar(hp, maxhealth);
        if(hp < 0)
        {
            Statusinfo statusinfo = GameObject.FindGameObjectWithTag("Player").GetComponent<Statusinfo>();

            if (statusinfo != null) statusinfo.hp = 1000;
            if (statusinfo != null) statusinfo.Maxhealth = 1000;
            stateMachine.ChangeState(new DeadState(), this);    
        }
    }
    public void LookTarget(Vector3 direction)
    {
        UpdateAnimatorParameters(direction);
    }
    void UpdateAnimatorParameters(Vector3 direction)
    {
        Vector3 localDirection = transform.InverseTransformDirection(direction);
        float angle = Mathf.Atan2(localDirection.y, localDirection.x) * Mathf.Rad2Deg;
        int directionIndex = Mathf.RoundToInt((angle + 360f) % 360f / 45f) % 8;
        switch (directionIndex)
        {
            case 0: // Right
                animator.SetFloat("Horizontal", 1);
                animator.SetFloat("Vertical", 0);
                break;
            case 1: // Up-right
                animator.SetFloat("Horizontal", 1);
                animator.SetFloat("Vertical", 1);
                break;
            case 2: // Up
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", 1);
                break;
            case 3: // Up-left
                animator.SetFloat("Horizontal", -1);
                animator.SetFloat("Vertical", 1);
                break;
            case 4: // Left
                animator.SetFloat("Horizontal", -1);
                animator.SetFloat("Vertical", 0);
                break;
            case 5: // Down-left
                animator.SetFloat("Horizontal", -1);
                animator.SetFloat("Vertical", -1);
                break;
            case 6: // Down
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", -1);
                break;
            case 7: // Down-right
                animator.SetFloat("Horizontal", 1);
                animator.SetFloat("Vertical", -1);
                break;
            default:
                break;
        }
    }
    private void Update()
    {
        //Debug.Log("넌 누구니");
        if (agent.velocity.magnitude > 0)
        {
            animator.SetBool("Run", true);
            //
            Vector3 direction = agent.velocity.normalized;

            UpdateAnimatorParameters(direction);
        }
        else
        {
            animator.SetBool("Run", false);
        }

        stateMachine.Update(this);
    }

    //몬스터 사망 애니메이션 이벤트 함수
    public void Die()
    {
        ObjectPool.Instance.AddInactiveObject(gameObject);
    }

    /// <summary>
    /// 몬스터 클래스에 대한 추가 메서드 및 속성...
    /// </summary>


    private Coroutine exitDelayCoroutine; // Coroutine variable for the exit delay

    void OnTriggerEnter2D(Collider2D other)
    {
        if (stateMachine.currentState is DeadState) return;
        if (other.gameObject.CompareTag("Player"))
        {
            if (exitDelayCoroutine != null)
            {
                StopCoroutine(exitDelayCoroutine);
                exitDelayCoroutine = null;
            }

            target = other.gameObject;
            stateMachine.ChangeState(new MoveState(), this);
        }
    }
    //비활성화 됐을 때


    void OnTriggerExit2D(Collider2D other)
    {
        if (stateMachine.currentState is DeadState) return;
        if (other.gameObject.CompareTag("Player") && target != null && target.activeSelf && gameObject.activeSelf)
        {
            exitDelayCoroutine = StartCoroutine(ExitDelay());
        }

    }

    private void OnDisable()
    {
        GetComponent<NavMeshAgent>().enabled = false;
    }
    //활성화 됐을 때
    private void OnEnable()
    {
        StartCoroutine(testc());

    }
    IEnumerator testc()
    {
        yield return new WaitForSeconds(0.1f);

        GetComponent<NavMeshAgent>().enabled = true;
    }
    IEnumerator ExitDelay()
    {
        yield return new WaitForSeconds(5f);

        stateMachine.ChangeState(new IdleState(), this);
    }

    public void IdleState()
    {
        transform.rotation = new Quaternion(0, 0, 0, 1);
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized * 20f;
            Vector3 randomPosition = transform.position + new Vector3(randomDirection.x, 0f, randomDirection.y);
            randomPosition.y = 0f;
            randomPosition.z = 0f;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPosition, out hit, 20f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            timeSinceLastUpdate = 0f;
        }
    }

    public void rocateset()
    {
        if (agent != null)
        {
            /*transform.rotation = Quaternion.Euler(0f, 0f, desiredRotationZ);
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized, Vector3.up);
            targetRotation.eulerAngles = new Vector3(0f, 0f, targetRotation.eulerAngles.z);
            transform.rotation = targetRotation;*/
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            //Debug.LogError("Agent is not initialized in MonsterController.rocateset().");
        }
    }

    public void MoveState()
    {
        if (Agent.isOnNavMesh && (transform.position - target.transform.position).magnitude < 2)
        {
            Agent.isStopped = true;
            stateMachine.ChangeState(new AttackState(), this);
            rocateset();
        }
        else
        {
            if (Agent.isOnNavMesh) Agent.isStopped = false;
            if (Agent.isOnNavMesh) Agent.speed = 2f;
            if (Agent.isOnNavMesh) Agent.SetDestination(target.transform.position);
            Quaternion targetRotation = Quaternion.LookRotation(Agent.velocity.normalized, Vector3.up);
            targetRotation.eulerAngles = new Vector3(0f, 0f, targetRotation.eulerAngles.z);
            transform.rotation = targetRotation;
        }

    }
    public void Attack()
    {
        //타겟방향 바라보도록
        Vector3 a = target.transform.position - transform.position;
        animator.SetBool("Atk", true);
        animator.Play("Attack", 0, 0f);
        target.GetComponent<MesRay>().TakeDamage(str);
        LookTarget(a);

    }
    public void Attackend()
    {
        animator.SetBool("Atk", false);
    }


}
/// <summary>
/// 상태 클래스들
/// </summary>

// 대기 상태 클래스 정의

public class IdleState : IState<MonsterController>
{
    public float updateInterval = 2f; // Update interval for random movement
    private float timeSinceLastUpdate = 0f;
    public void Enter(MonsterController monster)
    {

        monster.rocateset();
        //monster.idlestate = true;
    }
    public void Update(MonsterController monster)
    {
        monster.IdleState();
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            SetRandomDestination(monster);
            timeSinceLastUpdate = 0f;
        }
    }

    public void Exit(MonsterController monster)
    {
        // 몬스터에 대한 상태 종료 로직...
        //  Debug.Log("Idle Exit :...");
        // monster.StopIdleAnimation();
    }
    void SetRandomDestination(MonsterController monster)
    {
        Vector3 randomPosition = GetRandomNavMeshPoint(monster.transform.position, 20f); // Get random position within 20 units
        monster.Agent.SetDestination(randomPosition);
    }

    Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas);

        return hit.position;
    }
}
public class MoveState : IState<MonsterController>
{
    public void Enter(MonsterController monster)
    {
        
    }
    public void Update(MonsterController monster)
    {
        monster.MoveState();
    }
    public void Exit(MonsterController monster)
    {
        //  Debug.Log("Move Exit:...");
    }
}
// 공격 상태 클래스 정의
// 공격 상태 클래스 정의
public class AttackState : IState<MonsterController>
{
    float attackInterval = 1f;
    float timeSinceLastAttack = 0f; // Track time since last attack

    public void Enter(MonsterController monster)
    {
        // Start coroutine only if it's not already running
        if (!monster.isCoroutineRunning)
        {
            monster.isCoroutineRunning = true;
        }
    }
    public void Update(MonsterController monster)
    {
        monster.rocateset();

        // Check if enough time has passed for the next attack
        timeSinceLastAttack += Time.deltaTime;
        if (timeSinceLastAttack >= attackInterval)
        {
            monster.Attack(); // Call Attack() function
            timeSinceLastAttack = 0f; // Reset timer
        }

        if ((monster.transform.position - monster.target.transform.position).magnitude >= 4)
        {
            monster.Attackend(); // Call Attackend() to end attack animation if necessary
            monster.Agent.isStopped = false;
            monster.stateMachine.ChangeState(new MoveState(), monster);
        }
    }
    public void Exit(MonsterController monster)
    {
        if (monster.Agent.isOnNavMesh) monster.Agent.speed = 2f;
    }
}

// Define the DeadState class
public class DeadState : IState<MonsterController>
{
    public void Enter(MonsterController monster)
    {
        monster.animator.SetTrigger("Die");
    }

    public void Update(MonsterController monster)
    {
        Quaternion targetRotation = Quaternion.LookRotation(monster.Agent.velocity.normalized, Vector3.up);
        targetRotation.eulerAngles = new Vector3(0f, 0f, 0f);
        monster.transform.rotation = targetRotation;

    }

    public void Exit(MonsterController monster)
    {
        Debug.Log("Dead Exit :...");
    }
}