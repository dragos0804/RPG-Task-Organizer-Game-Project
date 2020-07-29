using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : BaseAttack
{
    public Slash()
    {
        attackName = "Slash";
        attackDescription = "Unleash a fast attack with the equipped weapon.";
        attackDamage = 10f;
        attackCost = 0;
    }
}
