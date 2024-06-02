using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class Enemy : MonoBehaviour{
    public static int count;
    public GameObject parentRoom;
    public abstract void Damage(float dmg);
    
    public void OnDestroy(){
        if(parentRoom == null) return;
        parentRoom.GetComponent<Room>().enemies.Remove(this);
        count--;
    }
}
