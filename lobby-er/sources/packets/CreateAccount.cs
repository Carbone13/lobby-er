using LiteNetLib.Utils;

namespace Bonebreaker.Net
{
    /// <summary>
    /// Register/Create a new account on the Lobby-er
    /// </summary>
    [Packet]
    public class CreateAccount : INetSerializable
    {
        public int OperationID;

        public Account Account;
        
        public void Serialize (NetDataWriter writer)
        {
            writer.Put((int)PacketsList.ACCOUNT_CREATE);
            
            writer.Put(OperationID);
            writer.Put(Account);
        }

        public void Deserialize (NetDataReader reader)
        {
            OperationID = reader.GetInt();
            Account = reader.Get<Account>();
        }
    }
}