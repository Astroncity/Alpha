using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;


[System.Serializable]
public class DebuffUIPackage{
    public Image[] images;
    public TextMeshProUGUI timeText;
}


public static class DebuffDefinitions{
    public static PlayerController player;
    public static Vignette vignette;
    public static float debuffIntensity = 0;
    private readonly static float lerpRate = 0.5f;


    public static void GetDebuff(AttackType type){
        if(!Valid()) return;

        switch(type){
            case AttackType.Goo:
                GooDebuff();
                break;
            case AttackType.Electric:
                ElectricDebuff();
                break;
        }
    }


    private static bool Valid(){
        if(player is null || vignette is null){
            Debug.LogError("Player or Vignette is null");
            return false;
        }
        return true;
    }


    private static void HandleVignette(Color color){
        vignette.color.value = Color.Lerp(vignette.color.value, color, lerpRate * Time.deltaTime);
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, debuffIntensity, lerpRate * Time.deltaTime);
    }


    private static void GooDebuff(){
        player.health -= 0.025f;
        HandleVignette(Color.green);
    }


    private static void ElectricDebuff(){
        player.health -= 0.5f;
        if(!player.electrified) player.Drop();
        player.electrified = true;
        HandleVignette(Color.blue);
    }
}