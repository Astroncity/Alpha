using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public struct ActionPopup{
    public string description;
    public char key;

    public ActionPopup(string description, char key){
        this.description = description;
        this.key = key;
    }
}


public class PlayerController : MonoBehaviour{
    public static GameObject player;
    
    [Header("Movement")]
    public float defSpeed = 5f;
    private float speed;
    public Rigidbody rb;
    public bool sprinting = false;
    private float defFOV;
    public float lookRange = 100f;
    public float groundDrag;
    public bool grounded = false;
    public bool jumping = false;
    private Vector3 inputDir;
    private float horizontalInput;
    private float verticalInput;

    
    [Header("Ammo Bar")]
    public TextMeshProUGUI ammoText;
    public GameObject ammoBar;
    public float fullBarPos;
    public float barWidth;
    
    [Header("Health Bar")]
    public float maxHealth = 100;
    public float health;
    public TextMeshProUGUI healthText;  
    public GameObject healthBar;
    public float fullHealthBarPos;
    public float healthBarWidth;

    [Header("Grabbables")]
    public static GameObject lookingAt;
    public TextMeshProUGUI actionDescription;
    public TextMeshProUGUI actionKey;
    public Image keyBackground;
    public Grabbable holding;

    void Start(){
        player = gameObject;
        health = maxHealth;
        defFOV = Camera.main.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        fullBarPos = ammoBar.GetComponent<RectTransform>().rect.x + ammoBar.GetComponent<RectTransform>().rect.width / 2;
        barWidth = ammoBar.GetComponent<RectTransform>().rect.width / 7.5f;
        fullHealthBarPos = healthBar.GetComponent<RectTransform>().rect.x + healthBar.GetComponent<RectTransform>().rect.width / 2;
        healthBarWidth = healthBar.GetComponent<RectTransform>().rect.width / 7.5f;
    }


    public void displayPopup(){
        if(lookingAt == null) return;

        IPopup p = lookingAt.GetComponent<IPopup>();
        if(p != null && p.GetPopup().description != "" && p.GetPopup().key != ' '){
            ActionPopup popup = lookingAt.GetComponent<IPopup>().GetPopup();
            actionDescription.text = popup.description;
            actionKey.text = popup.key.ToString();
            keyBackground.gameObject.SetActive(true);
        }
        else{
            actionDescription.text = "";
            actionKey.text = "";
            keyBackground.gameObject.SetActive(false);
        }
    }

    void Update(){
        //Debug.Log(holding?.name ?? "null");
        if(holding != null){
            if(holding is Weapon) HandleWeapon();
            if(holding is Throwable) holding.Use();
        }

        handleLook();
        displayPopup();
        Move();
        HandleHealthBar();

        if(Input.GetKeyDown(KeyCode.T)){
            holding?.Drop();
        }

        if(Input.GetKeyDown(KeyCode.E)){
            if(lookingAt != null && Vector3.Distance(transform.position, lookingAt.transform.position) < Grabbable.grabDistance){
                Grabbable p = lookingAt.GetComponent<Grabbable>();
                if(p != null){
                    holding?.Drop();
                    p.Grab();
                    holding = p;
                }
            }
        }

        //Debug.Log("Enemies: " + Enemy.count.ToString());
        //Debug.Log(lookingAt?.name ?? "null");
    }

    void FixedUpdate(){
        ApplyMovementForce(inputDir, speed);
        Jump();
    }


    void handleLook(){
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, lookRange, ~LayerMask.GetMask("Player"))){
            lookingAt = hit.collider.gameObject;
        }
        else{
            lookingAt = null;
        }
    }


    void HandleWeapon(){
        Weapon wep = (Weapon)holding;
        wep.Use();
        wep.Sway(8, 2);
        ammoText.text = wep.AmmoInfo();

        //* Update bar;
        // move bar to the left
        RectTransform rt = ammoBar.GetComponent<RectTransform>();
        float ycomp = rt.localPosition.y;
        rt.localPosition = Vector3.Lerp(rt.localPosition, new Vector3(fullBarPos - barWidth + (barWidth * wep.AmmoPercent()), ycomp, 0), 7f * Time.deltaTime);
    }


    void HandleHealthBar(){
        RectTransform rt = healthBar.GetComponent<RectTransform>();
        float ycomp = rt.localPosition.y;
        rt.localPosition = Vector3.Lerp(rt.localPosition, new Vector3(fullHealthBarPos - healthBarWidth + (healthBarWidth * health/maxHealth), ycomp, 0), 7f * Time.deltaTime);
        healthText.text = health + " / " + maxHealth;
    }


    void ApplyMovementForce(Vector3 dir, float speed){
        rb.AddForce(dir * speed * 10f, ForceMode.Force);
    }

    void Jump(){
        if(grounded && jumping){
            rb.AddForce(Vector3.up * 20, ForceMode.Impulse);
        }
    }


    void Move(){
        bool moving = false;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(KeyCode.LeftShift)){ 
            speed = defSpeed * 2;
            sprinting = true;
        }
        else{ 
            speed = defSpeed;
            sprinting = false;
        }


        jumping = Input.GetKey(KeyCode.Space);

        //check ground
        grounded = Physics.Raycast(transform.position, -transform.up, 1.1f);
        //extra gravity
        if(!grounded) rb.AddForce(Vector3.down * 5, ForceMode.Force);
        if(grounded || !grounded) rb.drag = groundDrag;
        else rb.drag = 0;

        inputDir = transform.forward * verticalInput + transform.right * horizontalInput;
        moving = inputDir.magnitude > 0;

        if(moving){
            if(speed != defSpeed){ //sprint
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, defFOV * 1.15f, 7f * Time.deltaTime);
            }
            else{ //walk
                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, defFOV * 1.05f, 7f * Time.deltaTime);
            }
        }
        else{ //static
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, defFOV, 7f * Time.deltaTime);
        }

        transform.rotation = new Quaternion(0, Camera.main.transform.rotation.y, 0,  Camera.main.transform.rotation.w);
    }
}
