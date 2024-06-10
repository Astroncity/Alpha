using UnityEngine;

public class SlimeProjectile : MonoBehaviour{
    public float damage;
    public AttackType type;


    public void Init(float damage, AttackType type, Vector3 velocity, bool super){
        this.damage = damage;
        this.type = type;
        GetComponent<Rigidbody>().velocity = velocity;
        transform.LookAt(PlayerController.player.transform.position);
        if(!super) transform.localScale /= 2;
    }


    private void OnCollisionEnter(Collision col){
        Grabbable holding = PlayerController.instance.holding;
        if(col.gameObject.tag == "Player" || holding != null && holding == col.gameObject.GetComponent<Grabbable>()){
            PlayerController.instance.Damage(damage, type);
            Destroy(gameObject);
        }
        else{
            if(col.gameObject.GetComponent<SlimeProjectile>() == null){
                Destroy(gameObject);
            }
        }
    }

}
