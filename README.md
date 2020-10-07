# UniScriptableObjectForPreferences


Preferences に簡単にメニューを追加できるエディタ拡張

## 使い方

```cs
using Kogane;
using UnityEditor;
using UnityEngine;

public class MySettings : ScriptableObjectForPreferences<MySettings>
{
    [SerializeField] private int    m_id   = 25;
    [SerializeField] private string m_name = "ピカチュウ";

    public int    Id   => m_id;
    public string Name => m_name;

    // Project Settings のメニューに登録
    [SettingsProvider]
    private static SettingsProvider SettingsProvider()
    {
        return CreateSettingsProvider
        (
            settingsProviderPath: "MyProject/MySettings", // メニューの名前を指定できます
            onGUI: null,                                  // メニューの GUI を上書きしたい場合は指定します
            onGUIExtra: null                              // メニューの末尾に GUI を追加したい場合は指定します
        );
    }
}
```

例えば上記のようなクラスを作成すると  

![2020-10-07_201430](https://user-images.githubusercontent.com/6134875/95323778-aab75880-08d9-11eb-888b-12c2017d34dd.png)

Preferences にこのようにメニューを追加できます  

```cs
using UnityEditor;
using UnityEngine;

public class Example
{
    [MenuItem( "Tools/Hoge" )]
    private static void Hoge()
    {
        var settings = MySettings.GetInstance();

        Debug.Log( settings.Id );
        Debug.Log( settings.Name );
    }
}
```

メニューの設定にアクセスしたい場合はこのようなコードを記述します  

設定は Unity プロジェクトの「UserSettings/EditorUserSettings.asset」に保存されます
