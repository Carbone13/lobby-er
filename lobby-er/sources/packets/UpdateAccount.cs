using LiteNetLib.Utils;

namespace Bonebreaker.Net
{
    /// <summary>
    /// Update your account, you can change the username, the e-mail & the password
    /// </summary>
    [Packet]
    public class UpdateAccount : INetSerializable
    {
        public int OperationID;
        
        public Account Account;
        
        public void Serialize (NetDataWriter writer)
        {
            writer.Put((int)PacketsList.ACCOUNT_UPDATE);
            
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