using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleStateMachine : MonoBehaviour
{
    public enum PerformAction
    {
        WAIT,
        TAKEACTION,
        PERFORMACTION,
        CHECKALIVE,
        WIN,
        LOSE
    }

    public PerformAction battleStates;

    public List<HandleTurn> PerformList = new List<HandleTurn>();

    public List<GameObject> HerosInBattle = new List<GameObject>();

    public enum HeroGUI
    {
        ACTIVATE,
        WAITING,
        INPUT1,
        INPUT2,
        DONE
    }

    public HeroGUI HeroInput;

    public List<GameObject> HerosToManage = new List<GameObject>();
    private HandleTurn HeroChoice;

    public GameObject enemyButton;
    public Transform Spacer;

    public GameObject AttackPanel;
    public GameObject EnemySelectPanel;
    public GameObject MagicPanel;

    //magic attacks
    public Transform actionSpacer;
    public Transform magicSpacer;
    public GameObject actionButton;
    public GameObject magicButton;
    private List<GameObject> atkBtns = new List<GameObject>();

    //enemy buttons
    private List<GameObject> enemyBtns = new List<GameObject>();
    public GameManagerScript gameManager;

    void Start()
    {
        battleStates = PerformAction.WAIT;
        HerosInBattle.AddRange(GameObject.FindGameObjectsWithTag("Hero"));
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManagerScript>();
        HeroInput = HeroGUI.ACTIVATE;

        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(false);
        MagicPanel.SetActive(false);

        EnemyButtons();

    }
    // Update is called once per frame
    void Update()
    {
        switch (battleStates)
        {
            case (PerformAction.WAIT):
                if(PerformList.Count > 0)
                {
                    battleStates = PerformAction.TAKEACTION;
                }
            break;
            case (PerformAction.TAKEACTION):
                GameObject performer = PerformList[0].AttackersGameObject;
                if(PerformList[0].Type == "Enemy")
                {
                    EnemyStateMachine ESM = performer.GetComponent<EnemyStateMachine>();
                        for(int i = 0; i < HerosInBattle.Count; i++)
                        {
                            if(PerformList[0].AttackersTarget == HerosInBattle[i])
                            {
                                 ESM.HeroToAttack = PerformList[0].AttackersTarget;
                                 ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                                 break;
                            }
                            else
                            {
                                PerformList[0].AttackersTarget = HerosInBattle[Random.Range(0, HerosInBattle.Count)];
                                ESM.HeroToAttack = PerformList[0].AttackersTarget;
                                ESM.currentState = EnemyStateMachine.TurnState.ACTION;
                            }
                        }
                    
                }
                if (PerformList[0].Type == "Hero")
                {
                    HeroStateMachine HSM = performer.GetComponent<HeroStateMachine>();
                    HSM.EnemyToAttack = PerformList[0].AttackersTarget;
                    HSM.currentState = HeroStateMachine.TurnState.ACTION;
                }
                battleStates = PerformAction.PERFORMACTION;
                break;
            case (PerformAction.PERFORMACTION):
                //idle
            break;
            case (PerformAction.CHECKALIVE):
                if (HerosInBattle.Count < 1)
                {
                    battleStates = PerformAction.LOSE;
                    //lose game
                }
                else if (gameManager.enemiesToBattle.Count < 1)
                    battleStates = PerformAction.WIN; //win game
                else
                {
                    //call function
                    clearAttackPanel();
                    HeroInput = HeroGUI.ACTIVATE;
                }
            break;
            case (PerformAction.LOSE):
                {
                    SceneManager.LoadScene(1);
                    
                }
            break;
            case (PerformAction.WIN):
                {
                    SceneManager.LoadScene(2);
                    for(int i = 0; i < HerosInBattle.Count; i++)
                    {
                        HerosInBattle[i].GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;

                    }
                }
            break;

        }

        switch (HeroInput)
        {
            case (HeroGUI.ACTIVATE):
                if(HerosToManage.Count > 0)
                {
                    HerosToManage[0].transform.Find("Selector").gameObject.SetActive(true);
                    //create new handleturn instance
                    HeroChoice = new HandleTurn();

                    AttackPanel.SetActive (true);
                    //populate action buttons
                    CreateAttackButtons();

                    HeroInput = HeroGUI.WAITING;
                }
                break;
            case (HeroGUI.WAITING):

                break;
            case (HeroGUI.DONE):
                HeroInputDone();
                break;
            
        }
    }

    public void CollectActions(HandleTurn input)
    {
        PerformList.Add(input);
    }

    public void EnemyButtons()
    {
        //clean up
        foreach(GameObject enemyBtn in enemyBtns)
        {
            Destroy(enemyBtn);
        }
        enemyBtns.Clear();
        //create buttons
        foreach(GameObject enemy in gameManager.enemiesToBattle)
        {
            GameObject newButton = Instantiate(enemyButton) as GameObject;
            EnemySelectButton button = newButton.GetComponent<EnemySelectButton>();

            EnemyStateMachine cur_enemy = enemy.GetComponent<EnemyStateMachine>();

            Text buttonText = newButton.transform.Find("Text").gameObject.GetComponent<Text>();
            buttonText.text = cur_enemy.enemy.theName; 

            button.EnemyPrefab = enemy;

            newButton.transform.SetParent(Spacer, false);
            enemyBtns.Add(newButton);
        }
    }

    public void Input1() //attack button
    {
        HeroChoice.Attacker = HerosToManage[0].name;
        HeroChoice.AttackersGameObject = HerosToManage[0];
        HeroChoice.Type = "Hero";
        HeroChoice.choosenAttack = HerosToManage[0].GetComponent<HeroStateMachine>().hero.attacks[0];
        AttackPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);

    }

    public void Input2(GameObject choosenEnemy) // enemy selection
    {
        HeroChoice.AttackersTarget = choosenEnemy;
        HeroInput = HeroGUI.DONE;
    }

    void HeroInputDone()
    {
        PerformList.Add(HeroChoice);
        //clean the attack panel
        clearAttackPanel();

        HerosToManage[0].transform.Find("Selector").gameObject.SetActive(false);
        HerosToManage.RemoveAt(0);
        HeroInput = HeroGUI.ACTIVATE;
    }

    void clearAttackPanel()
    {
        EnemySelectPanel.SetActive(false);
        AttackPanel.SetActive(false);
        MagicPanel.SetActive(false);

        foreach (GameObject atkBtn in atkBtns)
        {
            Destroy(atkBtn);
        }
        atkBtns.Clear();
    }

    //create action buttons
    void CreateAttackButtons()
    {
        GameObject AttackButton = Instantiate(actionButton) as GameObject;
        Text AttackButtonText = AttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        AttackButtonText.text = "Attack";
        AttackButton.GetComponent<Button>().onClick.AddListener(()=>Input1());
        AttackButton.transform.SetParent(actionSpacer, false);
        atkBtns.Add(AttackButton);
        
        GameObject MagicAttackButton = Instantiate(actionButton) as GameObject;
        Text MagicAttackButtonText = MagicAttackButton.transform.Find("Text").gameObject.GetComponent<Text>();
        MagicAttackButtonText.text = "Magic";
        MagicAttackButton.GetComponent<Button>().onClick.AddListener(() => Input3());
        MagicAttackButton.transform.SetParent(actionSpacer, false);
        atkBtns.Add(MagicAttackButton);

        if(HerosToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks.Count > 0)
        {
            foreach(BaseAttack magicAtk in HerosToManage[0].GetComponent<HeroStateMachine>().hero.MagicAttacks)
            {
                GameObject MagicButton = Instantiate(magicButton) as GameObject;
                Text MagicButtonText = MagicButton.transform.Find("Text").gameObject.GetComponent<Text>();
                MagicButtonText.text = magicAtk.attackName;
                AttackButton ATB = MagicButton.GetComponent<AttackButton>();
                ATB.magicAttackToPerform = magicAtk;
                MagicButton.transform.SetParent(magicSpacer, false);
                atkBtns.Add(MagicButton);
            }
        }
        else
        {
            MagicAttackButton.GetComponent<Button>().interactable = false;
        }
    }

    public void Input4(BaseAttack choosenMagic) //choosen magic attack
    {
        HeroChoice.Attacker = HerosToManage[0].name;
        HeroChoice.AttackersGameObject = HerosToManage[0];
        HeroChoice.Type = "Hero";

        HeroChoice.choosenAttack = choosenMagic;
        MagicPanel.SetActive(false);
        EnemySelectPanel.SetActive(true);
    }

    public void Input3() //switching to magic attacks
    {
        AttackPanel.SetActive(false);
        MagicPanel.SetActive(true);
    }
}
