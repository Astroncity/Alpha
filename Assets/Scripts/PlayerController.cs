using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
    public static PlayerController instance;
    
    [Header("Movement")] [Space(10)]
    public float defSpeed;
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

    
    [Header("Ammo Bar")] [Space(10)]
    public TextMeshProUGUI ammoText;
    public GameObject ammoBar;
    public float fullBarPos;
    public float barWidth;

    [Header("Weapons")] [Space(10)]
    public TextMeshProUGUI reloadTimeText;
    public Image reloadTimeBar;
    public Image reloadTimeBarBackground;
    
    [Header("Health")] [Space(10)]
    public float maxHealth = 100;
    public float health;
    public TextMeshProUGUI healthText;  
    public GameObject healthBar;
    public float fullHealthBarPos;
    public float healthBarWidth;

    public Dictionary<AttackType, float> debuffs = new();
    [Header("Debuffs")] [Space(10)]
    public DebuffUIPackage[] debuffUIPackages; //* in order of AttackType
    private Vignette vignette;
    public VolumeProfile volumeProfile;
    public float vingnetteDefIntensity;
    public Color vignetteDefColor;

    [Header("Grabbables")] [Space(10)]  
    public static GameObject lookingAt;
    public TextMeshProUGUI actionDescription;
    public Grabbable holding;
    //! temp
    public RawImage testInvetoryIcon;
    public Camera inventoryCam;
    public InventoryThumbnailRenderer thumbnailRenderer;
    [SerializeField] private Vector2Int thumbnailSize = new Vector2Int(48 << 5, 64 << 5);
    private Vector2Int thumbnailRectSize = new Vector2Int(48, 64);
    private bool setThumbnail = false;
    //! end temp
    public bool electrified = false;

    public void Start(){
        volumeProfile.TryGet(out vignette);
        vingnetteDefIntensity = vignette.intensity.value;
        vignetteDefColor = vignette.color.value;
        instance = this;
        player = gameObject;
        health = maxHealth;
        defFOV = Camera.main.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        fullBarPos = ammoBar.GetComponent<RectTransform>().rect.x + ammoBar.GetComponent<RectTransform>().rect.width / 2;
        barWidth = ammoBar.GetComponent<RectTransform>().rect.width / 7.5f;
        fullHealthBarPos = healthBar.GetComponent<RectTransform>().rect.x + healthBar.GetComponent<RectTransform>().rect.width / 2;
        healthBarWidth = healthBar.GetComponent<RectTransform>().rect.width / 7.5f;

        //!temp
        thumbnailRenderer = new InventoryThumbnailRenderer(inventoryCam.gameObject, volumeProfile);
        RectTransform rt = testInvetoryIcon.GetComponent<RectTransform>();
        //change image size to match thumbnail size but scale down to rect size
        rt.sizeDelta = new Vector2(thumbnailRectSize.x, thumbnailRectSize.y);

        // -- DEBUFF SETUP --
        DebuffDefinitions.player = this;
        DebuffDefinitions.vignette = vignette;
        DebuffDefinitions.debuffIntensity = vignette.intensity.value * 0.9f;
        InitDebuffs();
    }



    public void displayPopup(){
        if(lookingAt == null) return;

        IPopup p = lookingAt.GetComponent<IPopup>();
        if(p != null && p.GetPopup().description != "" && p.GetPopup().key != ' '){
            ActionPopup popup = lookingAt.GetComponent<IPopup>().GetPopup();
            actionDescription.text = "[" + popup.key + "]-" + popup.description;
        }
        else{
            actionDescription.text = "";
        }
    }

    public void Update(){
        //Debug.Log(holding?.name ?? "null");
        if(holding != null){
            if(holding.icon == null) holding.icon = thumbnailRenderer.Render(thumbnailSize, holding);
            if(!setThumbnail){
                testInvetoryIcon.texture = holding.icon;
                setThumbnail = true;
            }
            if(holding is Weapon) HandleWeapon();
            if(holding is Throwable) holding.Use();
        }

        handleLook();
        displayPopup();
        Move();
        HandleHealthBar();
        HandleDebuffs();

        if(Input.GetKeyDown(KeyCode.T)){
            holding?.Drop();
            setThumbnail = false;
        }

        if(!electrified && Input.GetKeyDown(KeyCode.E)){
            Grab();
        }

        //Debug.Log("Enemies: " + Enemy.count.ToString());
        //Debug.Log(lookingAt?.name ?? "null");
    }


    private void Grab(){
        if(lookingAt != null && Vector3.Distance(transform.position, lookingAt.transform.position) < Grabbable.grabDistance){
            Grabbable p = lookingAt.GetComponent<Grabbable>();
            if(p != null){
                holding?.Drop();
                p.Grab();
                holding = p;
            }
        }
    }

    public void FixedUpdate(){
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

        // update reloading
        if(wep.reloading){
            reloadTimeBar.enabled = true;
            reloadTimeBarBackground.enabled = true;
            reloadTimeText.text = (wep.reloadTime - wep.reloadTimer).ToString("F1") + "s";
            reloadTimeBar.fillAmount = wep.reloadTimer / wep.reloadTime;
        }
        else{
            reloadTimeText.text = "";
            reloadTimeBar.fillAmount = 0;
            reloadTimeBar.enabled = false;
            reloadTimeBarBackground.enabled = false;
        }    
    }


    void HandleHealthBar(){
        RectTransform rt = healthBar.GetComponent<RectTransform>();
        float ycomp = rt.localPosition.y;
        rt.localPosition = Vector3.Lerp(rt.localPosition, new Vector3(fullHealthBarPos - healthBarWidth + (healthBarWidth * health/maxHealth), ycomp, 0), 7f * Time.deltaTime);
        healthText.text = health.ToString("F0") + " / " + maxHealth;
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

    private void HandleDebuffs(){
        bool debuffActive = false;
        bool seenElectric = false;
        for(int i = 0; i < debuffUIPackages.Length; i++){
            AttackType debuff = (AttackType)i + 1;
            if(debuffs[debuff] <= 0){
                HandleDebuffPackage(debuff, false, 0);
                continue;
            }
            debuffActive = true;
            debuffs[debuff] -= Time.deltaTime;
            DebuffDefinitions.GetDebuff(debuff);
            HandleDebuffPackage(debuff, true, debuffs[debuff]);

            if(debuff == AttackType.Electric){
                seenElectric = true;
            }
        }

        electrified = seenElectric;

        if(!debuffActive){
            ResetVignette();
        }
    }


    void HandleDebuffPackage(AttackType type, bool active, float dur){
        int index = (int)type - 1;
        if(index > debuffUIPackages.Length - 1) return; //! Temp while not all debuffs are implemented
        DebuffUIPackage package = debuffUIPackages[(int)type - 1];
        float opacityMul = active ? 1 : 0.25f;

        foreach(Image img  in package.images){
            img.color = new Color(img.color.r, img.color.g, img.color.b, opacityMul);
        }

        if(dur == 0){
            package.timeText.text = "N/A";
        }
        else{
            package.timeText.text = dur.ToString("F1");
        }
    }


    public void ResetVignette(){
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, vingnetteDefIntensity, 7f * Time.deltaTime);
        vignette.color.value = Color.Lerp(vignette.color.value, vignetteDefColor, 7f * Time.deltaTime);
    }


    private void AddDebuff(AttackType type, float duration){
        if(debuffs[type] < duration){
            debuffs[type] = duration;
        }
    }

    private void InitDebuffs(){
        for(int i = 1; i < Enum.GetValues(typeof(AttackType)).Length; i++){
            debuffs.Add((AttackType)i, 0);
        }

    }



    public void Damage(float damage, AttackType type){
        health -= damage;
        float tempDuration = 5f; //! temp
        if(type != AttackType.Normal) AddDebuff(type, tempDuration);
        if(health <= 0){
            //die
        }
    }


    private void OnDestroy(){
        vignette.intensity.value = vingnetteDefIntensity;
        vignette.color.value = vignetteDefColor;
    }

}
