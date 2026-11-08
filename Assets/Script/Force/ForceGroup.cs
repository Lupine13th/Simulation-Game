using System.Collections.Generic;
using UnityEngine;

public class ForceGroup
{
    /// <summary>
    /// ‚±‚ÌƒXƒe[ƒg‹y‚Ñ‘‹«‚É‚¢‚éŒR’c
    /// </summary>
    public List<Force> ThisPlaceForceList { get; private set; } = new List<Force>();

    /// <summary>
    /// UŒ‚’†‚ÌƒXƒe[ƒg(UŒ‚‚µ‚Ä‚¢‚È‚¢‚Ínull)
    /// </summary>
    public State AttackingState { get; private set; }

    /// <summary>
    /// ’““Ô’†‚ÌƒXƒe[ƒg(UŒ‚‚µ‚Ä‚¢‚é‚Ínull)
    /// </summary>
    public State GuardingState { get; private set; }

    /// <summary>
    /// ‚±‚ÌŒR’c‚Ì‘•º—Í
    /// </summary>
    public int AllSoldier { get; private set; }

    public void UpdateDatas()
    {
        int allSoldier = 0;

        foreach (var force in ThisPlaceForceList)   //ŒR’c‚Ì‘•º—Í‚ğ‘«‚µZ
        {
            allSoldier += force.Soldiers;
        }

        AllSoldier = allSoldier;
    }

    /// <summary>
    /// UŒ‚‚·‚éƒXƒe[ƒg‚ğİ’è
    /// </summary>
    /// <param name="state"></param>
    public void SetAttackState(State state)
    {
        AttackingState = state;
    }

    /// <summary>
    /// ’““Ô‚·‚éƒXƒe[ƒg‚ğİ’è
    /// </summary>
    /// <param name="state"></param>
    public void SetGuarrisonState(State state)
    {
        GuardingState = state;
    }

    /// <summary>
    /// æ“ª‚ÌŒR’c
    /// </summary>
    public Force GetListHead()
    {
        return ThisPlaceForceList[0];
    }

    /// <summary>
    /// è—ÌŠ®—¹‚µ‚½ŒR’c‚ª‚±‚Ìƒƒ\ƒbƒh‚ğŒÄ‚ÑAƒ^[ƒQƒbƒg‚ÌƒXƒe[ƒg‚É‘SŒR’c‚ğ’““Ô‚³‚¹‚é
    /// </summary>
    /// <param name="target"></param>
    public void EndOccupy(State target)
    {
        for (int i = 0; i < ThisPlaceForceList.Count; i++)
        {
            FieldManager.Instance.SetForceOnState(target, ThisPlaceForceList[i]);   //ŒR’c‚ğ’€ŸˆÚ“®‚³‚¹‚é
        }
    }

    /// <summary>
    /// ‰ó–Å‚µ‚½ŒR’c‚ğíœ
    /// </summary>
    /// <param name="force"></param>
    public void RemoveForce(Force force)
    {
        var country = force.OwnerCountry;

        ThisPlaceForceList.Remove(force);   //ŒR’c‚ğíœ

        if (ThisPlaceForceList.Count == 0)  //‚à‚µŒR’c‚ª‚¢‚È‚­‚È‚ê‚Î
        {
            if (country.AttackingForceGroupDict.ContainsValue(this))    //UŒ‚’†‚Ì‘åŒR’c‚¾‚Á‚½‚ç
            {
                foreach (var pair in country.AttackingForceGroupDict)
                {
                    if (pair.Value.Equals(this))
                    {
                        ModelManager.Instance.ChangeBattleModel(false, AttackingState, country, this);  //ƒ‚ƒfƒ‹íœ
                        WorldUIManager.Instance.RemoveForceUI(this);        //UIíœ
                        country.AttackingForceGroupDict.Remove(pair.Key);   //‘åŒR’cíœ
                        break;  //ŠY“–‚·‚é‚à‚Ì‚ª•¡”‚È‚¯‚ê‚Î‚±‚±‚ÅI—¹
                    }
                }
            }
            else if (country.GuardingForceGroupDict.ContainsValue(this))//–h‰q’†‚Ì‘åŒR’c‚¾‚Á‚½‚ç
            {
                foreach (var pair in country.GuardingForceGroupDict)
                {
                    if (pair.Value.Equals(this))
                    {
                        ModelManager.Instance.ChangeBattleModel(false, GuardingState, country, this);  //ƒ‚ƒfƒ‹íœ
                        WorldUIManager.Instance.RemoveForceUI(this);        //UIíœ
                        country.GuardingForceGroupDict.Remove(pair.Key);    //‘åŒR’cíœ
                        break;  //ŠY“–‚·‚é‚à‚Ì‚ª•¡”‚È‚¯‚ê‚Î‚±‚±‚ÅI—¹
                    }
                }
            }
        }
    }
}
