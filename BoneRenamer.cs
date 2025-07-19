using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Xml.Linq;
using AvaterInfo;
using Unity.Collections;
using System;
using UnityEngine.UI;

public class BoneRenamer : EditorWindow
{
    static XMLLoader xml;
    static string ResultMessage;
    static GameObject TargetObj;
    static int SelectIndex = 0;
    static bool SelectUseStartWith;

    [MenuItem("Tools/BoneRenamer")]
    public static void BoneRename()
    {
        GetWindow<BoneRenamer> ();
    }
    private void OnGUI()
    {
        if (xml == null)
            xml = new XMLLoader();

        GUILayout.Label("ボーン名を変更したいオブジェクトを指定");
        TargetObj = (GameObject)EditorGUILayout.ObjectField("GameObject", TargetObj, typeof(GameObject), true);

        if (GUILayout.Button("アバター辞書を再読み込み"))
        {
            xml.LoadXML();
            ResultMessage = "再読み込み完了";
            Debug.Log("再読み込み完了");
        }

        GUILayout.Space(5);
        string[] avaterNames = xml.GetAvaterNames();
        GUILayout.Label("変更先のアバター名");

        if (avaterNames.Length > 0)
        {
            SelectIndex = EditorGUILayout.Popup(SelectIndex, avaterNames);
        }

        SelectUseStartWith = GUILayout.Toggle(SelectUseStartWith, "先頭一致でリネーム");

        if (GUILayout.Button("適用"))
        {
            int matchCount = 0;
            if (TargetObj == null)
            {
                ResultMessage = "Objectが設定されていません";
            }
            else if (avaterNames.Length == 0)
            {
                ResultMessage = "アバター名が読み込まれていません";
            }
            else
            {
                if (BoneRenameProcess(TargetObj, avaterNames[SelectIndex], ref matchCount))
                {
                    ResultMessage = $"正常に処理が終了しました ボーン置換数: {matchCount}";
                    Debug.Log(ResultMessage);
                }
            }
        }

        GUILayout.Label(ResultMessage);
    }
    private static bool BoneRenameProcess(GameObject obj, string avatarKey, ref int matchCount)
    {
        var boneMap = xml.GetBoneMapByAvatarName(avatarKey);
        if (boneMap == null)
        {
            Debug.LogError("選択されたアバターに対応するボーンデータが見つかりません: " + avatarKey);
            return false;
        }

        Transform children = obj.GetComponentInChildren<Transform>();
        if (children.childCount == 0)
        {
            return false;
        }

        foreach (Transform ob in children)
        {
            string tName = ob.gameObject.name;

            foreach (var kv in boneMap)
            {
                string boneKey = kv.Key;
                string boneName = kv.Value;

                if (!SelectUseStartWith)
                {
                    if (tName == boneName)
                    {
                        Debug.Log($"置換: {tName} → {boneName}");
                        ob.gameObject.name = boneName;
                        matchCount++;
                        break;
                    }
                }
                else
                {
                    if (tName.StartsWith(boneName) && tName != boneName)
                    {
                        Debug.Log($"置換: {tName}（StartsWith: {boneName}）→ {boneName + tName.Substring(boneName.Length)}");
                        ob.gameObject.name = boneName + tName.Substring(boneName.Length);
                        matchCount++;
                        break;
                    }
                }
            }

            // 再帰処理
            BoneRenameProcess(ob.gameObject, avatarKey, ref matchCount);
        }

        return true;
    }

}
