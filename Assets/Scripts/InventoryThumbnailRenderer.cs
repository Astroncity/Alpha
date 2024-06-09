using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class InventoryThumbnailRenderer{
    private GameObject camObj;
    private Camera cam;
    private Transform offset;
    private VolumeProfile volume;
    private float defExp = 0;
    private float defsat = 0;


    public InventoryThumbnailRenderer(GameObject camObj, VolumeProfile volume){
        this.camObj = camObj;
        cam = camObj.GetComponent<Camera>();
        offset = camObj.transform.GetChild(0).transform;
        this.volume = volume;
    }


    public RenderTexture Render(Vector2Int imageSize, Grabbable orgObj){
        Debug.Log("Generated thumbnail for " + orgObj.name + "!");
        RenderTexture texture = new RenderTexture(imageSize.x, imageSize.y, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Quaternion temp = orgObj.iconRotation;
        
        GameObject newObj = MonoBehaviour.Instantiate(orgObj.gameObject, offset.position, Quaternion.identity, cam.transform);
        newObj.transform.localRotation = temp;
        cam.orthographicSize += orgObj.iconDistanceMult / 100;
        newObj.layer = LayerMask.NameToLayer("InventoryRendering");
        foreach(Transform t in newObj.GetComponentsInChildren<Transform>()){
            t.gameObject.layer = LayerMask.NameToLayer("InventoryRendering");
        }
        cam.targetTexture = texture;
        volume.TryGet<ColorAdjustments>(out ColorAdjustments color);
        PrePost(color);
        cam.Render();
        PostPost(color);
        //MonoBehaviour.Destroy(newObj);
        cam.targetTexture = null;
        return texture;        
    }


    private void PrePost(ColorAdjustments color){
        defExp = color.postExposure.value;
        defsat = color.saturation.value;
        color.postExposure.value = 1;
        color.saturation.value = 100;
    }

    private void PostPost(ColorAdjustments color){
        color.postExposure.value = defExp;
        color.saturation.value = defsat;
    }
}
