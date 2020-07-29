using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackButton : MonoBehaviour
{
    public BaseAttack magicAttackToPerform;

    public void CastMagicAttack()
    {
        BattleStateMachine BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        if(BSM.HerosToManage[0].GetComponent<HeroStateMachine>().hero.curMP > magicAttackToPerform.attackCost)
            BSM.Input4(magicAttackToPerform);
    }
}
