using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseEnemy : BaseClass
{

    public enum Type
    {
        ELECTRIC,
        FIRE,
        WATER,
        ICE
    }

    public enum Rarity
    {
        COMMON,
        UNCOMMON,
        RARE,
        SUPERRARE
    }

    public Type EnemyType;
    public Rarity rarity; 
}
