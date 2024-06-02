using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum WeaponType{
    None = 0,
    Ranged,
    Melee,
    OneTime
}

public abstract class Weapon : Grabbable{
    public GameObject hitEffect;
    public WeaponType type;

    
    public abstract string AmmoInfo();
    public abstract float AmmoPercent();
    

    public virtual void Sway(float smooth, float sway){
        float h = Input.GetAxisRaw("Mouse X") * sway;
        float v = Input.GetAxisRaw("Mouse Y") * sway;

        Debug.DrawRay(transform.position, transform.forward * 5, Color.red);

        Quaternion x = Quaternion.AngleAxis(-v, Vector3.right);
        Quaternion y = Quaternion.AngleAxis(h, Vector3.up);

        Quaternion targetRot = x * y;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, smooth * Time.deltaTime);
    }
    
}
