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
        startPos = transform.position;
        curCooldown = Random.Range(0, 2.5f);
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

        Vector3 enemyPos = new Vector3(EnemyToAttack.transform.position.x, transform.position.y, EnemyToAttack.transform.position.z + 1.5f);
        while (MoveTowardsEnemy(enemyPos))
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        DoDamage();

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
        if (player.curHP <= 0f)
        {
            currentState = TurnState.DEAD;
        }
    }
    void DoDamage()
    {
        float calcDamage = 15;
        EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calcDamage);
        Debug.Log("Player attacks and deals " + calcDamage + " damage!");
    }

}
