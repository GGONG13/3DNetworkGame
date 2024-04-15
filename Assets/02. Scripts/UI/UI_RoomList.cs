using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class UI_RoomList : MonoBehaviourPunCallbacks
{
    public List<UI_Room> UIRooms;

    private void Start()
    {
        RoomSet();
    }

    // Room(방)의 정보가 변경(추가/수정/삭제) 되었을 때 호출되는 콜백 함수
    public void OnRoomListUpdated(List<RoomInfo> roomList)
    {
        RoomSet();

        List<RoomInfo> liveRoomList = roomList.FindAll(r => r.RemovedFromList == false);
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++) 
        {
            UIRooms[i].Set(roomList[i]);    
            UIRooms[i].gameObject.SetActive(true);
        }
    }

    public void RoomSet()
    {
        foreach (UI_Room roomUI in UIRooms)
        {
            roomUI.gameObject.SetActive(false);
        }
    }
}
