using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.Player;

namespace TestNetwork
{
    public enum MessageType
    {
        InfoMessage,
        GameDataMessage
    }

    public class NetworkLLAPI : MonoBehaviour, IDisposable
    {
        public static NetworkLLAPI Instance { get; private set; }

        
        public event Action ConnectedEvent;
        public event Action DoneConnected;
        public event Action Disconnected;
        public event Action<BinaryReader> OnGameDataReceived;
        //chanel
        int myReliableChannelId = 0;
        int playerPositionChannel = 0;
        int infoChannel;

        private int socketId;
        private int socketPort = 8888;
        private int connectionId;
        private int hostId;

        private MemoryStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;

        private byte[] data;

         

        void Start()
        {
            Instance = this;

            NetworkTransport.Init();
            ConnectionConfig config = new ConnectionConfig();
            config.PacketSize = 1000;

            myReliableChannelId = config.AddChannel(QosType.Reliable);
            playerPositionChannel = config.AddChannel(QosType.Reliable);
            infoChannel = config.AddChannel(QosType.Reliable);

            int maxConnections = 10;

            HostTopology topology = new HostTopology(config, maxConnections);
            socketId = NetworkTransport.AddHost(topology, socketPort);
            Debug.Log("Socket Open. SocketId is: " + socketId);

            data = new byte[config.PacketSize];

            stream = new MemoryStream(data);
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
        }
        
        void Update()
        {
            int recHostId;
            int recConnectionId;
            int recChannelId;
            
            int dataSize;
            byte error;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostId,
                out recConnectionId, out recChannelId, data, data.Length, out dataSize, out error);

            stream.Position = 0;
            
            switch (recNetworkEvent)
            {
                case NetworkEventType.Nothing:
                    break;

                case NetworkEventType.ConnectEvent:
                    connectionId = recConnectionId;
                    ConnectPlayer(recConnectionId);
                    Debug.Log("incoming connection event received");
                    break;

                case NetworkEventType.DataEvent:
                    Receive(recChannelId);
                    //Debug.Log("incoming message event received:");
                    break;

                case NetworkEventType.DisconnectEvent:
                    if (Disconnected != null) Disconnected();
                    Debug.Log("remote client event disconnected");
                    break;
            }

            
        }

        private void ConnectPlayer(int recConnectionId)
        {
            if (ConnectedEvent != null) ConnectedEvent();
        }

        private void Receive(int recChannelId)
        {
            if (playerPositionChannel == recChannelId)
                if (OnGameDataReceived != null) OnGameDataReceived(reader);
        }
        
        public void SendPositionPlayer(GameData sendData)
        {
            stream.Position = 0;

            writer.Write(sendData.position.x);
            writer.Write(sendData.position.y);
            writer.Write(sendData.position.z);

            writer.Write(sendData.rotation.x);
            writer.Write(sendData.rotation.y);
            writer.Write(sendData.rotation.z);

            writer.Write(sendData.isMove);
            writer.Write(sendData.isFire);

            writer.Write(sendData.fireBallCount);

            foreach (var firePos in sendData.fireBallPosition)
            {
                writer.Write(firePos.x);
                writer.Write(firePos.y);
                writer.Write(firePos.z);
            }

            writer.Write(sendData.isDestroyFireball);

            if (sendData.isDestroyFireball)
            {
                writer.Write(sendData.indexDestroyFireball);
            }
            
            writer.Write(sendData.isHit);
            writer.Write(sendData.isDeath);


            SendSocketMessage(MessageType.GameDataMessage);
        }

        public void RunServer()
        {
            if (DoneConnected != null) DoneConnected();
        }

        public void Connect(Text text)
        {
            byte error;
            connectionId = NetworkTransport.Connect(socketId, text.text, socketPort, 0, out error);

            if (error > 0)
                return;
            if (DoneConnected != null) DoneConnected();

            Debug.Log("Connected to server. ConnectionId: " + connectionId);
        }

        public void SendSocketMessage(MessageType messageType)
        {
            byte error;
            int channel;
            switch (messageType)
            {
                case MessageType.GameDataMessage:
                    channel = playerPositionChannel;
                    break;
                case MessageType.InfoMessage:
                    channel = infoChannel;
                    break;
                default:
                    channel = myReliableChannelId;
                    break;
            }
            bool isSend = NetworkTransport.Send(socketId, connectionId, channel, data, (int)stream.Position, out error);

            //if (!isSend)
            //{
            //    Debug.Log("NoSend");
            //}
        }
        

        public void Dispose()
        {
            if (NetworkTransport.IsStarted)
            {
                byte error;
                NetworkTransport.Disconnect(hostId, connectionId, out error);
                NetworkTransport.Shutdown();

                stream.Dispose();
            }
        }
    }
}
