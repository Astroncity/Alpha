using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class GenerateDungeon : MonoBehaviour{
    public GameObject origin;
    public GameObject doorPrefab;
    public RoomList roomList;
    [HideInInspector] public List<GameObject> rmList;
    public int rooms;
    public float roomRenderDistance = 20f;
    Queue<GameObject> roomQueue = new();
    public int roomCount;


    void Start(){
        roomQueue = new Queue<GameObject>();
        Room.Init(origin, doorPrefab);
        Generate(origin, rooms, doorPrefab);
    }

    void Update(){
        roomCount = rmList.Count + 1;
        if(Input.GetKeyDown(KeyCode.F5)){
            foreach(GameObject rm in rmList){
                Destroy(rm);
            }
        }
        if(Input.GetKeyDown(KeyCode.F6)){
            Generate(origin, rooms, doorPrefab);
        }
        renderOnlyAdjacentRooms();
    }


    public void renderOnlyAdjacentRooms(){
        //enable / disable rooms based on distance from player
        foreach(GameObject rm in rmList){
            if(Vector3.Distance(rm.transform.position, PlayerController.player.transform.position) > roomRenderDistance){
                rm.SetActive(false);
            }
            else{
                rm.SetActive(true);
            }
        }
    }

    ///<summary>
    ///<para>Generates n rooms by going through each side and randomly deciding to add a room.</para>
    ///<para>Duplicate doors are removed.</para>
    ///</summary>
    public void Generate(GameObject origin, int n, GameObject doorPrefab){
        float offset = -roomList.roomPrefabs[0].GetComponent<Room>().roomObj.transform.GetChild(1).GetComponent<MeshRenderer>().bounds.size.z / 2;
        
        roomQueue.Enqueue(origin);

        while(rmList.Count < (n - 1)){
            bool added = false;
            List<GameObject> doors = roomQueue.Dequeue().GetComponent<Room>().doors;
            
            for(int i = 0; i < doors.Count; i++){   
                float p = Random.Range(0f, 1f);

                if(p <= 0.33f){
                    createRoom(doors, offset, i, doorPrefab);
                    added = true;
                }
            }
            if(!added){
                int door = getAvailableDoor(doors);
                if(door == -1) continue;
                createRoom(doors, offset, door, doorPrefab);
            }
        }

        roomQueue.Clear();
    }


    private int getAvailableDoor(List<GameObject> doors){
        for(int i = 0; i < doors.Count; i++){
            if(doors[i].activeSelf){
                return i;
            }
        }
        return -1;
    }


    private void createRoom(List<GameObject> doors, float offset, int i, GameObject doorPrefab){
        Vector3 pos = doors[i].transform.position;
        pos += doors[i].transform.up * offset;

        //select random room from list
        GameObject roomPrefab = roomList.roomPrefabs[Random.Range(0, roomList.roomPrefabs.Count)];
        
        GameObject room = Instantiate(roomPrefab, pos, roomPrefab.transform.rotation);
        room.transform.position = new Vector3(room.transform.position.x, 0, room.transform.position.z);

        //set door
        doors[i].GetComponent<Door>().connected = true;

        //* remove door
        Room.Init(room, doorPrefab);

        //* add to lists
        roomQueue.Enqueue(room);
        rmList.Add(room);

        room.GetComponent<Room>().Populate();
    }
    
}
