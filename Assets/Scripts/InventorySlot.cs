using System;
using UnityEngine.UI;
using UnityEngine;

[Serializable]
public class InventorySlot{
    [HideInInspector] public Grabbable item;
    [field: SerializeField] public RawImage icon{get; private set;}
    [SerializeField] private Image background;
    public Color selectedColor = new Color(167/255, 199/255, 1);

    
    public static void InitInventorySlots(InventorySlot[] inventorySlots, Vector2 thumbnailRectSize){
        foreach(InventorySlot slot in inventorySlots){
            //change image size to match thumbnail size but scale down to rect size
            slot.icon.GetComponent<RectTransform>()
            .sizeDelta = new Vector2(thumbnailRectSize.x, thumbnailRectSize.y);
            slot.Clear();
            slot.Deselect();
        }
    }

    public static (InventorySlot slot, int index) GetNextEmptySlot(InventorySlot[] slots){
        for(int i = 0; i < slots.Length; i++){
            if(slots[i].isEmpty()) return (slots[i], i);
        }
        return (null, -1);
    }


    public bool isEmpty(){
        return item == null;
    }

    public void Select(){
        background.color = selectedColor;
    }

    public void Deselect(){
        background.color = new Color(1, 1, 1, 1);
    }

    
    public void SetIcon(Texture tex){
        icon.texture = tex;
        icon.color = new Color(1, 1, 1, 1);
    }


    public void Clear(){
        item = null;
        icon.texture = null;
        icon.color = new Color(0, 0, 0, 0);
    }
}