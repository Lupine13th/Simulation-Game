using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class Province : MonoBehaviour
{
    [Serializable]
    struct BorderDirection
    {
        public Direction direction;
        public GameObject borderObj;
    }

    /// <summary>
    /// このプロヴィンスが方角ごとにどのステートと隣接しているか
    /// </summary>
    public struct NeighberProvince
    {
        public Direction direction;
        public State state;
    }

    [SerializeField] private bool m_IsBorderProvince;
    [SerializeField] private bool m_OutSideProvince;
    [SerializeField] private Vector2Int m_ProvincePosition;
    [SerializeField] private List<BorderDirection> m_BorderObjList = new List<BorderDirection>();

    public Dictionary<Direction, GameObject> BorderObjDict { get; private set; } = new Dictionary<Direction, GameObject>();

    /// <summary>
    /// このプロヴィンスが隣接するステート
    /// </summary>
    public List<NeighberProvince> NeighberStateList { get; private set; } = new List<NeighberProvince>();

    public State OwnedState { get; private set; }

    public bool IsBorderProvince => m_IsBorderProvince;
    public bool OutSideProvince => m_OutSideProvince;
    public Vector2Int ProvincePosition => m_ProvincePosition;

    void Awake()
    {
        OwnedState = GetComponentInParent<State>();
        for (int i = 0; i < m_BorderObjList.Count; i++)
        {
            BorderObjDict[m_BorderObjList[i].direction] = m_BorderObjList[i].borderObj;
        }
    }

    void Update()
    {
        
    }

    public void SetProvincePosition(int x, int y)
    {
        m_ProvincePosition.x = x;
        m_ProvincePosition.y = y;
    }

    public void SetOutSideProvince(bool t)
    {
        m_OutSideProvince = t;
    }

    /// <summary>
    /// このプロヴィンスの色を変更(好きな色)
    /// </summary>
    /// <param name="color"></param>
    public void SetProvinceColor(Vector3 color)
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();

        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_BaseColor", new Color(color.x, color.y, color.z));

        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    /// <summary>
    /// それぞれの方面に隣接するプロヴィンスがあるかを探索
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool CheckNeighborProvince(Direction direction)  
    {
        switch (direction)
        {
            case Direction.East:
                return FieldManager.Instance.ProvinceDict.ContainsKey(new Vector2Int(m_ProvincePosition.x + 1, m_ProvincePosition.y)) ? true : false;
            case Direction.West:
                return FieldManager.Instance.ProvinceDict.ContainsKey(new Vector2Int(m_ProvincePosition.x - 1, m_ProvincePosition.y)) ? true : false;
            case Direction.South:
                return FieldManager.Instance.ProvinceDict.ContainsKey(new Vector2Int(m_ProvincePosition.x, m_ProvincePosition.y - 1)) ? true : false;
            case Direction.North:
                return FieldManager.Instance.ProvinceDict.ContainsKey(new Vector2Int(m_ProvincePosition.x, m_ProvincePosition.y + 1)) ? true : false;
            default:
                return false;
        }
    }

    /// <summary>
    /// それぞれの方面に隣接するステートがあるかを探索
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool CheckNeighborProvinceIsDifferentState(Direction direction)  
    {
        Vector2Int provPosition = m_ProvincePosition;
        switch (direction)
        {
            case Direction.East:
                provPosition.x += 1;
                break;
            case Direction.West:
                provPosition.x -= 1;
                break;
            case Direction.South:
                provPosition.y -= 1;
                break;
            case Direction.North:
                provPosition.y += 1;
                break;
            default:
                break;
        }

        if (FieldManager.Instance.ProvinceDict.ContainsKey(provPosition))
        {
            if (FieldManager.Instance.ProvinceDict[provPosition].OwnedState != OwnedState)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// それぞれの方面で隣接するステートを返す
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public State GetNeighborProvinceState(Direction direction)  
    {
        Vector2Int provPosition = m_ProvincePosition;
        switch (direction)
        {
            case Direction.East:
                provPosition.x += 1;
                break;
            case Direction.West:
                provPosition.x -= 1;
                break;
            case Direction.South:
                provPosition.y -= 1;
                break;
            case Direction.North:
                provPosition.y += 1;
                break;
            default:
                break;
        }

        if (FieldManager.Instance.ProvinceDict.ContainsKey(provPosition))
        {
            if (FieldManager.Instance.ProvinceDict[provPosition].OwnedState != OwnedState)
            {
                return FieldManager.Instance.ProvinceDict[provPosition].OwnedState;
            }
        }

        return null;
    }

    /// <summary>
    /// 全方面に隣接するプロヴィンスがあるかを探索  true->全方面に隣接アリ  false->どっかしら端
    /// </summary>
    /// <returns></returns>
    public bool CheckNeighborAllDirection()  
    {
        if (FieldManager.Instance.ProvinceDict.ContainsKey(new Vector2Int(m_ProvincePosition.x + 1, m_ProvincePosition.y)) == false)
        {
            return false;
        }

        if (FieldManager.Instance.ProvinceDict.ContainsKey(new Vector2Int(m_ProvincePosition.x - 1, m_ProvincePosition.y)) == false)
        {
            return false;
        }

        if (FieldManager.Instance.ProvinceDict.ContainsKey(new Vector2Int(m_ProvincePosition.x, m_ProvincePosition.y - 1)) == false)
        {
            return false;
        }

        if (FieldManager.Instance.ProvinceDict.ContainsKey(new Vector2Int(m_ProvincePosition.x, m_ProvincePosition.y + 1)) == false)
        {
            return false;
        }

        return true;
    }

    public void SetBorderObject(Direction direction)
    {
        BorderObjDict[direction].SetActive(true);
    }

    public void SetIsBorder(bool t)
    {
        m_IsBorderProvince = t;
    }

    public void SetNeighberState(Direction dir, State state)
    {
        NeighberStateList.Add(new NeighberProvince { direction = dir, state = state });
    }

/*    public bool HasNeighberState(State state)
    {
        foreach (var neighberData in NeighberStateList)
        {
            if (neighberData.state == state)
            {
                return true;
            }
        }

        return false;
    }*/
}
