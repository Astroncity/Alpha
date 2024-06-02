using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour{
    [HideInInspector] public List<GameObject> doors;
    public Transform doorStart;
    [HideInInspector] public List<Enemy> enemies;
    [Header("Temporary")] public GameObject testEnemyPrefab;
    public GameObject roomObj;


    public void Update(){
        checkUnlock();
    }


    public void Populate(){
        for(int i = 0; i < 5; i++){
            Collider col = GetComponent<Collider>();
            float x = Random.Range(col.bounds.min.x, col.bounds.max.x);
            float z = Random.Range(col.bounds.min.z, col.bounds.max.z);
            float y = col.bounds.max.y;
            Vector3 pos = new Vector3(x, y, z);

            GameObject enemy = Instantiate(testEnemyPrefab, pos, Quaternion.identity, transform);
            enemy.GetComponent<Enemy>().parentRoom = gameObject;
            enemies.Add(enemy.GetComponent<Enemy>());

        }
    }


    public void checkUnlock(){
        if(enemies.Count == 0){
            for(int i = 0; i < doors.Count; i++){
                Door door = doors[i].GetComponent<Door>();
                if(door.connected){
                    door.setLock(false);
                }
            }
        }
        else{
            for(int i = 0; i < doors.Count; i++){
                Door door = doors[i].GetComponent<Door>();
                if(door.connected){
                    door.setLock(true);
                }
            }
        }
    }


    public static void Init(GameObject room, GameObject door){
        for(int i = 0; i < 4; i++){
            Room r = room.GetComponent<Room>();
            Transform doorStart = r.doorStart;

            //check if door is already there from another room
            RaycastHit hit;
            Vector3 pos = doorStart.position - doorStart.up * 4;
            Ray ray = new Ray(pos, doorStart.up);
            Debug.DrawRay(pos, doorStart.up * 5, Color.red, 1000f);
            if(Physics.Raycast(ray, out hit, 5f)){
                if(hit.collider.attachedRigidbody.gameObject.tag == "door"){
                    //?Debug.Log("door already there");
                    doorStart.RotateAround(room.transform.position, Vector3.up, 90);
                    continue;
                }
                else{
                    Debug.Log(hit.collider.attachedRigidbody.gameObject.name);
                }
            }
            
            
            GameObject d = MonoBehaviour.Instantiate(door, doorStart.position, doorStart.rotation, room.transform);
            
            r.doors.Add(d);
            d.GetComponent<Door>().connected = false;
            d.GetComponent<Door>().setLock(true);
            d.GetComponent<Door>().room = room;
            
            //reset scale
            d.transform.localScale /= room.transform.lossyScale.x;
            doorStart.RotateAround(room.transform.position, Vector3.up, 90);


        }
    }
}
