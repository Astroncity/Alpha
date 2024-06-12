using System.Collections.Generic;
using UnityEngine;
using RandomListSelection;

public class Room : MonoBehaviour{
    public Transform doorStart;
    public GameObject roomObj;
    [HideInInspector] public List<GameObject> doors;
    [HideInInspector] public List<Enemy> enemies;


    private void Update(){
        CheckUnlock();
    }


    public void Populate(){
        for(int i = 0; i < 5; i++){
            Collider col = GetComponent<Collider>();
            float x = Random.Range(col.bounds.min.x, col.bounds.max.x);
            float z = Random.Range(col.bounds.min.z, col.bounds.max.z);
            float y = col.bounds.max.y;
            Vector3 pos = new(x, y, z);

            Enemy enemy = Instantiate(PrefabManager.inst.enemies.Random(), pos, Quaternion.identity, transform).GetComponent<Enemy>();
            enemy.parentRoom = this;
            enemies.Add(enemy);

        }
    }


    public void CheckUnlock(){
        if(enemies.Count == 0){
            for(int i = 0; i < doors.Count; i++){
                Door door = doors[i].GetComponent<Door>();
                if(door.connected){
                    door.SetLock(false);
                }
            }
        }
        else{
            for(int i = 0; i < doors.Count; i++){
                Door door = doors[i].GetComponent<Door>();
                if(door.connected){
                    door.SetLock(true);
                }
            }
        }
    }


    public void AddEnemy(Enemy enemy){
        enemies.Add(enemy);
        enemy.parentRoom = this;
    }


    public static void Init(GameObject room, GameObject door){
        for(int i = 0; i < 4; i++){
            Room r = room.GetComponent<Room>();
            Transform doorStart = r.doorStart;

            //check if door is already there from another room
            RaycastHit hit;
            Vector3 pos = doorStart.position - doorStart.up * 4;
            Ray ray = new(pos, doorStart.up);
            Debug.DrawRay(pos, doorStart.up * 5, Color.red, 1000f);

            if(Physics.Raycast(ray, out hit, 5f)){
                if(hit.collider.attachedRigidbody.gameObject.tag == "door"){
                    hit.collider.gameObject.GetComponent<Door>().connected = true;
                    hit.collider.gameObject.GetComponent<Door>().windowBlocker.SetActive(false);
                    //move door to average position between new door and old door
                    hit.collider.transform.position = (hit.collider.transform.position + doorStart.position) / 2;
                    doorStart.RotateAround(room.transform.position, Vector3.up, 90);
                    continue;
                }
                else{
                    Debug.Log(hit.collider.attachedRigidbody.gameObject.name);
                }
            }
            Door d = MonoBehaviour.Instantiate(door, doorStart.position, doorStart.rotation, room.transform).GetComponent<Door>();
            
            r.doors.Add(d.gameObject);
            d.connected = false;
            d.windowBlocker.SetActive(true);
            d.SetLock(true);
            d.room = room;
            
            d.transform.localScale /= room.transform.lossyScale.x;
            doorStart.RotateAround(room.transform.position, Vector3.up, 90);
        }
    }
}
