using System;

[Serializable]
public struct StatValue
{
    public float HP;
    public float speed;

    public StatValue(float hp, float speed)
    {
        HP = hp;
        this.speed = speed;
    }
}
