using System.Collections.Generic;
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


    private void Start(){
        roomQueue = new Queue<GameObject>();
        Room.Init(origin, doorPrefab);
        Generate(origin, rooms, doorPrefab);
    }


    private void Update(){
        roomCount = rmList.Count + 1;
        RenderRooms();
    }


    public void RenderRooms(){
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
                    CreateRoom(doors, offset, i, doorPrefab);
                    added = true;
                }
            }
            if(!added){
                int door = GetAvailableDoor(doors);
                if(door == -1) continue;
                CreateRoom(doors, offset, door, doorPrefab);
            }
        }
        roomQueue.Clear();
    }


    private int GetAvailableDoor(List<GameObject> doors){
        for(int i = 0; i < doors.Count; i++){
            if(doors[i].activeSelf){
                return i;
            }
        }
        return -1;
    }


    private void CreateRoom(List<GameObject> doors, float offset, int i, GameObject doorPrefab){
        Vector3 pos = doors[i].transform.position;
        pos += doors[i].transform.up * offset;

        //select random room from list
        GameObject roomPrefab = roomList.roomPrefabs[Random.Range(0, roomList.roomPrefabs.Count)];
        
        GameObject room = Instantiate(roomPrefab, pos, roomPrefab.transform.rotation);
        room.transform.position = new Vector3(room.transform.position.x, 0, room.transform.position.z);

        //set door
        doors[i].GetComponent<Door>().connected = true;
        doors[i].GetComponent<Door>().windowBlocker.SetActive(false);

        //* remove door
        Room.Init(room, doorPrefab);

        //* add to lists
        roomQueue.Enqueue(room);
        rmList.Add(room);

        room.GetComponent<Room>().Populate();
    }
    
}
