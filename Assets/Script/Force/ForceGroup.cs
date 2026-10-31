using System.Collections.Generic;
using UnityEngine;

public class ForceGroup : MonoBehaviour
{
    /// <summary>
    /// このステート及び国境にいる軍団
    /// </summary>
    [SerializeField] private List<Force> m_ThisPlaceForceList = new List<Force>();

    /// <summary>
    /// 攻撃中のステート(攻撃していない時はnull)
    /// </summary>
    [SerializeField] private State m_AttackingState;

    /// <summary>
    /// 駐屯中のステート(攻撃している時はnull)
    /// </summary>
    [SerializeField] private State m_GuardingState;

    /// <summary>
    /// このステート及び国境にいる軍団
    /// </summary>
    public List<Force> ThisPlaceForceList { get => m_ThisPlaceForceList; }

    /// <summary>
    /// 攻撃中のステート(攻撃していない時はnull)
    /// </summary>
    public State AttackingState { get => m_AttackingState; }

    /// <summary>
    /// 駐屯中のステート(攻撃している時はnull)
    /// </summary>
    public State GuardingState { get => m_GuardingState; }
}
