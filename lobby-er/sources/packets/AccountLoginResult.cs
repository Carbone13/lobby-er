using System.Dynamic;
using LiteNetLib.Utils;

namespace Bonebreaker.Net
{
    /// <summary>
    /// Response sent by the Lobby-er from a <see cref="LoginAccount"/> packet
    /// Say if the connection if successful, and link an <see cref="Account"/> object.
    /// </summary>
    [Packet]
    public class AccountLoginResult : INetSerializable
    {
        // OperationID linking to the initial LoginAccount query
        public int OperationID;
        /// <summary>
        /// Error code of the operation, see <see cref="Bonebreaker.Net.ErrorCode"/>
        /// </summary>
        public int ErrorCode;
        
        public Account Account;

        public void Serialize (NetDataWriter writer)
        {
            writer.Put((int)PacketsList.L_ACCOUNT_LOGIN_RESULT);
            
            writer.Put(OperationID);
            writer.Put(ErrorCode);
            writer.Put(Account);
        }

        public void Deserialize (NetDataReader reader)
        {
            OperationID = reader.GetInt();
            ErrorCode = reader.GetInt();
            Account = reader.Get<Account>();
        }
    }
}