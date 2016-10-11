using UnityEngine;

public static class VectorEdit
{
    public enum Component { x, y, z }

    //X
    /// <summary>
    /// Use: myVector = myVector.SetX(0);
    /// </summary>
    public static Vector2 SetX(this Vector2 vector, float value)
    {
        return SetComponent(vector, value, Component.x);
    }

    /// <summary>
    /// Use: myVector = myVector.SetX(0);
    /// </summary>
    public static Vector3 SetX(this Vector3 vector, float value)
    {
        return SetComponent(vector, value, Component.x);
    }

    //Y
    /// <summary>
    /// Use: myVector = myVector.SetY(0);
    /// </summary>
    public static Vector2 SetY(this Vector2 vector, float value)
    {
        return SetComponent(vector, value, Component.y);
    }

    /// <summary>
    /// Use: myVector = myVector.SetY(0);
    /// </summary>
    public static Vector3 SetY(this Vector3 vector, float value)
    {
        return SetComponent(vector, value, Component.y);
    }

    //Z
    /// <summary>
    /// Use: myVector = myVector.SetZ(0);
    /// </summary>
    public static Vector3 SetZ(this Vector3 vector, float value)
    {
        return SetComponent(vector, value, Component.z);
    }

    static Vector3 SetComponent(Vector3 vector, float value, Component component)
    {
        var newVector = new Vector3(
            component == Component.x ? value : vector.x,
            component == Component.y ? value : vector.y,
            component == Component.z ? value : vector.z
            );

        return newVector;
    }
}