using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BraveAttack : BaseAttack
{
    public BraveAttack()
    {
        attackName = "Brave";
        attackDescription = "Unleash a powerful physical attack.";
        attackDamage = 15f;
        attackCost = 0;    
    }
}
