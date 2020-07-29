using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire1Spell : BaseAttack
{
    public Fire1Spell()
    {
        attackName = "Fire 1";
        attackDescription = "Damages one foe with a weak fire attack.";
        attackDamage = 12f;
        attackCost = 2f;
    }
}
