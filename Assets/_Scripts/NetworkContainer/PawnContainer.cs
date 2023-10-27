
using System;
using _Scripts.NetworkContainter;
using Unity.Collections;
using Unity.Netcode;

public struct PawnContainer: INetworkSerializable, IEquatable<PawnContainer>
{
    public int PawnID;
    public ulong ClientOwnerID;
    public int StandingMapCell;
    public int StandingMapSpot;
    
    public PawnStatContainer PawnStatContainer;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PawnID);
        serializer.SerializeValue(ref ClientOwnerID);
        serializer.SerializeValue(ref StandingMapCell);
        serializer.SerializeValue(ref StandingMapSpot);
        serializer.SerializeValue(ref PawnStatContainer);
    }

    public bool Equals(PawnContainer other)
    {
        return PawnID == other.PawnID && 
               ClientOwnerID == other.ClientOwnerID;
    }
        
        
}