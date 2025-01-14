using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavAgentBehavior))]
[RequireComponent(typeof(HealthBehavior))]
public class NavCreepController : MonoBehaviour, IDamageDealer
{
    public CreepData creepData;
    
    private NavAgentBehavior _agentBehavior;
    private HealthBehavior _health;
    public bool canDealDamage { get; private set;  } = true;

    private WaitForSeconds _damageWait;
    private readonly WaitForFixedUpdate _wffu = new();
    private Coroutine _damageCoroutine;
    
    [SerializeField] private Collider damageDealingHitBox;
    [SerializeField, ReadOnly] private bool _dealDamageOnCollision;
    [SerializeField, ReadOnly] private bool _dealDamageOnTrigger;

    [SerializeField, SteppedRange(rangeMin:0.5f, rangeMax:10f, step:0.1f)] private float damageCooldown = 3f;
    
    public float damage
    {
        get => creepData.damage;
        set => creepData.damage = value;
    }
    
    public float health
    {
        get => creepData.health;
        set => creepData.health = value;
    }

    public Vector3 hitPoint { get; private set; }

    private void OnEnable() => canDealDamage = true;

    private void Awake()
    {
        _damageWait = new WaitForSeconds(damageCooldown);
        
        if (!damageDealingHitBox)
        {
            _dealDamageOnCollision = false;
            _dealDamageOnTrigger = false;
        }
        else
        {
            _dealDamageOnCollision = damageDealingHitBox.isTrigger == false;
            _dealDamageOnTrigger = damageDealingHitBox.isTrigger;
        }
        
        StartCoroutine(Setup());
    }
    
    private IEnumerator Setup()
    {
        _health = GetComponent<HealthBehavior>();
        
        var attempts = 0;
        while (!_agentBehavior && attempts < 5)
        {
            _agentBehavior = GetComponent<NavAgentBehavior>();
            attempts++;
            yield return _wffu;
        }
        
        if (_agentBehavior)
        {
            _agentBehavior.SetSpeed(creepData.speed);
            _agentBehavior.SetRadius(creepData.radius);
            _agentBehavior.SetHeight(creepData.height);
        } else {
#if UNITY_EDITOR
            Debug.LogError("NavAgentBehavior not found in " + name, this);
#endif
        }
        
        _health.maxHealth = health;
        _health.health = health;
    }
    
    public void StopMovement() => _agentBehavior.StopMovement();

    private void OnCollisionEnter(Collision other)
    {
        if (!_dealDamageOnCollision) return;
        
        var damageable = AdvancedGetComponent<IDamagable>(other.gameObject);
        if(damageable == null) return;
        
        hitPoint = other.GetContact(0).point;
        DealDamage(damageable);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!_dealDamageOnTrigger) return;
        
        var damageable = AdvancedGetComponent<IDamagable>(other.gameObject);
        if(damageable == null) return;
        
        hitPoint = other.ClosestPoint(transform.position);
        DealDamage(damageable);
    }
    
    private IEnumerator HandleDealingDamage(IDamagable target)
    {
        canDealDamage = false;
        target.TakeDamage(this);
        yield return _damageWait;
        yield return _wffu;
        
        canDealDamage = true;
        _damageCoroutine = null;
    }
    
    public void DealDamage(IDamagable target)
    {
        if (!canDealDamage) return;
        _damageCoroutine ??= StartCoroutine(HandleDealingDamage(target));
    }
}
