using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class HandleTurn
{
    public string Attacker; //name of the attacker
    public string Type;
    public GameObject AttackersGameObject;
    public GameObject AttackersTarget; //who is going ti be attacked

    //which attack is performed
    public BaseAttack choosenAttack;

}
