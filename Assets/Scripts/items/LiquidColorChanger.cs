using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidColorChanger : MonoBehaviour{
    public Material material;
    public GameObject liquid;
    public Throwable throwable;
    public Color c;

    public void Start(){
        c = new Color(Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), 1f);
        //normalize color
        c /= c.maxColorComponent;
        SetObjColor();
    }

    
    public void SetEffectColor(GameObject effect){
        Material effectMat = effect.GetComponent<ParticleSystemRenderer>().material;
        Debug.Log(effectMat.name);
        Material mat = new(effectMat);
        mat.SetColor("_EmissionColor", c * 4f);
        mat.SetColor("_Color", c);
        effect.GetComponent<ParticleSystemRenderer>().material = mat;
    }


    public void SetObjColor(){
        Material mat = new(material);
        mat.SetColor("_EmissionColor", c * 4f);
        mat.SetColor("_Color", c);
        liquid.GetComponent<MeshRenderer>().material = mat;
        RandomizeRotation();
    }


    public void RandomizeRotation(){
        transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
    }
}
