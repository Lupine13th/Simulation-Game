using UnityEngine;

public class ModelManager : SingletonManager<ModelManager>
{
    /// <summary>
    /// 駐屯モデル変更
    /// </summary>
    /// <param name="on"></param>
    public void ChangeGuarrisonModel(bool on, State target, Force force)
    {
        switch (on)
        {
            case true:  //駐屯モデルオン
                if (target.GuarrisonModel == null) //ステート状のモデルがいない場合
                {
                    GameObject model = Instantiate(BattleSystemManager.Instance.SoldierPrefab, target.MidModelPos, Quaternion.identity, transform); //モデル生成

                    target.SetGuarrisonModel(model);    

                    Animator anim = target.GuarrisonModel.GetComponentInChildren<Animator>();       //Animator取得
                    anim.Play(AddressableManager.Instance.AnimationAddress[AnimationType.Idle]);    //アイドル状態              //TODO:Animatorはややこしいので一旦スルー

                    WorldUIManager.Instance.InstantiateForceUI(force, model);   //モデル追従用のUIを生成

                    Debug.Log("駐屯モデル生成：" + target.StateNameJP);
                }
                break;
            case false:  //駐屯モデルオフ
                if (target.GuarrisonModel != null) //ステート状のモデルがいる場合
                {
                    WorldUIManager.Instance.RemoveForceUI(force);   //UI削除
                    Destroy(target.GuarrisonModel);                 //モデル削除
                    Debug.Log("駐屯モデル削除：" + target.StateNameJP);
                }
                break;
        }
    }

    /// <summary>
    /// 戦闘モデル変更
    /// </summary>
    /// <param name="on"></param>
    /// <param name="target">侵攻先のステート</param>
    public void ChangeBattleModel(bool on, State target, CountryData country, Force force)
    {
        switch (on)
        {
            case true:  //攻撃モデルオン
                if (country.BattleModelDict[target] == null) //ステート状のモデルがいない場合
                {
                    Vector3 borderMidPos = Vector3.zero;

                    foreach (var prov in country.NeighborStateDict[target]) //指定したステートの国境のプロヴィンスを取得
                    {
                        borderMidPos += prov.proviince.transform.position;
                    }

                    borderMidPos = borderMidPos / country.NeighborStateDict[target].Count;  //平均値を取って国境の中央座標を取得

                    GameObject model = Instantiate(BattleSystemManager.Instance.SoldierPrefab, borderMidPos, Quaternion.identity, transform); //モデル生成

                    country.SetBattleModel(model, target);

                    Animator anim = country.BattleModelDict[target].GetComponentInChildren<Animator>();     //Animator取得
                    anim.Play(AddressableManager.Instance.AnimationAddress[AnimationType.Fire]);    //アイドル状態              //TODO:Animatorはややこしいので一旦スルー

                    WorldUIManager.Instance.InstantiateForceUI(force, model);   //モデル追従用のUIを生成

                    Debug.Log("駐屯モデル生成：" + target.StateNameJP);
                }
                break;
            case false:  //攻撃モデルオフ
                if (country.BattleModelDict[target] != null) //ステート状のモデルがいる場合
                {
                    WorldUIManager.Instance.RemoveForceUI(force);   //UI削除
                    Destroy(country.BattleModelDict[target]);       //モデル削除
                    Debug.Log("駐屯モデル削除：" + target.StateNameJP);
                }
                break;
        }
    }
}
