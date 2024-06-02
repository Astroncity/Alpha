using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPistol : Weapon {
    public int magSize = 10;
    // public ammo field with range 0 to ammo use [] to make it a slider
    [Range(0, 10)] public int ammo;
    public float reloadTime = 1.5f;
    public float fireRate = 0.5f;
    public float damage = 10;
    public float range = 5f;
    public float splashRange = 3f;
    public float splashDamageFalloff = 0.5f;
    public float rotOffsetY = -90;
    public bool laserEnabled = true;

    public Transform laserStart;
    public LineRenderer laser;
    public static ActionPopup popup = new ActionPopup("Grab Pistol", 'E');


    public void Start(){
        ammo = magSize;
        type = WeaponType.Ranged;
    }

    public override ActionPopup GetPopup(){
        return popup;
    }


    public override void Use(){
        if(Input.GetKeyDown(KeyCode.R)){
            reload();
        }
        if(Input.GetMouseButtonDown(0) && ammo > 0){
            shoot();
        }
        laserEnabled = true;
    }

    public override string AmmoInfo(){
        return ammo + " / " + magSize;
    }

    public void onEnemyHit(Enemy enemy, bool isSplash = false){
        if(!isSplash) enemy.Damage(damage);
        else enemy.Damage(damage * splashDamageFalloff);
    }


    public override float AmmoPercent(){
        return (float)ammo / (float)magSize;
    }

    public void LateUpdate(){
        if(laserEnabled) handleLaser();
        else{
            laser.SetPosition(0, laserStart.position);
            laser.SetPosition(1, laserStart.position);
        }
    }


    public void handleLaser(){
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if(Physics.Raycast(ray, out hit, range)){
            laser.SetPosition(0, laserStart.position);
            laser.SetPosition(1, hit.point);
        }
        else{
            laser.SetPosition(0, laserStart.position);
            laser.SetPosition(1, laserStart.position + Camera.main.transform.forward * range);
        }
        laserEnabled = false;
    }


    public void shoot(){
        RaycastHit hit;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if(Physics.Raycast(ray, out hit, range)){
            List<Collider> hitList = new List<Collider>(Physics.OverlapSphere(hit.point, splashRange));

            //*handle first hit
            if(hit.transform.tag == "Enemy"){
                onEnemyHit(hit.transform.GetComponent<Enemy>());
            }
            hit.rigidbody.AddExplosionForce(500, hit.point, 3);
            hitList.Remove(hit.collider);

            //*handle splash damage
            foreach(Collider c in hitList){
                if(c.gameObject.transform.tag == "Enemy"){
                    onEnemyHit(c.transform.GetComponent<Enemy>(), true);
                }
                c.attachedRigidbody.AddExplosionForce(250, hit.point, 3);
            }


            //* Particle effect
            Vector3 hitPoint = hit.point + hit.normal * 0.1f;
            GameObject effect = Instantiate(hitEffect, hitPoint, Quaternion.identity);
            Destroy(effect, 1);
        }
        ammo--;
    }

    public void reload(){
        ammo = magSize;
    }

}