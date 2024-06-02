using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : Grabbable{
    public float throwForce = 50;
    public float damage = 5;
    public GameObject hitEffect;
    public AudioClip explosionSound;
    public string actionMsg = "Grab [NAME]";
    public static char actionKey = 'E';
    public bool thrown = false;


    public override void Use(){
        if(Input.GetMouseButtonUp(0)){
            Drop();
            rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);
            thrown = true;
        }
    }


    public override ActionPopup GetPopup(){
        return new ActionPopup(actionMsg, actionKey);
    }


    public void OnCollisionEnter(Collision other){
        if(!thrown) return;

        if(other.gameObject.CompareTag("Enemy")){
            other.gameObject.GetComponent<Enemy>().Damage(1);
        }

        if(hitEffect != null){
            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2);

            LiquidColorChanger changer = GetComponent<LiquidColorChanger>();
            changer?.SetEffectColor(effect);
        }

        if(explosionSound != null){
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        Destroy(gameObject);
    }
}
