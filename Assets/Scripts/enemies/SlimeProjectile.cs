using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeProjectile : MonoBehaviour{
    public float damage;


    public void OnCollisionEnter(Collision col){
        Grabbable holding = PlayerController.instance.holding;
        if(col.gameObject.tag == "Player" || holding != null && holding == col.gameObject.GetComponent<Grabbable>()){
            PlayerController.instance.health -= damage;
            Destroy(gameObject);
        }
        else{
            if(col.gameObject.GetComponent<SlimeProjectile>() == null){
                Destroy(gameObject);
            }
        }
        
        
    }

}
