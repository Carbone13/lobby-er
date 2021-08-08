using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Bonebreaker.Net;
using LiteNetLib;
using LiteNetLib.Utils;
using static System.Console;

namespace LobbyEr
{
    public class Server : INetEventListener
    {
        private const string KEY = "bb_a_v0.07";
        public const string ADDRESS = "0.0.0.0";
        public const int PORT = 3456;
        
        public NetManager client;

        public Server ()
        {
            WriteLine("> Booting up server...");
            client = new NetManager(this);
            client.Start(PORT);
    
            WriteLine(" > Up and running !");
            Thread pollingThread = new (Poll);
            pollingThread.Start();
        }

        public void Poll ()
        {
            while (true)
            {
                client.PollEvents();
                Thread.Sleep(50);
            }
        }
        
        public static void SendToPeer (INetSerializable packet, NetPeer peer)
        {
            NetDataWriter writer = new NetDataWriter();
            packet.Serialize(writer);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public void OnPeerConnected (NetPeer peer)
        {
            WriteLine("> New peer connected: " + peer.EndPoint);
        }

        public void OnPeerDisconnected (NetPeer peer, DisconnectInfo disconnectInfo)
        {
            WriteLine("> Peer disconnected: " + peer.EndPoint);
        }

        public void OnNetworkError (IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkReceive (NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            // get packet header
            PacketsList header = (PacketsList)reader.GetInt();
            WriteLine(header);
            switch (header)
            {
                case PacketsList.ACCOUNT_CREATE:
                    CreateAccount packet = new CreateAccount();
                    packet.Deserialize(reader);

                    EmptyOperationResult result = new EmptyOperationResult();
                    result.OperationID = packet.OperationID;
                    result.ErrorCode = 1;
                    if (SQL.CheckIfEmailIsAlreadyInUse(packet.Account.Email))
                    {
                        result.ErrorCode = 2;
                        SendToPeer(result, peer);
                        break;
                    }

                    if (SQL.CheckIfUsernameIsAlreadyInUse(packet.Account.Username))
                    {
                        result.ErrorCode = 3;
                        SendToPeer(result, peer);
                        break;
                    }
                    
                    SQL.RegisterAccount(packet.Account);
                    SendToPeer(result, peer);
                    break;
                case PacketsList.ACCOUNT_LOGIN:
                    LoginAccount log = new ();
                    log.Deserialize(reader);

                    Account account = SQL.QueryAccountByEmail(log.Credential);
                    
                    AccountLoginResult loginResult = new ();
                    loginResult.OperationID = log.OperationID;
                    loginResult.Account = new Account() { ID = -1 };
                    
                    if (account == null)
                    {
                        loginResult.ErrorCode = 5;
                        SendToPeer(loginResult, peer);
                        break;
                    }

                    if (account.Password == log.Password)
                    {
                        loginResult.ErrorCode = 1;
                        loginResult.Account = account;
                        SendToPeer(loginResult, peer);
                        break;
                    }
                    else
                    {
                        loginResult.ErrorCode = 6;
                        SendToPeer(loginResult, peer);
                        break;
                    }

                    break;
            }
            
            reader.Recycle();
        }

        public void OnNetworkReceiveUnconnected (IPEndPoint remoteEndPoint, NetPacketReader reader,
            UnconnectedMessageType messageType)
        {
            
        }

        public void OnNetworkLatencyUpdate (NetPeer peer, int latency)
        {
            
        }

        public void OnConnectionRequest (ConnectionRequest request)
        {
            request.AcceptIfKey(KEY);
        }
    }
}