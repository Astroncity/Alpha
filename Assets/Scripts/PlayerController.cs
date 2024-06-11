using System;
using System.Collections.Generic;
using TMPro;
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

    [Header("Debuffs")] [Space(10)]
    public DebuffUIPackage[] debuffUIPackages; //* in order of AttackType
    public Dictionary<AttackType, float> debuffs = new();
    private Vignette vignette;
    public VolumeProfile volumeProfile;
    public float vingnetteDefIntensity;
    public Color vignetteDefColor;

    [Header("Grabbables")] [Space(10)]  
    public TextMeshProUGUI actionDescription;
    public static GameObject lookingAt;
    public Image actionBackground;
    public Grabbable holding;
    
    [Header("Inventory Handling")] [Space(10)]
    public InventorySlot[] inventorySlots;
    private int currentSlot = 0;
    public Camera inventoryCam;
    public InventoryThumbnailRenderer thumbnailRenderer;
    public Vector2Int thumbnailSize = new Vector2Int(64 << 5, 64 << 5);
    private Vector2Int thumbnailRectSize = new Vector2Int(32, 32);
    public bool electrified = false;


    private void Start(){
        instance = this;
        player = gameObject;
        health = maxHealth;

        defFOV = Camera.main.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        
        InitBars();
        InventorySlot.InitInventorySlots(inventorySlots, thumbnailRectSize);
        InitDebuffsAndEffects();
        thumbnailRenderer = new InventoryThumbnailRenderer(inventoryCam, volumeProfile);
    }



    private void InitBars(){
        fullBarPos = ammoBar.GetComponent<RectTransform>().rect.x + ammoBar.GetComponent<RectTransform>().rect.width / 2;
        barWidth = ammoBar.GetComponent<RectTransform>().rect.width / 7.5f;
        fullHealthBarPos = healthBar.GetComponent<RectTransform>().rect.x + healthBar.GetComponent<RectTransform>().rect.width / 2;
        healthBarWidth = healthBar.GetComponent<RectTransform>().rect.width / 7.5f;
    }


    private void DisplayPopup(){
        if(lookingAt == null) return;
        IPopup p = lookingAt.GetComponent<IPopup>();
        if(p != null && p.GetPopup().description != "" && p.GetPopup().key != ' '){
            ActionPopup popup = lookingAt.GetComponent<IPopup>().GetPopup();
            actionDescription.text = "[" + popup.key + "]-" + popup.description;
            actionBackground.enabled = true;
        }
        else{
            actionDescription.text = "";
            actionBackground.enabled = false;
        }
    }


    private void Update(){
        //Debug.Log(holding?.name ?? "null");
        if(holding != null){
            if(holding is Weapon) HandleWeapon();
            if(holding is Throwable) holding.Use();
        }

        HandleLook();
        DisplayPopup();
        Move();
        HandleHealthBar();
        HandleDebuffs();
        HandleInventory();

        if(Input.GetKeyDown(KeyCode.T)) Drop();
        if(!electrified && Input.GetKeyDown(KeyCode.E)) Grab();
    }


    private void Grab(){
        if(lookingAt != null && Vector3.Distance(transform.position, lookingAt.transform.position) < Grabbable.grabDistance){
            Grabbable item = lookingAt.GetComponent<Grabbable>();
            if(item == null) return;
            (InventorySlot slot, int index) nextSlot = InventorySlot.GetNextEmptySlot(inventorySlots);
            if(inventorySlots[currentSlot].isEmpty()){
                nextSlot.slot = inventorySlots[currentSlot];
                nextSlot.index = currentSlot;
            }
            else if(nextSlot.slot == null){
                Drop();
                nextSlot.slot = inventorySlots[currentSlot];
                nextSlot.index = currentSlot;
            }
            nextSlot.slot.item = item.Grab();
            if(item.icon == null) item.icon = thumbnailRenderer.Render(thumbnailSize, item);
            nextSlot.slot.SetIcon(item.icon);
            ChangeSlot(nextSlot.index);
        }
    }


    public void Drop(){
        holding.gameObject.SetActive(true);
        holding.Drop();
        inventorySlots[currentSlot].Clear();
        holding = null;
    }


    private void HandleInventory(){
        if(Input.GetKeyDown(KeyCode.Alpha1)) ChangeSlot(0);
        else if(Input.GetKeyDown(KeyCode.Alpha2)) ChangeSlot(1);
        else if(Input.GetKeyDown(KeyCode.Alpha3)) ChangeSlot(2);
    }


    private void ChangeSlot(int slot){
        inventorySlots[currentSlot].Deselect();
        currentSlot = slot;
        holding?.gameObject.SetActive(false);
        holding = inventorySlots[slot].item;
        holding?.gameObject.SetActive(true);
        inventorySlots[slot].Select();
    }


    private void FixedUpdate(){
        ApplyMovementForce(inputDir, speed);
        Jump();
    }


    private void HandleLook(){
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, lookRange, ~LayerMask.GetMask("Player"))){
            lookingAt = hit.collider.gameObject;
        }
        else{
            lookingAt = null;
        }
    }


    private void HandleWeapon(){
        Weapon wep = (Weapon)holding;
        wep.Use();
        wep.Sway(8, 2);
        ammoText.text = wep.AmmoInfo();

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


    private void HandleHealthBar(){
        RectTransform rt = healthBar.GetComponent<RectTransform>();
        float ycomp = rt.localPosition.y;
        rt.localPosition = Vector3.Lerp(rt.localPosition, new Vector3(fullHealthBarPos - healthBarWidth + (healthBarWidth * health/maxHealth), ycomp, 0), 7f * Time.deltaTime);
        healthText.text = health.ToString("F0") + " / " + maxHealth;
    }


    private void ApplyMovementForce(Vector3 dir, float speed){
        rb.AddForce(dir * speed * 10f, ForceMode.Force);
        if(!grounded) rb.AddForce(Vector3.down * 20, ForceMode.Acceleration);
    }


    private void Jump(){
        if(grounded && jumping){
            rb.AddForce(Vector3.up * 20, ForceMode.Impulse);
        }
    }


    private void Move(){
        grounded = Physics.Raycast(transform.position, -transform.up, 1.1f);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if(!grounded) horizontalInput = 0;
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(KeyCode.LeftShift)){ 
            speed = defSpeed * 2;
            sprinting = true;
        }
        else{ 
            speed = defSpeed;
            sprinting = false;
        }

        transform.rotation = new Quaternion(0, Camera.main.transform.rotation.y, 0,  Camera.main.transform.rotation.w);


        jumping = Input.GetKey(KeyCode.Space);
        rb.drag = groundDrag;

        inputDir = transform.forward * verticalInput + transform.right * horizontalInput;
        bool moving = inputDir.magnitude > 0;

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


    private void HandleDebuffPackage(AttackType type, bool active, float dur){
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


    private void ResetVignette(){
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, vingnetteDefIntensity, 7f * Time.deltaTime);
        vignette.color.value = Color.Lerp(vignette.color.value, vignetteDefColor, 7f * Time.deltaTime);
    }


    private void AddDebuff(AttackType type, float duration){
        if(debuffs[type] < duration){
            debuffs[type] = duration;
        }
    }


    private void InitDebuffsAndEffects(){
        volumeProfile.TryGet(out vignette);
        vingnetteDefIntensity = vignette.intensity.value;
        vignetteDefColor = vignette.color.value;
        
        DebuffDefinitions.player = this;
        DebuffDefinitions.vignette = vignette;
        DebuffDefinitions.debuffIntensity = vignette.intensity.value * 0.9f;

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
