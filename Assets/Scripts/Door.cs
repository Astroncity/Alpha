using UnityEngine;
using System;

public class Door : MonoBehaviour, IPopup{
    public bool connected;
    private bool locked;
    public GameObject windowBlocker;
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


    private void Update(){
        HandleOpen();
    }


    public void SetLock(bool isLocked){
        locked = isLocked;
        keypad1.locked.SetActive(locked);
        keypad1.unlocked.SetActive(!locked);

        keypad2.locked.SetActive(locked);
        keypad2.unlocked.SetActive(!locked);
    }


    public void EnableCol(){
        col.enabled = true;
    }


    public void HandleOpen(){
        if(!Input.GetKeyDown(KeyCode.E) || !inRange || locked) return;

        col.enabled = false;
        actionPopup = new ActionPopup("", ' ');
        SetLock(true);
        connected = false;
        
        Invoke(nameof(EnableCol), 0.1f);
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


    private void OnCollisionEnter(Collision col){
        if(col.gameObject.tag == "door"){
            collidingWithOtherDoor = true;
        }
    }

    private void OnCollisionExit(Collision col){
        if(col.gameObject.tag == "door"){
            collidingWithOtherDoor = false;
        }
    }


    private void OnDestroy(){
        room.GetComponent<Room>().doors.Remove(gameObject);
    }
    
}
