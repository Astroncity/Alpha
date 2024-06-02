using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestEnemy : Enemy{
    public LineRenderer line;

    public override void Damage(float dmg){
        health -= dmg;
    }


    public override void Update(){
        base.Update();
        if(health <= 0){
            Destroy(gameObject);
        }

        line.SetPosition(0, transform.position); 
        line.SetPosition(1, parentRoom.transform.position + new Vector3(0, 10, 0)); 
    }


    public void OnCollisionEnter(Collision col){
        if(col.gameObject.tag == "Player"){
            col.gameObject.GetComponent<PlayerController>().health -= 10;
        }
    }

}
