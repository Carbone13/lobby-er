using System;

namespace Bonebreaker.Net
{
    public enum PacketsList
    {
        ACCOUNT_CREATE = 0,
        ACCOUNT_LOGIN = 1,
        ACCOUNT_UPDATE = 2,
        L_ACCOUNT_LOGIN_RESULT = 3,
        L_OPERATION_RESULT = 4
    }
    
    public class Packet : Attribute {}
}