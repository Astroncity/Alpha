using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class Door : MonoBehaviour, IPopup{
    public bool connected;
    private bool locked;
    [HideInInspector] public bool collidingWithOtherDoor = false;
    [HideInInspector] public GameObject room;
    [SerializeField] private Keypad keypad1;
    [SerializeField] private Keypad keypad2;
    public static ActionPopup actionPopup = new("Break Door", 'E');
    public static float popupRange = 5f;
    private bool inRange = false;
    public BoxCollider col;


    [Serializable]
    private struct Keypad{
        public GameObject locked;
        public GameObject unlocked;
    }


    public void Update(){
        HandleOpen();
    }

    public void setLock(bool isLocked){
        locked = isLocked;
        keypad1.locked.SetActive(locked);
        keypad1.unlocked.SetActive(!locked);

        keypad2.locked.SetActive(locked);
        keypad2.unlocked.SetActive(!locked);
    }


    public void enableCol(){
        col.enabled = true;
    }



    public void HandleOpen(){
        if(!Input.GetKeyDown(KeyCode.E) || !inRange || locked) return;

        col.enabled = false;
        actionPopup = new ActionPopup("", ' ');
        setLock(true);
        connected = false;
        
        Invoke("enableCol", 0.1f);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;

        rb.AddForce(-transform.up * 100, ForceMode.Impulse);
        Destroy(gameObject, 1.5f);
    }


    public ActionPopup GetPopup(){
        if(Vector3.Distance(transform.position, PlayerController.player.transform.position) < popupRange && !locked){
            inRange = true;
            return actionPopup;
        }
        else{
            inRange = false;
            return new ActionPopup("", ' ');
        }
    }


    public void OnCollisionEnter(Collision col){
        if(col.gameObject.tag == "door"){
            collidingWithOtherDoor = true;
        }
    }

    public void OnCollisionExit(Collision col){
        if(col.gameObject.tag == "door"){
            collidingWithOtherDoor = false;
        }
    }


    public void OnDestroy(){
        room.GetComponent<Room>().doors.Remove(gameObject);
    }
    
}
