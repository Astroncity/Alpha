using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Grabbable : MonoBehaviour, IPopup{
    public Rigidbody rb;
    public Collider[] colliders;
    public static float grabDistance = 5f;
    
    
    public abstract void Use();
    public abstract ActionPopup GetPopup();
    
    
    public virtual void Grab(){
        foreach(Collider c in colliders){c.enabled = false;}
        rb.isKinematic = true;
        transform.position = Camera.main.transform.GetChild(0).position;
        transform.SetParent(Camera.main.transform, true);
    }


    public virtual void Drop(){
        foreach(Collider c in colliders){c.enabled = true;}
        rb.isKinematic = false;
        transform.SetParent(null, true);
        PlayerController.player.GetComponent<PlayerController>().holding = null;
    }
}
