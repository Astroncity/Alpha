using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestEnemy : Enemy{
    public GameObject healthBar;
    public TextMeshProUGUI healthText;
    public GameObject canvas;
    public float maxHealth = 100;
    public float health;
    public float speed = 5;
    public LineRenderer line;

    public override void Damage(float dmg){
        health -= dmg;
    }

    void UpdateCanvas(){
        healthBar.transform.localScale = new Vector3(health / maxHealth, 1, 1);
        healthText.text = health + " / " + maxHealth;
        canvas.transform.LookAt(Camera.main.transform);
    }


    public void Start(){
        count++;
    }


    void Update(){
        UpdateCanvas();
        if(health <= 0){
            Destroy(gameObject);
        }

        line.SetPosition(0, transform.position); 
        line.SetPosition(1, parentRoom.transform.position + new Vector3(0, 10, 0)); 
    }


    void OnCollisionEnter(Collision col){
        if(col.gameObject.tag == "Player"){
            col.gameObject.GetComponent<PlayerController>().health -= 10;
        }
    }

}
