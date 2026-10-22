using UnityEngine;

public class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    // 外部からアクセスするための静的プロパティ
    public static T Instance { get; private set; }

    private void Awake()
    {
        // すでにインスタンスが存在しているかチェック
        if (Instance != null && Instance != this)
        {
            // 2つ目以降の重複インスタンスは破棄する
            Destroy(gameObject);
            return;
        }

        // 自身をインスタンスとして登録
        Instance = this as T;
    }
}

