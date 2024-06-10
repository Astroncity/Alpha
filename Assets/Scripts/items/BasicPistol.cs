using System.Collections.Generic;
using UnityEngine;

public class BasicPistol : Weapon{
    [Header("Pistol Stats")]
    public int magSize = 10;
    public int ammo;
    public float fireRate = 0.5f;
    public float damage = 10;
    public float range = 50f;
    public float splashRange = 3f;
    public float splashDamageFalloff = 0.5f;
    

    [Header("Laser Info")]
    public bool laserEnabled = true;
    public Transform laserStart;
    public LineRenderer laser;
    public GameObject laserLight;

    private static ActionPopup popup = new("Grab[Pistol]", 'E');
    public override ActionPopup GetPopup(){return popup;}


    private void Start(){
        ammo = magSize;
        type = WeaponType.Ranged;
    }


    public override void Use(){
        if(Input.GetKeyDown(KeyCode.R)){
            reloading = true;
            Invoke("Reload", reloadTime);
        }
        if(Input.GetMouseButtonDown(0) && ammo > 0 && !reloading){
            Shoot();
        }

        if(reloading) reloadTimer += Time.deltaTime;
        laserEnabled = true;
    }

    public override string AmmoInfo(){return ammo + " / " + magSize;}

    private void OnEnemyHit(Enemy enemy, bool isSplash = false){
        if(!isSplash) enemy.Damage(damage);
        else enemy.Damage(damage * splashDamageFalloff);
    }


    public override float AmmoPercent(){
        return (float)ammo / (float)magSize;
    }

    private void LateUpdate(){
        HandleLaser();
    }


    private void HandleLaser(){
        if(laserEnabled){
            RaycastHit hit;
            Vector3 rayPos = new Vector3(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(rayPos, 0);
            laserLight.SetActive(true);

            if(Physics.Raycast(ray, out hit, range)){
                laser.SetPosition(0, laserStart.position);
                laser.SetPosition(1, hit.point);
                laserLight.transform.position = hit.point;
                //move back a bit to prevent clipping
                laserLight.transform.position -= Camera.main.transform.forward * 0.1f;
            }
            else{
                Vector3 inf = Camera.main.transform.forward * range;
                laser.SetPosition(0, laserStart.position);
                laser.SetPosition(1, inf);
                laserLight.transform.position = inf;
            }
            laserEnabled = false;
        }
        else{
            laser.SetPosition(0, laserStart.position);
            laser.SetPosition(1, laserStart.position);
            laserLight.SetActive(false);
        }
    }


    private void Shoot(){
        RaycastHit hit;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        ammo--;

        if(Physics.Raycast(ray, out hit, range)){
            List<Collider> hitList = new(Physics.OverlapSphere(hit.point, splashRange));

            // handle first hit
            if(hit.transform.tag == "Enemy"){
                OnEnemyHit(hit.transform.GetComponent<Enemy>());
            }
            hit.rigidbody.AddExplosionForce(500, hit.point, 3);
            hitList.Remove(hit.collider);

            // handle splash damage
            foreach(Collider c in hitList){
                if(c.gameObject.transform.tag == "Enemy"){
                    Enemy enemy = c.transform.GetComponent<Enemy>();
                    if(enemy is null) c.transform.GetComponentInParent<Enemy>();
                    OnEnemyHit(enemy, true);
                }
                c.attachedRigidbody.AddExplosionForce(250, hit.point, 3);
            }


            // Particle effect
            Vector3 hitPoint = hit.point + hit.normal * 0.1f;
            GameObject effect = Instantiate(hitEffect, hitPoint, Quaternion.identity);
            Destroy(effect, 1);
        }
    }

    private void Reload(){
        ammo = magSize;
        reloading = false;
        reloadTimer = 0;
    }

}
