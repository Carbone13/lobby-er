using LiteNetLib.Utils;

namespace Bonebreaker.Net
{
    /// <summary>
    /// Try to login onto an account, followed by a <see cref="AccountLoginResult"/>
    /// </summary>
    [Packet]
    public class LoginAccount : INetSerializable
    {
        // Operation ID; to keep this query tracked
        public int OperationID;

        public string Credential;
        public string Password;

        public void Serialize (NetDataWriter writer)
        {
            writer.Put((int)PacketsList.ACCOUNT_LOGIN);
            
            writer.Put(OperationID);
            writer.Put(Credential, 100);
            writer.Put(Password, 100);
        }

        public void Deserialize (NetDataReader reader)
        {
            OperationID = reader.GetInt();
            Credential = reader.GetString(100);
            Password = reader.GetString(100);
        }
    }
}