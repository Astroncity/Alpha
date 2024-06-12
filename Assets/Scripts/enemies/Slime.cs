using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Slime : Enemy{
    public static List<Slime> slimes = new();
    [SerializeField] private GameObject slimeObj;

    [Header("Slime Stats")]
    public GameObject projectileP;
    public AttackType attackType;
    public int fireRate = 2;
    public float projectileSpeed = 5;
    public float damage = 10;
    public float range = 5;
    private float attackTimer = 0;

    [Header("Combine Skill")]
    public bool super = true;
    public float combineTime = 7;
    public float combineRange = 5;
    public int combineThreshold = 3;
    private static List<Action> managerFuncs = null;
    private float combineTimer;


    protected override void Start(){
        base.Start();
        slimes.Add(this);
        combineThreshold--;

        InitManagerFuncs();

        if(!super){
            maxHealth /= 2;
            health = maxHealth;
            damage /= 2;
            transform.localScale /= 2;
        }

    }


    private void InitManagerFuncs(){
        if(managerFuncs == null){
            managerFuncs = new List<Action>(){
                ExternalHandleCombines
            };
            Enemy.thruManager.Add(managerFuncs);
        }
    }


    protected override void Update(){
        
        base.Update();
        HandleDeath();
        Attack();
    }


    public void HandleDeath(){
        if(health <= 0){
            if(super) CreateSlimesOnDeath();
            Destroy(gameObject);
        }
    }


    private List<Slime> GetValidSlimes(){
        List<Slime> validSlimes = new();

        foreach(Slime s in slimes){
            if(s == this) continue;
            if(Vector3.Distance(transform.position, s.transform.position) < combineRange){
                if(s.attackType == this.attackType && !s.super){
                    validSlimes.Add(s);
                }
            }
        }

        //* leave only combineThreshold - 1 closest slimes
        validSlimes = validSlimes.OrderBy(s => Vector3.Distance(transform.position, s.transform.position)).ToList();
        if(validSlimes.Count > combineThreshold){
            validSlimes.RemoveRange(combineThreshold, validSlimes.Count - combineThreshold + 1);
        } 

        return validSlimes;
    }


    private static void ExternalHandleCombines(){
        List<Slime> currSlimes = new(slimes);

        for(int i = 0; i < currSlimes.Count; i++){
            Slime s = currSlimes[i];
            if(!s.super) s.combineTimer += Time.deltaTime;
            if(s.combineTimer < s.combineTime) continue;

            List<Slime> validSlimes = s.GetValidSlimes();
            if(validSlimes.Count < s.combineThreshold) continue;
            validSlimes.Add(s);

            Vector3 avgPos = GetAveragePosition(validSlimes);
            avgPos += new Vector3(0, 2, 0);

            if(Physics.OverlapSphere(avgPos, 1).Length > 0) continue;

            Slime newSlime = Instantiate(PrefabManager.inst.slime, avgPos, Quaternion.identity).GetComponent<Slime>();
            s.parentRoom.AddEnemy(newSlime);

            foreach(Slime a in validSlimes){
                Destroy(a.gameObject);
                currSlimes.Remove(a);
            }
            i--;
        }
    }


    private static Vector3 GetAveragePosition(List<Slime> slimes){
        Vector3 avgPos = Vector3.zero;
        foreach(Slime s in slimes){
            avgPos += s.transform.position;
        }
        avgPos /= slimes.Count;
        return avgPos;
    }


    public void Attack(){
        transform.LookAt(PlayerController.player.transform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        attackTimer += Time.deltaTime;
        Vector3 spawnPos = transform.position + new Vector3(0, 1.5f, 0);
        spawnPos += transform.forward * 0.5f;
        float distance = Vector3.Distance(transform.position, PlayerController.player.transform.position);

        if(attackTimer > fireRate && distance < range){
            Vector3 vel = PlayerController.player.transform.position - transform.position;
            vel = vel.normalized * projectileSpeed;
            SlimeProjectile projectile = Instantiate(projectileP, spawnPos, Quaternion.identity).GetComponent<SlimeProjectile>();
            projectile.Init(damage, attackType, vel, super);
            attackTimer = 0;
        }
    }


    public void CreateSlimesOnDeath(){
        Vector3 spawnPos = transform.position;

        for(int i = 0; i < 3; i++){
            Slime s = Instantiate(PrefabManager.inst.slime, transform.position, Quaternion.identity).GetComponent<Slime>();
            s.super = false;
            parentRoom.AddEnemy(s);
            float range = 2;
            spawnPos += new Vector3(UnityEngine.Random.Range(-range, range), 0, UnityEngine.Random.Range(-range, range));
        }
    }


    protected override void OnDestroy(){
        base.OnDestroy();
        slimes.Remove(this);
    }

    public override void Damage(float dmg){
        health -= dmg;
    }

}
