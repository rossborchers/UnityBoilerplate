/// <summary>
/// Pair class. Stores two objects together.
/// </summary>
/// <typeparam name="T">First objects type.</typeparam>
/// <typeparam name="U">Second objects type.</typeparam>
public class Pair<T, U>
{
    public Pair(){}
    public Pair(T first, U second)
    {
        this.First = first;
        this.Second = second;
    }

    /// <summary>
    /// The first value, corresponds to the first type param 'T'
    /// </summary>
    public T First { get; set; }

    /// <summary>
    /// The second value, corresponds to the second type param 'U'
    /// </summary>
    public U Second { get; set; }
};


