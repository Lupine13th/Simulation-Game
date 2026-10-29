using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum StatusDataType
{
    Type = 0,
    MaxSoldier = 1,
    MaxMorale = 2,
    Attack = 3,
    Defence = 4,
    Occupy = 5,
    MoveSpeed = 6,
    FundCost = 7,
    ResourceCost = 8,
}

public enum BuildingDataType
{
    Type = 0,
    DispName = 1,
    Value = 2,
    FundCost = 3,
    ResourceCost = 4,
}

public enum AnimationType
{
    None,
    Idle,
    Fire
}

public class AddressableManager : SingletonManager<AddressableManager>
{
    /// <summary>
    /// マスターデータのアドレス
    /// </summary>
    public Dictionary<MasterData, string> MasterAddress { get; private set; } = new Dictionary<MasterData, string>()
    {
        {MasterData.Ideology, "IdeologyMasterData" },
        {MasterData.State, "StateMasterData" },
        {MasterData.FOGoverment, "FOGovermentMasterData" },
        {MasterData.ForceStatus, "ForceStatusMasterData" },
        {MasterData.InfantryPreset, "InfantryPresetMasterData" },
        {MasterData.Building, "BuildingMasterData" },
    };

    public Dictionary<AnimationType, string> AnimationAddress { get; private set; } = new Dictionary<AnimationType, string>()
    {
        {AnimationType.Idle, "RifleIdle"},
        {AnimationType.Fire, "RifleFire"},
    };

    public void ParseCsvIdeologyType(string csv, Dictionary<LIdeology, List<string>> ideologyTextList)   //CSVデータを行ごとに並び替え(引数２は状況に応じて変わるため必要)
    {
        using (StringReader reader = new StringReader(csv))
        {
            string line;
            string headerLine = reader.ReadLine();  //一行目(イデオロギー)を読み取り
            if (headerLine == null) return;
            string[] headers = headerLine.Split(',');

            Dictionary<int, LIdeology> HeaderList = new Dictionary<int, LIdeology>();

            for (int i = 0; i < headers.Length; i++)    //ヘッダーとインデックスを掛け合わせる
            {
                if (System.Enum.TryParse(headers[i].Trim(), out LIdeology parsedEnum))
                {
                    HeaderList[i] = parsedEnum;

                    if (!ideologyTextList.ContainsKey(parsedEnum))  //もし現在入力中のリストにイデオロギーのキーが無ければ
                    {
                        ideologyTextList[parsedEnum] = new List<string>();
                    }
                }
            }

            while ((line = reader.ReadLine()) != null)
            {
                string[] values = line.Split(",");

                for (int i = 0; i < values.Length; i++)
                {
                    if (HeaderList.ContainsKey(i))
                    {
                        string textData = values[i].Trim();

                        //空白のセル（データが入っていないマス）は無視する
                        if (!string.IsNullOrEmpty(textData))
                        {
                            LIdeology ideologyType = HeaderList[i];
                            ideologyTextList[ideologyType].Add(textData);   //該当するイデオロギーのリストに追加
                        }
                    }
                }
            }
        }
    }

    ///<summary>
    ///CSVファイルからステート情報を読み取る
    /// </summary>
    public void ParseCsvStateData(string csv, Dictionary<int, StateData> stateDataDict)   //CSVデータを行ごとに並び替え(引数２は状況に応じて変わるため必要)
    {
        using (StringReader reader = new StringReader(csv))
        {
            string line;
            bool isHeader = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                string[] values = line.Split(",");

                StateData data = new StateData();
                data.StateName = values[1];
                data.StateNameJP = values[2];
                data.StateCulture = values[3];
                data.StateCultureJP = values[4];

                //1列目の文字列（ID）を int型の数値に変換
                if (int.TryParse(values[0].Trim(), out int id))
                {
                    // 辞書に追加（すでに同じキーがあったら上書きする書き方）
                    stateDataDict[id] = data;
                }
                else
                {
                    Debug.LogWarning($"IDの変換に失敗しました: {values[0]}");
                }
            }
        }
    }

    ///<summary>
    ///CSVファイルから軍団ステータス情報を読み取る
    /// </summary>
    public void ParseCsvStatusData(string csv, Dictionary<ForceType, EnlistData> statusDataDict)   //CSVデータを行ごとに並び替え(引数２は状況に応じて変わるため必要)
    {
        using (StringReader reader = new StringReader(csv))
        {
            string line;
            bool isHeader = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;           //一列目は説明だから無視
                }

                string[] values = line.Split(",");

                EnlistData data = new EnlistData();

                if (Enum.TryParse(values[(int)StatusDataType.Type], ignoreCase: true, out ForceType result))   //兵種名が兵種のenumと一致しているか
                {
                    data.ForceType = result;    //一致した場合、データに挿入
                }
                else
                {
                    continue;   //無い場合、次の列へ
                }

                data.MaxSoldiers = int.Parse(values[(int)StatusDataType.MaxSoldier]);   //最大兵数を挿入
                data.MaxMorales = int.Parse(values[(int)StatusDataType.MaxMorale]);     //最大士気を挿入
                data.Attack = int.Parse(values[(int)StatusDataType.Attack]);            //攻撃値を挿入
                data.Defence = int.Parse(values[(int)StatusDataType.Defence]);          //防衛値を挿入
                data.Occupy = int.Parse(values[(int)StatusDataType.Occupy]);            //占領値を挿入
                data.MoveSpeed = int.Parse(values[(int)StatusDataType.MoveSpeed]);      //移動値を挿入
                data.FundCost = int.Parse(values[(int)StatusDataType.FundCost]);        //資金コストを挿入
                data.ResourceCost = int.Parse(values[(int)StatusDataType.ResourceCost]);//資源コストを挿入

                statusDataDict[result] = data;  //データを挿入
            }
        }
    }

    ///<summary>
    ///CSVファイルから建築物情報を読み取る
    /// </summary>
    public void ParseCsvBuildingData(string csv, Dictionary<BuildingType, BuildingData> buildingDict)   //CSVデータを行ごとに並び替え(引数２は状況に応じて変わるため必要)
    {
        using (StringReader reader = new StringReader(csv))
        {
            string line;
            bool isHeader = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;           //一列目は説明だから無視
                }

                string[] values = line.Split(",");

                BuildingData data = new BuildingData();

                if (Enum.TryParse(values[(int)BuildingDataType.Type], ignoreCase: true, out BuildingType result))   //兵種名が兵種のenumと一致しているか
                {
                    data.type = result;
                }
                else
                {
                    continue;   //無い場合、次の列へ
                }

                data.dispName = values[(int)BuildingDataType.DispName];                     //表示名を挿入
                data.value = values[(int)BuildingDataType.Value];                           //効果を挿入
                data.fundCost = int.Parse(values[(int)BuildingDataType.FundCost]);          //資金コストを挿入
                data.resourceCost = int.Parse(values[(int)BuildingDataType.ResourceCost]);  //資源コストを挿入

                buildingDict[result] = data;  //データを挿入
            }
        }
    }

    ///<summary>
    ///CSVファイルから文字列を取得
    /// </summary>
    public async Task<string> LoadCsvTextAsync(string address)  
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>(address);
        await handle.Task;
        if (handle.Result != null)
        {
            return handle.Result.text;
        }
        else
        {
            return null;
        }
    }

    ///<summary>
    ///CSVファイルからSpriteを取得
    /// </summary>
    public async Task<Sprite> LoadSpriteAsync(string address)  
    {
        var handle = Addressables.LoadAssetAsync<Texture2D>(address);
        await handle.Task;
        if (handle.Result != null)
        {
            Rect rect = new Rect(0, 0, handle.Result.width, handle.Result.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f); // 中央基準

            Sprite textureSprite = Sprite.Create(handle.Result, rect, pivot);

            return textureSprite;
        }
        else
        {
            return null;
        }
    }

    public string GetRandomTextIdeoloryType(LIdeology targetIdeology, Dictionary<LIdeology, List<string>> ideologyTextList)
    {
        //辞書の中に指定されたイデオロギーのリストが存在するかチェック
        if (ideologyTextList.TryGetValue(targetIdeology, out List<string> textList) && textList.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, textList.Count);
            return textList[randomIndex];
        }

        return "（該当するテキストがありません）";
    }

    public StateData GetRandomStateData(List<int> excludeList, Dictionary<int, StateData> stateDataDict)
    {
        List<int> candidatesId = Enumerable.Range(0, stateDataDict.Count).ToList(); //StateDataの数だけintを並べる
        
        HashSet<int> excludeSet = new HashSet<int>(excludeList);    //除外する値を除外リスト(引数)から引き出す
        candidatesId.RemoveAll(i => excludeSet.Contains(i));        //それらを除外

        int currentStateDataId = 0;

        if (candidatesId.Count > 0) //残っているステートデータの中からランダムでデータを取る
        {
            int randomIndex = UnityEngine.Random.Range(0, candidatesId.Count);
            currentStateDataId = candidatesId[randomIndex];
        }

        //辞書の中に指定されたステートデータが存在するかチェック
        if (stateDataDict.TryGetValue(currentStateDataId, out StateData stateData))
        {
            excludeList.Add(currentStateDataId);
            return stateData;
        }

        return default;
    }

    public int GetTextureCount(string folderPath)
    {
        string[] searchFolders = new string[] { folderPath };
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", searchFolders);

        return guids.Length;
    }

    /// <summary>
    /// アニメーションをアドレスから取得
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<AnimationClip> GetAnimationClipFromAddress(AnimationType type)
    {
        var handle = Addressables.LoadAssetAsync<AnimationClip>(AnimationAddress[type] + "[" + AnimationAddress[type] + "]");   //アニメーションクリップのアドレス
        await handle.Task;
        if (handle.Result != null)
        {
            Debug.Log("アニメーション取得成功");
            return handle.Result;
        }
        else
        {
            Debug.LogError("アニメーション取得失敗！");
            return null;
        }
    }
}
