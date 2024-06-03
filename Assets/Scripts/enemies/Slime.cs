using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public enum AttackType{
    Goo,
    Electric,
    Fire,
    Ice
}

public class Slime : Enemy{
    public static List<Slime> slimes = new();
    public int fireRate = 2;
    private float delta = 0;
    public GameObject projectileP;
    public float projectileSpeed = 5;
    public float damage = 10;
    public float range = 5;
    public bool super = true;
    private float combineTimer;
    public float combineTime = 7;
    public float combineRange = 5;
    public int combineThreshold = 3;
    public AttackType attackType;
    public static List<Action> managerFuncs = null;


    public override void Start(){
        if(managerFuncs == null){
            managerFuncs = new List<Action>(){
                HandleCombines
            };
            Enemy.thruManager.Add(managerFuncs);
        }

        base.Start();
        slimes.Add(this);

        if(!super){
            maxHealth /= 2;
            health = maxHealth;
            damage /= 2;
            transform.localScale /= 2;
        }
    }
    

    public override void Update(){
        base.Update();

        if(health <= 0){
            if(super) CreateSlimesOnDeath();
            Destroy(gameObject);
        }
        delta += Time.deltaTime;
        Attack();
    }


    public List<Slime> GetValidSlimes(){
        List<Slime> validSlimes = new();
        foreach(Slime s in slimes){
            if(s == this) continue;
            if(Vector3.Distance(transform.position, s.transform.position) < combineRange){
                if(s.attackType == attackType && !s.super){
                    validSlimes.Add(s);
                }
            }
        }

        //leave only combineThreshold - 1 closest slimes
        validSlimes = validSlimes.OrderBy(s => Vector3.Distance(transform.position, s.transform.position)).ToList();
        if(validSlimes.Count > combineThreshold - 1){
            validSlimes.RemoveRange(combineThreshold - 1, validSlimes.Count - combineThreshold + 1);
        } 

        return validSlimes;
    }


    public static void HandleCombines(){
        List<Slime> currSlimes = new(slimes);
        for(int i = 0; i < currSlimes.Count; i++){
            Slime s = currSlimes[i];
            if(s.super) continue;

            Transform transform = s.transform;
            s.combineTimer += Time.deltaTime;
            if(s.combineTimer < s.combineTime) return;

            List<Slime> validSlimes = s.GetValidSlimes();
            if(validSlimes.Count < s.combineThreshold - 1) return;

            //get average position, check if colliding with anything, if colliding, return
            Vector3 avgPos = Vector3.zero;
            foreach(Slime a in validSlimes){
                avgPos += a.transform.position;
            }
            avgPos += transform.position;
            avgPos /= validSlimes.Count + 1;
            avgPos += new Vector3(0, 2, 0); //? offset to avoid colliding with ground

            if(Physics.OverlapSphere(avgPos, 1).Length > 0) return;

            Instantiate(PrefabManager.instance.slime, avgPos, Quaternion.identity);
            foreach(Slime a in validSlimes){
                Destroy(a.gameObject);
            }
            Destroy(s.gameObject);

            currSlimes.Remove(s);
            foreach(Slime a in validSlimes){
                currSlimes.Remove(a);
            }
            i--;
        }
    }


    public void Attack(){
        Vector3 spawnPos = transform.position + new Vector3(0, 2, 0);
        float distance = Vector3.Distance(transform.position, PlayerController.player.transform.position);
        if(delta > fireRate && distance < range){
            GameObject projectile = Instantiate(projectileP, spawnPos, Quaternion.identity);
            if(!super) projectile.transform.localScale /= 2;  
            projectile.transform.LookAt(PlayerController.player.transform.position);
            projectile.GetComponent<Rigidbody>().velocity = (PlayerController.player.transform.position - transform.position).normalized * projectileSpeed;
            delta = 0;
        }
    }


    public void CreateSlimesOnDeath(){
        Vector3 spawnPos = transform.position;
        for(int i = 0; i < 3; i++){
            Slime s = Instantiate(PrefabManager.instance.slime, transform.position, Quaternion.identity).GetComponent<Slime>();
            s.super = false;
            s.parentRoom = parentRoom;
            float b = 2;
            spawnPos += new Vector3(UnityEngine.Random.Range(-b, b), 0, UnityEngine.Random.Range(-b, b));
        }
    }


    public override void OnDestroy(){
        base.OnDestroy();
        slimes.Remove(this);
    }

    public override void Damage(float dmg){
        health -= dmg;
    }

}
