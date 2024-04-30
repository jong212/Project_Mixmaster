using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : Singleton<GameManager>
{
    public LinkData linkData;
    public GameObject player;
    public Tilemap tilemap;

    //public List<LinkedNode> nodes = new List<LinkedNode>();
    //public List<LinkedNode> nodes = new List<LinkedNode>();
    public LinkedNode currentNode = null;

    //선세팅 필요한 거 있으면 여기서
    private static void Init()
    {

    }
    public void SetPlayer(GameObject player)
    {
        this.player = player;
    }
    public void SetLinkData(LinkData data)
    {
        linkData = data;
    }
    public LinkedNode CheakOnNode(Vector3Int firstRoomCenter)
    {
        //Debug.Log(firstRoomCenter);
        return linkData.FindOnRect(firstRoomCenter);
        //linkedNode.linkedNodeDic
    }
}


