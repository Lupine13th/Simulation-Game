using UnityEngine;

public class CommanderData
{
    public int Attack {  get; private set; }

    public int Defence {  get; private set; }

    public int Logitics {  get; private set; }

    public int Supply {  get; private set; }

    public Sprite Portrait { get; private set; }

    public void Init()
    {
        Attack = Random.Range(0, 5);
        Defence = Random.Range(0, 5);
        Logitics = Random.Range(0, 5);
        Supply = Random.Range(0, 5);
    }

    /// <summary>
    /// ポートレート設定
    /// </summary>
    /// <param name="sprite"></param>
    public void SetPortrait(Sprite sprite)
    {
        Portrait = sprite;
    }
}
