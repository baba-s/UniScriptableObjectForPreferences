using JetBrains.Annotations;
using System;
using UnityEditor;
using UnityEngine;

namespace Kogane
{
	/// <summary>
	/// Preferences に表示する ScriptableObject を管理するクラス
	/// </summary>
	public abstract class ScriptableObjectForPreferences<T> : ScriptableObject
		where T : ScriptableObjectForPreferences<T>
	{
		//================================================================================
		// 変数(static)
		//================================================================================
		// EditorUserSettings から読み込んだ ScriptableObject のインスタンスをキャッシュするための変数
		private static T m_instance;

		//================================================================================
		// プロパティ(static)
		//================================================================================
		private static string ConfigName => typeof( T ).Name;

		//================================================================================
		// 関数(static)
		//================================================================================
		/// <summary>
		/// EditorUserSettings から ScriptableObject のインスタンスを読み込んで返します
		/// EditorUserSettings に保存されていない場合は ScriptableObject のインスタンスを新規作成して返します
		/// </summary>
		public static T GetInstance()
		{
			// 既にインスタンスを作成もしくは読み込み済みの場合はそれを返す
			if ( m_instance != null ) return m_instance;

			// EditorUserSettings から JSON 形式で ScriptableObject を読み込む
			m_instance = CreateInstance<T>();
			var json = EditorUserSettings.GetConfigValue( ConfigName );
			EditorJsonUtility.FromJsonOverwrite( json, m_instance );

			// EditorUserSettings に保存されていなかった場合は
			// インスタンスを新規作成する
			if ( m_instance == null )
			{
				m_instance = CreateInstance<T>();
			}

			return m_instance;
		}

		/// <summary>
		/// Preferences に表示する SettingsProvider を作成して返します
		/// </summary>
		public static SettingsProvider CreateSettingsProvider
		(
			[CanBeNull] string                   settingsProviderPath = null,
			[CanBeNull] Action<SerializedObject> onGUI                = null,
			[CanBeNull] Action<SerializedObject> onGUIExtra           = null
		)
		{
			if ( settingsProviderPath == null )
			{
				settingsProviderPath = $"Kogane/{typeof( T ).Name}";
			}

			// ScriptableObject のインスタンスを新規作成もしくは EditorUserSettings から読み込む
			// ScriptableObject の GUI を表示する SettingsProvider を作成する
			var instance         = GetInstance();
			var serializedObject = new SerializedObject( instance );
			var keywords         = SettingsProvider.GetSearchKeywordsFromSerializedObject( serializedObject );
			var provider         = new SettingsProvider( settingsProviderPath, SettingsScope.User, keywords );

			provider.guiHandler += _ => OnGuiHandler( onGUI, onGUIExtra );

			return provider;
		}

		/// <summary>
		/// SettingsProvider の GUI を描画する時に呼び出されます
		/// </summary>
		private static void OnGuiHandler
		(
			Action<SerializedObject> onGUI,
			Action<SerializedObject> onGUIExtra
		)
		{
			var instance = GetInstance();
			var editor   = Editor.CreateEditor( instance );

			using ( var scope = new EditorGUI.ChangeCheckScope() )
			{
				var serializedObject = editor.serializedObject;

				serializedObject.Update();

				// onGUI が指定されている場合はそれを描画する
				if ( onGUI != null )
				{
					onGUI( serializedObject );
				}
				else
				{
					// onGUI が指定されていない場合はデフォルトの Inspector を描画する
					editor.DrawDefaultInspector();
				}

				onGUIExtra?.Invoke( serializedObject );

				if ( !scope.changed ) return;

				// パラメータが編集された場合は インスタンスに反映して
				// なおかつ EditorUserSettings にも保存する
				serializedObject.ApplyModifiedProperties();

				var json = EditorJsonUtility.ToJson( editor.target );
				EditorUserSettings.SetConfigValue( ConfigName, json );
			}
		}
	}
}