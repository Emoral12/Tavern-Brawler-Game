using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public BaseEnemy enemy;
    private BattleStateMachine bsm;
    
    private bool alive = true;

    public enum TurnState
    {
        PROCESSING,
        CHOOSEACTION,
        WAITING,
        SELECTING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    private float curCooldown = 0f;
    private float maxCooldown = 10f;

    private Vector3 startPosition;

    private bool actionStarted = false;
    public GameObject PlayerToAttack;
    private float animSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        currentState = TurnState.PROCESSING;
        bsm = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case (TurnState.PROCESSING):
                UpgradeProgressBar();
                break;
            case (TurnState.CHOOSEACTION):
                ChooseAction();
                currentState = TurnState.WAITING;
                break;
            case (TurnState.WAITING):

                break;
            case (TurnState.SELECTING):

                break;
            case (TurnState.ACTION):
                StartCoroutine(TimeForAction());
                break;
            case (TurnState.DEAD):
                if (!alive)
                {
                    return;
                }
                else
                {
                    this.gameObject.tag = "DeadPlayer";
                    bsm.EnemiesInBattle.Remove(this.gameObject);
                    for (int i = 0; i < bsm.performList.Count; i++)
                    {
                        if (bsm.performList[i].AttackersGameObject == this.gameObject)
                        {
                            bsm.performList.RemoveAt(i);
                        }
                    }
                    this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(105, 105, 105, 255);

                    alive = false;
                }
                break;
        }

    }
    void UpgradeProgressBar()
    {
        curCooldown = curCooldown + Time.deltaTime;
        if (curCooldown >= maxCooldown)
        {
            currentState = TurnState.CHOOSEACTION;
        }
    }

    void ChooseAction()
    {
        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = enemy.name;
        myAttack.Type = "Enemy";
        myAttack.AttackersGameObject = this.gameObject;
        myAttack.AttackersTarget = bsm.PlayerInGame[Random.Range(0, bsm.PlayerInGame.Count)];

        int num = Random.Range(0, enemy.attacks.Count);
        myAttack.chosenAttack = enemy.attacks[num];
        

        bsm.CollectActions(myAttack);
    }

    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }
        actionStarted = true;

        Vector3 playerPos = new Vector3(PlayerToAttack.transform.position.x, transform.position.y, PlayerToAttack.transform.position.z-1.5f);
        while(MoveTowardsEnemy(playerPos))
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        DoDamage();

        Vector3 firstPosition = startPosition;
        while (MoveTowardsStart(firstPosition))
        {
            yield return null;
        }
        bsm.performList.RemoveAt(0);
        bsm.action = BattleStateMachine.PerformAction.WAIT;
        actionStarted = false;
        curCooldown = 0f;
        currentState = TurnState.PROCESSING;

    }

    private bool MoveTowardsEnemy(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }
    private bool MoveTowardsStart(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    void DoDamage()
    {
        
        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = enemy.name;
        int criticalHit = Random.Range(1, 11);
        if (criticalHit == 10)
        {
            float calcDamage = (enemy.curATK + bsm.performList[0].chosenAttack.attackDamage) * 2;
            PlayerToAttack.GetComponent<PlayerStateMachine>().TakeDamage(calcDamage);
            Debug.Log(enemy.name + " deals " + calcDamage + " damage! A critical hit!");
        }
        else
        {
            float calcDamage = enemy.curATK + bsm.performList[0].chosenAttack.attackDamage;
            PlayerToAttack.GetComponent<PlayerStateMachine>().TakeDamage(calcDamage);
            Debug.Log(enemy.name + " deals " + calcDamage + " damage!");
        }
        
    }
    public void TakeDamage(float getDamageAmount)
    {
        enemy.curHP -= getDamageAmount;
        if (enemy.curHP <= 0f)
        {
            currentState = TurnState.DEAD;
        }
    }
}
