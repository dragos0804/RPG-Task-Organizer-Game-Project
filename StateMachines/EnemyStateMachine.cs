using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyStateMachine : MonoBehaviour
{
    private BattleStateMachine BSM;
    public BaseEnemy enemy;

    public enum TurnState
    {
        PROCESSING,
        CHOOSEACTION,
        WAITING,
        ACTION,
        DEAD
    }

    public TurnState currentState;
    //for the progress bar
    private float cur_cooldown = 0f;
    private float max_cooldown = 10f;

    private Vector3 startposition;
    public GameObject Selector;
    //time for action
    private bool actionStarted = false;
    public GameObject HeroToAttack;
    private float animSpeed = 17f;
    //alive
    private bool alive = true;
    public MeshRenderer meshRenderer;

    void Start()
    {
        currentState = TurnState.PROCESSING;
        Selector.SetActive(false);
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        startposition = transform.position;
    }

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
                //idle state
        break;
        case (TurnState.ACTION):
                StartCoroutine(TimeForAction());
                break;
        case (TurnState.DEAD):
                if (!alive)
                    return;
                else
                {
                    //change tag of the enemy
                    this.gameObject.tag = "DeadEnemy";
                    //not attackable by heros
                    BSM.gameManager.enemiesToBattle.Remove(this.gameObject);
                    //disable the selector
                    Selector.SetActive(false);
                    //remove all inputs enemyattacks
                    if(BSM.gameManager.enemiesToBattle.Count > 0)
                    {
                        for(int i = 0; i < BSM.PerformList.Count; i++)
                        {
                            if (BSM.PerformList[i].AttackersTarget == this.gameObject)
                            {
                                BSM.PerformList[i].AttackersTarget = BSM.gameManager.enemiesToBattle[Random.Range(0, BSM.gameManager.enemiesToBattle.Count)];
                            }

                            if (BSM.PerformList[i].AttackersGameObject== this.gameObject)
                            {
                                BSM.PerformList.Remove(BSM.PerformList[i]);
                            }
                            
                        }
                    }
                    
                    //change the color to gray / play dead animation
                    meshRenderer.material.color = new Color32(105, 105, 105, 255);
                    //set alive to be false
                    alive = false;
                    //reset enemybuttons
                    BSM.EnemyButtons();
                    //check alive
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                }
               break;

        }
    }
    void UpgradeProgressBar()
    {
        cur_cooldown = cur_cooldown + Time.deltaTime;
        if (cur_cooldown >= max_cooldown)
        {
            currentState = TurnState.CHOOSEACTION;
        }
    }

    void ChooseAction()
    {
        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = enemy.theName;
        myAttack.Type = "Enemy";
        myAttack.AttackersGameObject = this.gameObject;
        myAttack.AttackersTarget = BSM.HerosInBattle[Random.Range(0, BSM.HerosInBattle.Count)];

        int num = Random.Range(0, enemy.attacks.Count);
        myAttack.choosenAttack = enemy.attacks[num];
   
        BSM.CollectActions(myAttack);
    }

    private IEnumerator TimeForAction()
    {
        if(actionStarted)
        {
            yield break;
        }
        
        actionStarted = true;

        //animate the enemy near the hero to attack
        Vector3 heroPosition = new Vector3(HeroToAttack.transform.position.x - 3f, HeroToAttack.transform.position.y, HeroToAttack.transform.position.z);
        while (MoveTowardsEnemy(heroPosition))
        {
            yield return null;
        }
        //wait a bit
        yield return new WaitForSeconds(0.5f);
        //do damage
        DoDamage();
        //animate back to the start position
        Vector3 firstPosition = startposition;
        while (MoveTowardsStart(firstPosition))
        {
            yield return null;
        }
        //remove this performe from the list in BSM
        BSM.PerformList.RemoveAt(0);
        //reset BSM to wait
        BSM.battleStates = BattleStateMachine.PerformAction.WAIT;
        //end coroutine
        actionStarted = false;
        //reset this enemy state
        cur_cooldown = 0f;
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
        float calc_damage = enemy.curAtk + BSM.PerformList[0].choosenAttack.attackDamage;
        HeroToAttack.GetComponent<HeroStateMachine>().TakeDamage(calc_damage) ;

    }

    public void TakeDamage(float getDamageAmount)
    {
        enemy.curHP -= getDamageAmount;
        if(enemy.curHP <= 0)
        {
            enemy.curHP = 0;
            currentState = TurnState.DEAD;
        }
    }
}
