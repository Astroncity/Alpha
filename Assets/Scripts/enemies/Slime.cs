using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class Slime : Enemy{
    public int fireRate = 2;
    private float delta = 0;
    public GameObject projectileP;
    public float projectileSpeed = 5;
    public float damage = 10;
    public float range = 5;

    public override void Update(){
        base.Update();

        if(health <= 0){
            Destroy(gameObject);
        }
        delta += Time.deltaTime;
        Attack();
    }


    public void Attack(){
        Vector3 spawnPos = transform.position + new Vector3(0, 2, 0);
        float distance = Vector3.Distance(transform.position, PlayerController.player.transform.position);
        if(delta > fireRate){
            GameObject projectile = Instantiate(projectileP, spawnPos, Quaternion.identity);
            projectile.transform.LookAt(PlayerController.player.transform.position);
            projectile.GetComponent<Rigidbody>().velocity = (PlayerController.player.transform.position - transform.position).normalized * projectileSpeed;
            delta = 0;
        }
    }




    public override void Damage(float dmg){
        health -= dmg;
    }

}
