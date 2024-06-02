using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour{
    public float rotationX;
    public float rotationY;
    public float maxAngle;
    public float minAngle;


    void Update(){
        transform.position = PlayerController.player.transform.position + new Vector3(0, 1f, 0);
        
        float xr = Input.GetAxis("Mouse X"); float yr = -Input.GetAxis("Mouse Y");
        rotationX += yr; rotationY += xr;

        transform.rotation = Quaternion.Euler(new Vector3(rotationX, rotationY, 0));

        if(rotationX > maxAngle){
            rotationX = Mathf.Lerp(rotationX, maxAngle, 0.05f * Time.deltaTime * 70);
        }
        else if(rotationX < minAngle) {
            rotationX = Mathf.Lerp(rotationX, minAngle, 0.05f * Time.deltaTime * 70);
        }

        if(rotationY > 360) rotationY = 0;
        else if(rotationY < 0) rotationY = 360;
    }
}
