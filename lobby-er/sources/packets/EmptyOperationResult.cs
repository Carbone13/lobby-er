using LiteNetLib.Utils;

namespace Bonebreaker.Net
{
    [Packet]
    public class EmptyOperationResult : INetSerializable
    {
        // OperationID linking to the initial LoginAccount query
        public int OperationID;
        /// <summary>
        /// Error code of the operation, see <see cref="Bonebreaker.Net.ErrorCode"/>
        /// </summary>
        public int ErrorCode;
        
        public void Serialize (NetDataWriter writer)
        {
            writer.Put((int)PacketsList.L_OPERATION_RESULT);
            
            writer.Put(OperationID);
            writer.Put(ErrorCode);
        }

        public void Deserialize (NetDataReader reader)
        {
            OperationID = reader.GetInt();
            ErrorCode = reader.GetInt();
        }
    }
}