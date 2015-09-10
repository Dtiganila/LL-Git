using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {
	private const string typeName = "dtiggametest";
	private const string gameName = "dtigtest";
	private HostData[] hostList;
	public GameObject redPrefab;
	public GameObject greenPrefab;
	public GameObject bluePrefab;
	public NetworkView networkView;
	private int playNum;
	private bool spawned = false;

	[RPC]
	void PrintText (string text)
	{
		Debug.Log(text);
	}

	[RPC]
	void PlayerNum(int num)
	{
		Debug.Log ("playerNum is: " + num);
		playNum = num;
		if (Network.isClient) {
			if(!spawned){
				SpawnPlayer (playNum);
			}
		}
	}

	public void StartServer()
	{
		Network.InitializeServer(3, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}
	void OnServerInitialized()
	{
		Debug.Log("Server Initializied");
		SpawnPlayer(0);
	}
	void OnGUI()
	{
		if (!Network.isClient && !Network.isServer)
		{
			if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
				StartServer();
			
			if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
				RefreshHostList();
			
			if (hostList != null)
			{
				for (int i = 0; i < hostList.Length; i++)
				{
					if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
						JoinServer(hostList[i]);
				}
			}
		}
	}
	private void RefreshHostList()
	{
		MasterServer.RequestHostList(typeName);
	}
	
	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList();
	}
	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}

	void OnPlayerConnected(){
		playNum++;
		//networkView.RPC ("PlayerNum", RPCMode.All, hostList[0].connectedPlayers);
		//networkView.RPC ("PrintText", RPCMode.All, "On Player Connected: ");
		networkView.RPC ("PlayerNum", RPCMode.All, playNum);
		Debug.Log ("playNum is: " + playNum);

	}

	void OnConnectedToServer()
	{
		Debug.Log("Server Joined");
		//networkView.RPC ("PlayerNum", RPCMode.All, hostList[0].connectedPlayers);
		Debug.Log ("playNum is: " + playNum);
	}
	
	private void SpawnPlayer(int playernum)
	{
		if (playernum == 0)
			Network.Instantiate (redPrefab, new Vector3 (-9f, 1f, -9f), Quaternion.identity, 0);
		else if (playernum == 1) {
			Network.Instantiate (greenPrefab, new Vector3 (-7.5f, 1f, -9f), Quaternion.identity, 0);
		} else if (playernum == 2) {
			Network.Instantiate (bluePrefab, new Vector3 (-6f, 1f, -9f), Quaternion.identity, 0);
		}
		spawned = true;
	}
}