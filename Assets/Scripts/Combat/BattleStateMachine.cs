using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

public class BattleStateMachine : MonoBehaviour
{
    public enum PerformAction
    {
        WAIT,
        TAKEACTION,
        PERFORMACTION
    }

    public PerformAction action;

    public List<HandleTurn> performList = new List<HandleTurn>();
    public List<GameObject> PlayerInGame = new List<GameObject>();
    public List<GameObject> EnemiesInBattle = new List<GameObject>();

    public enum PlayerGUI
    {
        ACTIVATE,
        WAITING,
        INPUT1,
        INPUT2,
        DONE
    }
    public PlayerGUI PlayerInput;

    public List<GameObject> PlayerToManage = new List<GameObject>();
    private HandleTurn PlayerChoice;

    public GameObject enemyButton;
    public GameObject magicButton;
    public Transform Spacer;
    public Transform MagicSpacer;
    private List<GameObject> atkBtns = new List<GameObject>();

    public GameObject AttackPanel;
    public GameObject MagicPanel;
    public GameObject EnemySelectPanel;

   

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
       

        action = PerformAction.WAIT;
        EnemiesInBattle.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        PlayerInGame.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        PlayerInput = PlayerGUI.ACTIVATE;

        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(false);
        MagicPanel.SetActive(false);
        EnemyButtons();
        
        CreateMagicButtons();
    }

    // Update is called once per frame
    void Update()
    {
        switch (action)
        {
            case PerformAction.WAIT:
                if (performList.Count > 0)
                {
                    action = PerformAction.TAKEACTION;
                }

                break;
            case PerformAction.TAKEACTION:
                GameObject performer = GameObject.Find(performList[0].Attacker);
                if (performList[0].Type == "Enemy")
                {
                    EnemyStateMachine esm = performer.GetComponent<EnemyStateMachine>();
                    for (int i = 0; i < PlayerInGame.Count; i++)
                    {
                        if (performList[0].AttackersTarget == PlayerInGame[i])
                        {
                            esm.PlayerToAttack = performList[0].AttackersTarget;
                            esm.currentState = EnemyStateMachine.TurnState.ACTION;
                            break;
                        }
                        else
                        {
                            performList[0].AttackersTarget = PlayerInGame[Random.Range(0, PlayerInGame.Count)];
                            esm.PlayerToAttack = performList[0].AttackersTarget;
                            esm.currentState = EnemyStateMachine.TurnState.ACTION;
                        }
                    }
                }

                else if (performList[0].Type == "Player")
                {
                    PlayerStateMachine psm = performer.GetComponent<PlayerStateMachine>();
                    psm.isDefending = false;
                    psm.EnemyToAttack = performList[0].AttackersTarget;
                    
                    psm.currentState = PlayerStateMachine.TurnState.ACTION;
                    
                    

                }
                action = PerformAction.PERFORMACTION;
                break;
            case PerformAction.PERFORMACTION:

                break;
        }
        switch (PlayerInput)
        {
            case PlayerGUI.ACTIVATE:
                if (PlayerToManage.Count > 0)
                {
                    AttackPanel.SetActive(true);
                    PlayerInput = PlayerGUI.WAITING;
                    PlayerChoice = new HandleTurn();

                }
                break;
            case PlayerGUI.WAITING:

                break;
            case PlayerGUI.DONE:
                PlayerInputDone();
                break;
        }

    }

    public void CollectActions(HandleTurn input)
    {
        performList.Add(input);
    }

    void EnemyButtons()
    {
        foreach (GameObject enemy in EnemiesInBattle)
        {
            GameObject newButton = Instantiate(enemyButton) as GameObject;
            newButton.transform.SetParent(Spacer);
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();

            EnemyStateMachine curEnemy = enemy.GetComponent<EnemyStateMachine>();

            TMP_Text buttonText = newButton.transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>();
            buttonText.text = curEnemy.enemy.name;

            button.EnemyPrefab = enemy;
        }
    }
    void CreateMagicButtons()
    {
        foreach (BaseAttack magic in PlayerInGame[0].GetComponent<PlayerStateMachine>().player.magic)
        {
            GameObject MagicButton = Instantiate(magicButton) as GameObject;
            MagicButton.transform.SetParent(MagicSpacer);
            TMP_Text MagicButtonText = MagicButton.transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>();
            MagicButtonText.text = magic.name;
            AttackButton ATB = MagicButton.GetComponent<AttackButton>();

        }
    }

    public void Input1()//attack button
    {
        PlayerChoice.Attacker = PlayerToManage[0].name;
        PlayerChoice.AttackersGameObject = PlayerToManage[0];
        PlayerChoice.Type = "Player";
        PlayerChoice.chosenAttack = PlayerToManage[0].GetComponent<PlayerStateMachine>().player.attacks[0];
        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);
    }

    public void Input2(GameObject chosenEnemy)//enemy selection
    {
        PlayerChoice.AttackersTarget = chosenEnemy;
        PlayerInput = PlayerGUI.DONE;
    }

    public void Input3(BaseAttack chosenMagic)//magic button
    {
        PlayerChoice.Attacker = PlayerToManage[0].name;
        PlayerChoice.AttackersGameObject = PlayerToManage[0];
        PlayerChoice.Type = "Player";
        PlayerChoice.chosenAttack = chosenMagic;
        PlayerChoice.chosenAttack = PlayerToManage[0].GetComponent<PlayerStateMachine>().player.magic[0];
        MagicPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);
    }

    public void Input4()
    {
        AttackPanel.SetActive(false);
        MagicPanel.SetActive(true);
    }

    void PlayerInputDone()
    {
        performList.Add(PlayerChoice);
        EnemySelectPanel.SetActive(false);
        PlayerToManage.RemoveAt(0);
        PlayerInput = PlayerGUI.ACTIVATE;
    }
}
        