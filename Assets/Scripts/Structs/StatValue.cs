using System;
using Unity.Collections;

[Serializable]
public struct StatValue
{
    public FixedString64Bytes name;
    public float HP;
    public float speed;

    public StatValue(float hp, float speed, string name)
    {
        this.name = Utils.FixString64(name);
        HP = hp;
        this.speed = speed;
    }
}
