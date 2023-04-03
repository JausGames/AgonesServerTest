using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameServer
{
	public string name;		//`json:"name"`
	public string kind;		//`json:"kind"`
	public string nspace;	//`json:"namespace"`
	public string status;	//`json:"status"`
	public string ip;		//`json:"ip"`
	public int port;		//`json:"port"`
	public int player_count;//`json:"player_count"`
	public int max_player;	//`json:"player_max"`
	public string uid;		//`json:"uid"`

}
