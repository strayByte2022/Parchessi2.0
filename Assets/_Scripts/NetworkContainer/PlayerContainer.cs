using System;
using Unity.Collections;
using Unity.Netcode;

namespace _Scripts.NetworkContainter
{
    public struct PlayerContainer : INetworkSerializable, IEquatable<PlayerContainer>
    {
        public FixedString64Bytes PlayerID;
        public ulong ClientID;
        public int ChampionID;
        public FixedString64Bytes PlayerName;
        
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerID);
            serializer.SerializeValue(ref ClientID);
            serializer.SerializeValue(ref ChampionID);
            serializer.SerializeValue(ref PlayerName);
            
        }

        public bool Equals(PlayerContainer other)
        {
            return PlayerID == other.PlayerID;
        }
        
        
    }
}