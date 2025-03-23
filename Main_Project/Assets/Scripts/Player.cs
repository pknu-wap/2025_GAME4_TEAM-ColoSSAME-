using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float attackRange = 2f;
    public float retreatDistance = 3f;
    public float attackDelay = 1f;
    public LayerMask enemyLayer;
    public LayerMask teammateLayer;
    public Transform initialTarget;
    public int strategyType;

    private Rigidbody2D rb;
    private TargetingSystem targeting;
    private BattleAI battleAI;
    private MovementSystem movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        targeting = GetComponent<TargetingSystem>();
        battleAI = GetComponent<BattleAI>();
        movement = GetComponent<MovementSystem>();

        targeting.Initialize(enemyLayer);
        movement.teammateLayer = teammateLayer;
        battleAI.Initialize(rb, targeting, movement, attackRange, attackDelay, retreatDistance, speed);

        targeting.StartTargeting();
        battleAI.StartBattle();
    }
}
