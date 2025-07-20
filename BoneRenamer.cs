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
        {
            xml = new XMLLoader();
        }
        xml.LoadXML();

        GUILayout.Label("ボーン名リネームツール", EditorStyles.boldLabel);

        GUILayout.Label("ボーン名を変更したいオブジェクトを指定");
        TargetObj = (GameObject)EditorGUILayout.ObjectField("GameObject", TargetObj, typeof(GameObject), true);

        using (new EditorGUILayout.VerticalScope("Box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("アバター辞書");
                if (GUILayout.Button("辞書を再読み込み"))
                {
                    xml.LoadXML();
                    ResultMessage = "再読み込み完了";
                    Debug.Log("再読み込み完了");
                }
            };
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("アバター辞書の読み込み状態: " + (xml.GetAvaterNames().Length) + "件読み込み完了。");
            }
        }
        
        GUILayout.Space(10);
        
        string[] avaterNames = xml.GetAvaterNames();
        GUILayout.Label("変更先のアバター名");

        if (avaterNames.Length > 0)
        {
            SelectIndex = EditorGUILayout.Popup(SelectIndex, avaterNames);
        }
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("ボーン名の置換を行います。\n" +
                                "アバター名を選択し、対象のオブジェクトを指定して適用ボタンを押してください。\n" +
                                "先頭一致でリネームを有効にすると、ボーン名の先頭が一致する場合に置換が行われます。\n" +
                                "注意: この処理は元に戻せませんので、事前にバックアップを取ってください。", MessageType.Info);
        SelectUseStartWith = GUILayout.Toggle(SelectUseStartWith, "先頭一致でリネーム");
        GUILayout.Space(5);
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
        
        if (children.childCount == 0) {
            return false;
        }
        foreach (Transform ob in children)
        {
            string tName = ob.gameObject.name;
            foreach (XMLLoader.keys key in Enum.GetValues(typeof(XMLLoader.keys))) 
            {
                string[] bones = xml.GetBoneNameFromKey(key);

                //先頭一致非使用
                if(! SelectUseStartWith)
                {
                    for (int i = 0; i < bones.Length; i++)
                    {
                        //Debug.Log("検索:" +tName + "/ "+ bones[i]);
                        if (tName == bones[i])
                        {
                            Debug.Log("置換:" + tName + " -> " + bones[SelectIndex]);
                            ob.gameObject.name = bones[SelectIndex];
                            matchCount++;
                            break;
                        }
                    }
                }
                //先頭一致使用
                else
                {
                    string rep = "";
                    for (int i = 0; i < bones.Length; i++)
                    {
                        if (tName.Equals(bones[SelectIndex]))
                            continue;
                        //Debug.Log("検索:" +tName + "/ "+ bones[i]);
                        if (tName.StartsWith(bones[i]))
                        {
                            //Debug.Log("合致:" + tName + " / " + bones[i]);
                            if (rep.Length <= bones[i].Length)  
                                rep = bones[i];
                        }
                    }
                    if(!rep.Equals(""))
                    {
                        Debug.Log("置換：" + tName+ " : " + rep + " -> " + bones[SelectIndex]);
                        matchCount++;
                        ob.gameObject.name = tName.Replace(rep, bones[SelectIndex]);
                    }
                }
            }
            BoneRenameProcess(ob.gameObject, avatarKey, ref matchCount);
        }
        return true;
    }

}
