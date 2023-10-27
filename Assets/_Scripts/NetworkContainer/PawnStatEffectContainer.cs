using System;
using Unity.Collections;
using Unity.Netcode;

namespace _Scripts.NetworkContainter
{
    public enum PawnStatEffectType : byte
    {
        Attack,
        Health,
        Speed,
    }
    
    public struct PawnStatEffectContainer : INetworkSerializable, IEquatable<PawnStatEffectContainer>
    {
        
        public ulong TriggerOwnerClientID;
        public ulong EffectedOwnerClientID;
        public int EffectedPawnContainerIndex;
        
        public PawnStatEffectType EffectType;
        public int EffectValue;
        public int EffectDuration;
        
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TriggerOwnerClientID);
            serializer.SerializeValue(ref EffectedOwnerClientID);
            serializer.SerializeValue(ref EffectedPawnContainerIndex);
            
            serializer.SerializeValue(ref EffectType);
            serializer.SerializeValue(ref EffectValue);
            serializer.SerializeValue(ref EffectDuration);
            
        }

        public bool Equals(PawnStatEffectContainer other)
        {
            return TriggerOwnerClientID == other.TriggerOwnerClientID 
                   && EffectedOwnerClientID == other.EffectedOwnerClientID 
                   && EffectedPawnContainerIndex == other.EffectedPawnContainerIndex 
                   && EffectType == other.EffectType
                   && EffectValue == other.EffectValue 
                   && EffectDuration == other.EffectDuration;
        }
        
        
    }
}