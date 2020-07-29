using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    //CLASS RANDOM ENEMY
    [System.Serializable]
    public class RegionData
    {
        public int maxAmountEnemies = 3;
        public List<GameObject> possibleEnemies = new List<GameObject>();
        public List<Transform> positionEnemies = new List<Transform>();
    }

    public RegionData RG;
    public List<GameObject> enemiesToBattle = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        StartBattle(); 
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void StartBattle()
    {
        //AMOUNT OF ENEMIES
        int enemyAmount = Random.Range(1, RG.maxAmountEnemies + 1);
        //WHICH ENEMIES
        for(int i = 0; i < enemyAmount; i++)
        {
            GameObject enemy = Instantiate(RG.possibleEnemies[Random.Range(0, RG.possibleEnemies.Count)], RG.positionEnemies[i].position, RG.positionEnemies[i].rotation);
            enemiesToBattle.Add(enemy);
        }
    }
}
