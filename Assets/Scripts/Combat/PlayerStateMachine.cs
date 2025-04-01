using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateMachine : MonoBehaviour
{
    public BasePlayer player;
    private BattleStateMachine bsm;
    

    public enum TurnState
    {
        PROCESSING,
        ADDTOLIST,
        WAITING,
        SELECTING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    private float curCooldown = 0f;
    private float maxCooldown = 5f;
    public Image ProgressBar;
    public bool isDefending = false;
    public GameObject EnemyToAttack;
    private bool actionStarted = false;
    private Vector3 startPos;
    private float animSpeed = 5f;


    private bool alive = true;

    private PlayerPanelStats stats;
    public GameObject PlayerPanel;
    private Transform PlayerPanelSpacer;
    void Start()
    {
        //PlayerPanelSpacer = GameObject.Find("BattleUI").transform.FindChild("PlayerPanel").FindChild("PlayerPanelSpace");
        //CreatePlayerPanel();
        startPos = transform.position;
        curCooldown = Random.Range(0, 2.5f);
        stats = PlayerPanel.GetComponent<PlayerPanelStats>();
        stats.PlayerName.text = player.name;
        stats.PlayerHP.text = "HP: " + player.curHP + "/" + player.baseHP;
        stats.PlayerMP.text = "MP: " + player.curMP + "/" + player.baseMP;
        currentState = TurnState.PROCESSING;
        bsm = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
    }

    void Update()
    {
        switch (currentState)
        {
            case (TurnState.PROCESSING):
                UpgradeProgressBar();
                break;
            case (TurnState.ADDTOLIST):
                bsm.PlayerToManage.Add(this.gameObject);
                currentState = TurnState.WAITING;
                break;
            case (TurnState.WAITING):

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
                    bsm.PlayerToManage.Remove(this.gameObject);
                    bsm.AttackPanel.SetActive(false);
                    bsm.EnemySelectPanel.SetActive(false);
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
        float calcCooldown = curCooldown / maxCooldown;
        ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calcCooldown, 0, 1), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.z);
        if (curCooldown >= maxCooldown)
        {
            currentState = TurnState.ADDTOLIST;
        }
    }
    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }
        actionStarted = true;

        if (isDefending)
        {
            Vector3 playerPos = new Vector3(EnemyToAttack.transform.position.x, transform.position.y, EnemyToAttack.transform.position.z);
        }
        else
        {
            Vector3 enemyPos = new Vector3(EnemyToAttack.transform.position.x, transform.position.y, EnemyToAttack.transform.position.z + 1.5f);

            while (MoveTowardsEnemy(enemyPos))
            {
                yield return null;
            }
        }



        yield return new WaitForSeconds(0.5f);
        if (isDefending == false)
        {
            DoDamage();
            TakeMana();
        }
        

        Vector3 firstPosition = startPos;
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
    public void TakeDamage(float getDamageAmount)
    {
        player.curHP -= getDamageAmount;
        stats.PlayerHP.text = "HP: " + player.curHP + "/" + player.baseHP;
        if (player.curHP <= 0f)
        {
            currentState = TurnState.DEAD;
        }
    }
    void DoDamage()
    {
        int criticalHit = Random.Range(1, 11);
        if (criticalHit == 10)
        {
            float calcDamage = (bsm.performList[0].chosenAttack.attackDamage) * 2;
            EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calcDamage);
            Debug.Log("Player attacks and deals " + calcDamage + " damage! A critical hit!");
        }
        else
        {
            float calcDamage = bsm.performList[0].chosenAttack.attackDamage;
            EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calcDamage);
            Debug.Log("Player attacks and deals " + calcDamage + " damage!");
        }
        
    }
    void TakeMana()
    {
        float takeMana = bsm.performList[0].chosenAttack.attackCost;
        player.curMP -= takeMana;
        stats.PlayerMP.text = "MP: " + player.curMP + "/" + player.baseMP;
    }

   

}
