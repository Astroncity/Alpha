using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Grabbable : MonoBehaviour, IPopup{
    [Header("Grabbable Info")]
    public Rigidbody rb;
    public Collider[] colliders;
    public static float grabDistance = 5f;

    [Header("Icon Info")]
    public RenderTexture icon;
    public Quaternion iconRotation;
    public float iconDistanceMult = 1;
    
    
    public abstract void Use();
    public abstract ActionPopup GetPopup();
    
    
    public virtual Grabbable Grab(){
        foreach(Collider c in colliders){c.enabled = false;}
        rb.isKinematic = true;
        transform.position = Camera.main.transform.GetChild(0).position;
        transform.SetParent(Camera.main.transform, true);
        return this;
    }


    public virtual void Drop(){
        foreach(Collider c in colliders){c.enabled = true;}
        rb.isKinematic = false;
        transform.SetParent(null, true);
    }
}
