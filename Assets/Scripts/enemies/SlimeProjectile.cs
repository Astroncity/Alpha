using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeProjectile : MonoBehaviour{
    public float damage;
    public AttackType type;


    public void OnCollisionEnter(Collision col){
        Grabbable holding = PlayerController.instance.holding;
        if(col.gameObject.tag == "Player" || holding != null && holding == col.gameObject.GetComponent<Grabbable>()){
            PlayerController.instance.Damage(damage, type);
            Destroy(gameObject);
        }
        else{
            if(col.gameObject.GetComponent<SlimeProjectile>() == null){
                Destroy(gameObject);
            }
        }
        
        
    }

}
