using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_PlayerRanking : MonoBehaviourPunCallbacks
{
    public List<UI_PlayerRankingSlot> Slots; // 1 ~ 5등
    public UI_PlayerRankingSlot MySlot; // 이 슬롯이 내 정보

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Refresh();
    }
    // 플레이어의 커스텀 프로퍼티가 변경되면 호출되는 함수
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Refresh();
    }
    public override void OnJoinedRoom()
    {
        Refresh();
    }

    public void Refresh()
    {
        Dictionary<int, Player> players = PhotonNetwork.CurrentRoom.Players;
        List<Player> playerList = players.Values.ToList();
        int PlayerCount = Math.Min(playerList.Count, 5);

        foreach (UI_PlayerRankingSlot slot in Slots)
        {
            slot.gameObject.SetActive(false);
        }
        for (int i = 0; i < PlayerCount; i++)
        {
            Slots[i].gameObject.SetActive(true);
            Slots[i].Set(playerList[i]);
        }

        MySlot.Set(PhotonNetwork.LocalPlayer);
    }
}