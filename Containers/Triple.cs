/// <summary>
/// Triple class. Stores three objects together.
/// </summary>
/// <typeparam name="T">First objects type</typeparam>
/// <typeparam name="U">Second objects type</typeparam>
/// <typeparam name="V">Third objects type</typeparam>
public class Triple<T, U, V>
{
    public Triple()
    {
    }

    public Triple(T first, U second, V third)
    {
        this.First = first;
        this.Second = second;
        this.Third = third;
    }

    /// <summary>
    /// The first value, corresponds to the first type 'T'
    /// </summary>
    public T First { get; set; }

    /// <summary>
    /// The second value, corresponds to the second type 'U'
    /// </summary>
    public U Second { get; set; }

    /// <summary>
    /// The third value, corresponds to the third type 'V'
    /// </summary>
    public V Third { get; set; }
};


