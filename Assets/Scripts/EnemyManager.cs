using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour{

    public void Update(){
        foreach(List<Action> actions in Enemy.thruManager){
            foreach(Action action in actions){
                action();
            }
        }
    }
    
}
