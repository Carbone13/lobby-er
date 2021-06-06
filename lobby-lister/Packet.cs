﻿using System.Net;

namespace Network.Packet
{
    // Represent a joinable Lobby
    public class Lobby
    {
        public IPEndPoint HostPublicAddress { get; set; }
        public string LobbyName { get; set; }
        public string HostName { get; set; }
        public int PlayerCount { get; set; }
    }

    
    // Empty packet, notify that you want to get the lobbies list
    public class RequestLobbyList {}

    // Ask to join a specific lobby
    public class JoinLobby
    {
        public IPEndPoint HostPublicAddress { get; set; }
        public string LobbyName { get; set; }
        public string HostName { get; set; }
        public int PlayerCount { get; set; }
    }

    // Sent by Lobby-er to client, notify them that they need to connect toward the specified end point
    public class ConnectTowardOrder
    {
        public IPEndPoint target { get; set; }
    }

    // Send an int linking to an error
    public class NATError
    {
        public const int LOBBY_HOST_LOST = 1;
        
        public int error { get; set; }
    }
}