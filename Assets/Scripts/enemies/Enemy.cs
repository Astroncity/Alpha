using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Runtime.CompilerServices;

public abstract class Enemy : MonoBehaviour{
    public GameObject healthBar;
    public TextMeshProUGUI healthText;
    public GameObject canvas;
    public float maxHealth;
    public float health;
    public float speed;
    public static int count;
    public GameObject parentRoom;

    //! Dumb way to do this, but if it works, it works
    public static List<List<Action>> thruManager = new();

    public abstract void Damage(float dmg);
    public virtual void UpdateCanvas(){
        healthBar.transform.localScale = new Vector3(health / maxHealth, 1, 1);
        healthText.text = health + " / " + maxHealth;
        canvas.transform.LookAt(Camera.main.transform);
    }
    
    public virtual void Start(){
        count++;
    }

    public virtual void Update(){
        UpdateCanvas();
    }
    

    public virtual void OnDestroy(){
        if(parentRoom == null) return;
        parentRoom.GetComponent<Room>().enemies.Remove(this);
        count--;
    }
}
