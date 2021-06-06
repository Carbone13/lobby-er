﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using Network.Packet;

namespace LobbyEr
{
    public class Server
    {
        public const string ADDRESS = "0.0.0.0";
        public const int PORT = 3456;

        private NetManager socket;
        private NetPacketProcessor processor;

        private Dictionary<NetPeer, Lobby> hostedLobbies = new();
        private List<NetPeer> connectedPeers = new();
        
        public Server ()
        {
            Console.WriteLine("--> Starting Lobby-er...");
            Console.WriteLine("Trying to host on " + ADDRESS + ":" + PORT);
            
            EventBasedNetListener listener = new();
            socket = new NetManager(listener);
            socket.Start(3456);
            
            SetupPacketProcessing();
            StartNetworkThread();
            
            Console.WriteLine(">> Up and Running !");
            
            listener.ConnectionRequestEvent += request =>
            {
                Console.WriteLine("> Received a connection request from " + request.RemoteEndPoint + " STATUS: ACCEPTED");
                request.Accept();
            };
            
            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine(">> Confirmed connection with " + peer.EndPoint);
                connectedPeers.Add(peer);
            };

            listener.PeerDisconnectedEvent += (peer, _) =>
            {
                Console.WriteLine(">> Disconnection from " + peer.EndPoint + " cleaning up...");
                Console.WriteLine(">>> Removed from peer list");
                connectedPeers.Remove(peer);

                if (hostedLobbies.ContainsKey(peer))
                {
                    hostedLobbies.Remove(peer);
                    Console.WriteLine(">>> Was hosting a lobby, cleaned it up.");
                }
            };
            
            listener.NetworkReceiveEvent += (fromPeer, dataReader, _) =>
            {
                Console.WriteLine(">> Received a packet from " + fromPeer.EndPoint + " processing...");
                processor.ReadAllPackets(dataReader, fromPeer);
            };
        }
        
        // When someone send us a packet containing lobby informations
        // It can be a register (= first packet) or and update of the lobby state 
        public void OnLobbyPacketReceived (Lobby lobby, NetPeer sender)
        {
            if (hostedLobbies.ContainsKey(sender))
            {
                // Lobby State update 
                Console.WriteLine(">>> Packet type : LOBBY_UPDATE");
                
                lobby.HostPublicAddress = sender.EndPoint;
                hostedLobbies[sender] = lobby;
            }
            else
            {
                // New lobby
                Console.WriteLine(">>> Packet type : LOBBY_REGISTER");

                lobby.HostPublicAddress = sender.EndPoint;
                hostedLobbies.Add(sender, lobby);
            }
        }

        // When someone ask for the list of available lobbies.
        public void OnLobbyListAsked (RequestLobbyList request, NetPeer asker)
        {
            Console.WriteLine(">>> Packet type : LOBBY_LIST_QUERY");
            Console.WriteLine(">>> Sending back informations about " + hostedLobbies.Values.Count + " lobbies.");
            // We send him every available lobby back, 1 per 1
            foreach (Lobby lobby in hostedLobbies.Values)
            {
                asker.Send(processor.Write(lobby), DeliveryMethod.ReliableOrdered);
            }
        }

        // Client notification that he want to connect toward a specific lobby
        // We will then send the 2 peers address to each other
        public void InitializeConnectionTowardLobby (JoinLobby targetLobby, NetPeer who)
        {
            Console.WriteLine(">>> Packet type : INITIALIZE_CONNECTION_TOWARD_LOBBY");
            
            NetPeer lobbyHostPeer = null;
            foreach (NetPeer peer in hostedLobbies.Keys.Where(peer => peer.EndPoint.Equals(targetLobby.HostPublicAddress)))
                lobbyHostPeer = peer;

            if (lobbyHostPeer == null)
            {
                Console.WriteLine(">>> Unknown lobby ! Sending back an error code");

                NATError error = new NATError() {error = 1};
                who.Send(processor.Write(error), DeliveryMethod.ReliableOrdered);
                return;
            }
            
            IPEndPoint hostEndpoint = targetLobby.HostPublicAddress;
            IPEndPoint clientEndpoint = who.EndPoint;

            ConnectTowardOrder hostOrder = new() {target = clientEndpoint};
            ConnectTowardOrder clientOrder = new() {target = hostEndpoint};

            who.Send(processor.Write(clientOrder), DeliveryMethod.ReliableOrdered);
            lobbyHostPeer.Send(processor.Write(hostOrder), DeliveryMethod.ReliableOrdered);
            
            Console.WriteLine(">>> Successfully sent end points to client & host.");
        }
        
        #region Initialization

        private void SetupPacketProcessing ()
        {
            processor = new NetPacketProcessor();
            
            processor.SubscribeReusable<Lobby, NetPeer> (OnLobbyPacketReceived);
            processor.SubscribeReusable<RequestLobbyList, NetPeer> (OnLobbyListAsked);
            processor.SubscribeReusable<JoinLobby, NetPeer> (InitializeConnectionTowardLobby);
        }

        private void StartNetworkThread ()
        {
            Thread _netThread = new(NetworkLoop);
            _netThread.Start();
        }
        
        public void NetworkLoop ()
        {
            while (!Console.KeyAvailable)
            {
                socket.PollEvents();
                Thread.Sleep(50);
            }

            socket.Stop();
        }
        #endregion
    }
}