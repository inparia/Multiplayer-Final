using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using NetworkMessages;
using NetworkObjects;
using System;
using System.Text;

public class NetworkClient : MonoBehaviour
{
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
                if(puMsg.player.id != GameManager.Instance.playerID.ToString() && string.IsNullOrEmpty(GameManager.Instance.firstPlayer))
                {
                    GameManager.Instance.firstPlayer = puMsg.player.id;
                }
                if(puMsg.player.id != GameManager.Instance.playerID.ToString() && puMsg.player.id != GameManager.Instance.firstPlayer &&!string.IsNullOrEmpty(GameManager.Instance.firstPlayer) && string.IsNullOrEmpty(GameManager.Instance.secondPlayer))
                {
                    GameManager.Instance.secondPlayer = puMsg.player.id;
                }
                Debug.Log("PlayerID : " + puMsg.player.id + " : " + puMsg.player.totalNum);
            break;
            case Commands.SERVER_UPDATE:
            ServerUpdateMsg suMsg = JsonUtility.FromJson<ServerUpdateMsg>(recMsg);
            Debug.Log("Server update message received!");
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
            m.player.totalNum = GameManager.Instance.total;
            m.player.id = GameManager.Instance.playerID.ToString();
            m.player.firstNum = GameManager.Instance.firstNum;
            m.player.secondNum = GameManager.Instance.secondNum;
            m.player.thirdNum = GameManager.Instance.thirdNum;
            SendToServer(JsonUtility.ToJson(m));
        }
    }
}