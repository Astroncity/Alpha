using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeProjectile : MonoBehaviour{
    public float damage;

    public void Start(){
        Invoke("Stop", 3);
        Destroy(gameObject, 2);
    }


    public void Stop(){
        GetComponent<ParticleSystem>().Stop();
    }

    public void OnCollisionEnter(Collision col){
        if(col.gameObject.tag == "Player"){
            col.gameObject.GetComponent<PlayerController>().health -= damage;
        }
    }

}
