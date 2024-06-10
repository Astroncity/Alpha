using System.Collections.Generic;
using UnityEngine;



public class PrefabManager : MonoBehaviour{ //! This is technically a singleton
    public static PrefabManager inst;
    
    //? [BEGIN PREFAB DECLARATIONS] //?
    public List<GameObject> enemies;
        public GameObject slime; 
        public GameObject testEnemy;


    public void Awake(){
        inst = this;
        enemies = new List<GameObject>(){
            slime,
            testEnemy
        };
    }
}
