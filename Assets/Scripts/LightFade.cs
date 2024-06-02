using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFade : MonoBehaviour{
    public static float globalDist = 10f; 
    public bool useGlobalDist;
    public float dist;

    void Update(){

        // chjange to not include y dist
        if(useGlobalDist){
            dist = globalDist;
        }
        if(Vector3.Distance(transform.position, PlayerController.player.transform.position) < dist){
            GetComponent<Light>().enabled = false;
        }
        else{
            GetComponent<Light>().enabled = true;
        }
    }
}
