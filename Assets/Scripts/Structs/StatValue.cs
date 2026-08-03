using System;
using Unity.Collections;
using Unity.Serialization;

[Serializable]
public struct StatValue
{
    [DontSerializeAttribute] public FixedString64Bytes name;
    public float HP;
    public float speed;

    public StatValue(float hp, float speed, string name)
    {
        this.name = Utils.FixString64(name);
        HP = hp;
        this.speed = speed;
    }
}
