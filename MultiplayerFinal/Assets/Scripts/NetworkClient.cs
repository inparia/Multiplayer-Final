using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using NetworkMessages;
using NetworkObjects;
using System;
using System.Text;


public class NetworkClient : MonoBehaviour
{
    // Ivan's edits
    /// <summary>
    /// this variable needed to confirm that all numbers are submitted 
    public bool readyCheck;
    public int numOfReadyPlayers = 0;
    /// </summary>



    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIP;
    public ushort serverPort;
    private bool sendInfo;
    void Start ()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.Parse(serverIP,serverPort);
        m_Connection = m_Driver.Connect(endpoint);
        sendInfo = false;

        //
        readyCheck = false;
        //

        //t = GameObject.Find("Cube");
    }
    
    void SendToServer(string message){
        var writer = m_Driver.BeginSend(m_Connection);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message),Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    void OnConnect(){
        Debug.Log("We are now connected to the server");
        sendInfo = true;

        //// Example to send a handshake message:
        //HandshakeMsg m = new HandshakeMsg();

    }

    void OnData(DataStreamReader stream){
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length,Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = Encoding.ASCII.GetString(bytes.ToArray());
        NetworkHeader header = JsonUtility.FromJson<NetworkHeader>(recMsg);
        
        switch (header.cmd){
            case Commands.HANDSHAKE:
            HandshakeMsg hsMsg = JsonUtility.FromJson<HandshakeMsg>(recMsg);
            Debug.Log("Handshake message received!");
                break;
            case Commands.PLAYER_UPDATE:
            PlayerUpdateMsg puMsg = JsonUtility.FromJson<PlayerUpdateMsg>(recMsg);
                if (GameManager.Instance.gameRoomID == puMsg.player.roomID && GameManager.Instance.gameRoomID != 0 && puMsg.player.roomID != 0)
                {
                    if (puMsg.player.id != GameManager.Instance.playerID.ToString() && string.IsNullOrEmpty(GameManager.Instance.firstPlayer))
                    {
                        
                        GameManager.Instance.firstPlayer = puMsg.player.id;
<<<<<<< HEAD
                        
                        GameManager.Instance.totalPlayers.Add(new Player(puMsg.player.id, puMsg.player.firstNum, puMsg.player.secondNum, puMsg.player.thirdNum, puMsg.player.totalNum, puMsg.player.roomID, puMsg.player.ready));
                        Debug.Log("asdqwe");
                        if (puMsg.player.ready == true)
                        {
                            numOfReadyPlayers++;
                        }
                        
=======
                        GameManager.Instance.totalPlayers.Add(new Player(puMsg.player.id, puMsg.player.firstNum, puMsg.player.secondNum, puMsg.player.thirdNum, puMsg.player.totalNum));

>>>>>>> parent of f5f9564... Rollback
                    }
                    if (puMsg.player.id != GameManager.Instance.playerID.ToString() && puMsg.player.id != GameManager.Instance.firstPlayer && !string.IsNullOrEmpty(GameManager.Instance.firstPlayer) && string.IsNullOrEmpty(GameManager.Instance.secondPlayer))
                    {
                        GameManager.Instance.secondPlayer = puMsg.player.id;
<<<<<<< HEAD
                      
                        GameManager.Instance.totalPlayers.Add(new Player(puMsg.player.id, puMsg.player.firstNum, puMsg.player.secondNum, puMsg.player.thirdNum, puMsg.player.totalNum, puMsg.player.roomID, puMsg.player.ready));
                        Debug.Log("asdqwe");
                        if (puMsg.player.ready == true)
                        {
                            numOfReadyPlayers++;
                        }
=======
                        GameManager.Instance.totalPlayers.Add(new Player(puMsg.player.id, puMsg.player.firstNum, puMsg.player.secondNum, puMsg.player.thirdNum, puMsg.player.totalNum));
>>>>>>> parent of f5f9564... Rollback
                    }

                    if (GameManager.Instance.totalPlayers.Count > 0 )
                    {
                        UpdatePlayerInfos(GameManager.Instance.totalPlayers[0], puMsg);
                    }

                    if (GameManager.Instance.totalPlayers.Count > 1 )
                    {
                        UpdatePlayerInfos(GameManager.Instance.totalPlayers[1], puMsg);
                    }

                    if (GameManager.Instance.totalPlayers.Count > 2 )
                    {
                        UpdatePlayerInfos(GameManager.Instance.totalPlayers[2], puMsg);
                    }
                }
                GameManager.Instance.roomNumbers = puMsg.player.matchedRooms;
                //Debug.Log("PlayerID : " + puMsg.player.id + " : " + puMsg.player.totalNum);
                break;
            case Commands.SERVER_UPDATE:
            ServerUpdateMsg suMsg = JsonUtility.FromJson<ServerUpdateMsg>(recMsg);
            Debug.Log("Server update message received!");
            break;

            case Commands.PLAYERS_READY:
            PlayersReadyMsg prMsg = JsonUtility.FromJson<PlayersReadyMsg>(recMsg);
            readyCheck = true;
                Debug.Log("I am ready");
            break;
            default:
            Debug.Log("Unrecognized message received!");
            break;
        }
    }

    void Disconnect(){
        m_Connection.Disconnect(m_Driver);
        m_Connection = default(NetworkConnection);
    }

    void OnDisconnect(){
        Debug.Log("Client got disconnected from server");
        m_Connection = default(NetworkConnection);
        //Destroy(t);
        sendInfo = false;
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }
    void Update()
    {

        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        cmd = m_Connection.PopEvent(m_Driver, out stream);
        while (cmd != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                OnConnect();
                //x = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                OnData(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                OnDisconnect();
            }

            cmd = m_Connection.PopEvent(m_Driver, out stream);

        }
 

        if(sendInfo)
        {
            PlayerUpdateMsg m = new PlayerUpdateMsg();
            
            m.player.id = GameManager.Instance.playerID.ToString();
            if(numOfReadyPlayers == 2)
            {
                m.player.totalNum = GameManager.Instance.total;
                m.player.firstNum = GameManager.Instance.firstNum;
                m.player.secondNum = GameManager.Instance.secondNum;
                m.player.thirdNum = GameManager.Instance.thirdNum;
            }



            m.player.roomID = GameManager.Instance.gameRoomID;
            m.player.ready = GameManager.Instance.ready;
            SendToServer(JsonUtility.ToJson(m));
        }
        
    }

    void UpdatePlayerInfos(Player tempP, PlayerUpdateMsg puMsg)
    {
        if (puMsg.player.id == tempP.playerID)
        {
            tempP.totalNum = puMsg.player.totalNum;
        }
    }

}