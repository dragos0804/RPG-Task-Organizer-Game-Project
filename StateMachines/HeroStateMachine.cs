using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HeroStateMachine : MonoBehaviour
{
    private BattleStateMachine BSM;
    public BaseHero hero;

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
    //for the progress bar
    private float cur_cooldown = 0f;
    private float max_cooldown = 5f;
    private Image ProgressBar;
    public GameObject Selector;
    //ienumerator
    public GameObject EnemyToAttack;
    private bool actionStarted = false;
    private Vector3 startPosition;
    private float animSpeed = 20f;
    //dead
    private bool alive = true;
    //heroPanel
    private HeroPanelStats stats;
    public GameObject HeroPanel;
    private Transform HeroPanelSpacer;
    void Start()
    {
        //find spacer
        HeroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");
        //create a panel, fill in info
        CreateHeroPanel();

        startPosition = transform.position;
        cur_cooldown = Random.Range(0, 3f);
        Selector.SetActive(false);
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine> ();
        currentState = TurnState.PROCESSING;
    }

    void Update()
    {
        switch (currentState)
        {
            case (TurnState.PROCESSING):
                UpgradeProgressBar();
            break;
            case (TurnState.ADDTOLIST):
                BSM.HerosToManage.Add(this.gameObject);
                currentState = TurnState.WAITING;
            break;
            case (TurnState.WAITING):
                //idle state
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
                    //change tag
                    this.gameObject.tag = "DeadHero";
                    //not attackable by any enemy
                    BSM.HerosInBattle.Remove(this.gameObject);
                    //not managable
                    BSM.HerosToManage.Remove(this.gameObject);
                    //deactivate the selector
                    Selector.SetActive(false);
                    //reset GUI
                    BSM.AttackPanel.SetActive(false);
                    BSM.EnemySelectPanel.SetActive(false);
                    //remove item from performlist
                    if(BSM.HerosInBattle.Count > 0)
                    {
                        for(int i = 0; i < BSM.PerformList.Count; i++)
                        {
                            if(BSM.PerformList[i].AttackersGameObject == this.gameObject)
                            {
                                BSM.PerformList.Remove(BSM.PerformList[i]);
                            }

                            if(BSM.PerformList[i].AttackersTarget == this.gameObject)
                            {
                                BSM.PerformList[i].AttackersTarget = BSM.HerosInBattle[Random.Range(0, BSM.HerosInBattle.Count)];
                            }
                        }
                    }
                    
                    //change color / play animation
                    this.gameObject.GetComponent<MeshRenderer>().material.color = new Color32(105, 105, 105, 255);
                    //reset heroinput
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                    alive = false;
                }
            break;

        }
    }

    void UpgradeProgressBar()
    {
        cur_cooldown = cur_cooldown + Time.deltaTime;
        float calc_cooldown = cur_cooldown / max_cooldown;
        ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calc_cooldown, 0, 1), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.z);
        if(cur_cooldown >= max_cooldown)
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

        //animate the enemy near the hero to attack
        Vector3 enemyPosition = new Vector3(EnemyToAttack.transform.position.x + 3f, EnemyToAttack.transform.position.y, EnemyToAttack.transform.position.z);
        while (MoveTowardsEnemy(enemyPosition))
        {
            yield return null;
        }
        //wait a bit
        yield return new WaitForSeconds(0.5f);

        //do damage
        DoDamage();
        hero.curMP -= BSM.PerformList[0].choosenAttack.attackCost;
        stats.HeroMP.text = "MP: " + hero.curMP;
        //animate back to the start position
        Vector3 firstPosition = startPosition;
        while (MoveTowardsStart(firstPosition))
        {
            yield return null;
        }
        //remove this performe from the list in BSM
        BSM.PerformList.RemoveAt(0);
        //reset BSM to wait
        if(BSM.battleStates != BattleStateMachine.PerformAction.WIN && BSM.battleStates != BattleStateMachine.PerformAction.LOSE)
        { 
            BSM.battleStates = BattleStateMachine.PerformAction.WAIT;
            //reset this enemy state
            cur_cooldown = 0f;
            currentState = TurnState.PROCESSING;
        }
        else
        {
            currentState = TurnState.PROCESSING;
        }
        //end coroutine
        actionStarted = false;
        
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
        hero.curHP -= getDamageAmount;
        if(hero.curHP <= 0)
        {
            hero.curHP = 0;
            currentState = TurnState.DEAD;
        }
        UpdateHeroPanel();
    }
    //do damage
    void DoDamage()
    {
        float calc_damage = hero.curAtk + BSM.PerformList[0].choosenAttack.attackDamage;
        EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calc_damage);
    }


    //create a hero panel
    void CreateHeroPanel()
    {
        HeroPanel = Instantiate(HeroPanel) as GameObject;
        stats = HeroPanel.GetComponent<HeroPanelStats>();
        stats.HeroName.text = hero.theName;
        stats.HeroHP.text = "HP: " + hero.curHP;
        stats.HeroMP.text = "MP: " + hero.curMP;

        ProgressBar = stats.ProgressBar;
        HeroPanel.transform.SetParent(HeroPanelSpacer, false);
    }

    void UpdateHeroPanel()
    {
        stats.HeroHP.text = "HP: " + hero.curHP;
        stats.HeroMP.text = "MP: " + hero.curMP;
    }
}
