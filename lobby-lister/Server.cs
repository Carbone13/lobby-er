using System;
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
        public NetPacketProcessor processor;

        private Dictionary<NetPeer, Lobby> hostedLobbies = new();
        private List<NetPeer> connectedPeers = new();

        public NetworkPeer Us;
        
        public Server ()
        {
            Us = new NetworkPeer("Lobby-Er",
                new EndpointCouple(new IPEndPoint(IPAddress.Parse("90.76.187.136"), 3456), new IPEndPoint(IPAddress.Parse("90.76.187.136"), 3456)),
                true
                );

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
                PublicAddress address = new PublicAddress(Us, peer.EndPoint);
                address.Send(peer, DeliveryMethod.ReliableOrdered);
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
        public void OnLobbyPacketReceived (RegisterAndUpdateLobbyState lobby, NetPeer sender)
        {
            if (hostedLobbies.ContainsKey(sender))
            {
                // Lobby State update 
                Console.WriteLine(">>> Packet type : LOBBY_UPDATE");
                
                //lobby.HostPublicAddress = sender.EndPoint;
                hostedLobbies[sender] = lobby.Lobby;
            }
            else
            {
                // New lobby
                Console.WriteLine(">>> Packet type : LOBBY_REGISTER");

                //lobby.HostPublicAddress = sender.EndPoint;
                hostedLobbies.Add(sender, lobby.Lobby);
            }
        }

        // When someone ask for the list of available lobbies.
        public void OnLobbyListAsked (QueryLobbyList request, NetPeer asker)
        {
            Console.WriteLine(">>> Packet type : LOBBY_LIST_QUERY");
            Console.WriteLine(">>> Sending back informations about " + hostedLobbies.Values.Count + " lobbies.");
            // We send him every available lobby back

            LobbyListAnswer answer = new LobbyListAnswer(Us, hostedLobbies.Values.ToArray());

            answer.Send(asker, DeliveryMethod.ReliableOrdered);
        }

        public void OnLobbyJoinAsked (AskToJoinLobby ask, NetPeer sender)
        {
            Console.WriteLine(">>> Packet type : JOIN_LOBBY_ASK");

            NetPeer lobbyHostPeer = null;
            foreach(NetPeer host in hostedLobbies.Keys)
            {
                Console.WriteLine(host.EndPoint + " " + ask.Target.Host.Endpoints.Public);
                if(host.EndPoint.ToString() == ask.Target.Host.Endpoints.Public.ToString())
                {
                    lobbyHostPeer = host;
                }
            }

            if (lobbyHostPeer == null)
            {
                Console.WriteLine(">>> Unknown lobby ! Sending back an error code");
                // TODO
                return;
            }

            IPEndPoint hostEndpoint = IPEndPoint.Parse(lobbyHostPeer.EndPoint.ToString());
            IPEndPoint clientEndpoint = IPEndPoint.Parse(sender.EndPoint.ToString());

            bool usePrivate = hostEndpoint.Address.ToString() == clientEndpoint.Address.ToString();

            HolePunchAddress hostHPAddress = new HolePunchAddress(Us, ask.Target.Host, usePrivate);
            HolePunchAddress clientHPAddress = new HolePunchAddress(Us, ask.Sender, usePrivate);

            hostHPAddress.Send(sender, DeliveryMethod.ReliableOrdered);
            clientHPAddress.Send(lobbyHostPeer, DeliveryMethod.ReliableOrdered);

            Console.WriteLine(">>> Successfully sent end points to client & host.");
        }
        
        #region Initialization

        private void SetupPacketProcessing ()
        {
            processor = new NetPacketProcessor();

            processor.RegisterNestedType<NetworkPeer>();
            processor.RegisterNestedType<EndpointCouple>();
            processor.RegisterNestedType<Lobby>();

            processor.SubscribeReusable<RegisterAndUpdateLobbyState, NetPeer> (OnLobbyPacketReceived);
            processor.SubscribeReusable<QueryLobbyList, NetPeer> (OnLobbyListAsked);
            processor.SubscribeReusable<AskToJoinLobby, NetPeer>(OnLobbyJoinAsked);
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