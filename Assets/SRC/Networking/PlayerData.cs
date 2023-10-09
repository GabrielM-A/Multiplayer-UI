using System;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public int index;
    public Color characterId;
    public bool playerReady;

    public  bool Equals(PlayerData other)
    {
        return clientId == other.clientId && index == other.index && characterId == other.characterId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref index);
        serializer.SerializeValue(ref characterId);
        serializer.SerializeValue(ref playerReady);
    }
}
