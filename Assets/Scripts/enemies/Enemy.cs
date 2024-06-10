using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public enum AttackType{
    Normal,
    Goo,
    Electric,
    Fire,
    Ice
}


public abstract class Enemy : MonoBehaviour{
    public Room parentRoom;
    
    [Header("Health Bar")]
    public GameObject healthBar;
    public TextMeshProUGUI healthText;
    public GameObject canvas;

    [Header("Enemy Stats")]
    public float maxHealth;
    public float health;
    public float speed;
    protected static int count = 0;

    //? Holds global functions that need to be called outside of the class / subclasses
    public static List<List<Action>> thruManager = new();

    public abstract void Damage(float dmg);

    protected virtual void UpdateCanvas(){
        healthBar.transform.localScale = new Vector3(health / maxHealth, 1, 1);
        healthText.text = health + " / " + maxHealth;
        canvas.transform.LookAt(Camera.main.transform);
    }
    
    protected virtual void Start(){
        count++;
    }

    protected virtual void Update(){
        UpdateCanvas();
    }
    

    protected virtual void OnDestroy(){
        if(parentRoom == null) return;
        parentRoom.GetComponent<Room>().enemies.Remove(this);
        count--;
    }
}
