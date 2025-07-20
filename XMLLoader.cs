using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AvaterInfo
{
    public class XMLLoader
    {
        public enum keys
        {
            NAME,
            Hips,
            LeftUpperLeg,
            RightUpperLeg,
            LeftLowerLeg,
            RightLowerLeg,
            LeftFoot,
            RightFoot,
            Spine,
            Chest,
            UpperChest,
            Neck,
            Head,
            LeftShoulder,
            RightShoulder,
            LeftUpperArm,
            RightUpperArm,
            LeftLowerArm,
            RightLowerArm,
            LeftHand,
            RightHand,
            LeftToes,
            RightToes,
            LeftEye,
            RightEye,
            Jaw,
            LeftThumbProximal,
            LeftThumbIntermediate,
            LeftThumbDistal,
            LeftIndexProximal,
            LeftIndexIntermediate,
            LeftIndexDistal,
            LeftMiddleProximal,
            LeftMiddleIntermediate,
            LeftMiddleDistal,
            LeftRingProximal,
            LeftRingIntermediate,
            LeftRingDistal,
            LeftLittleProximal,
            LeftLittleIntermediate,
            LeftLittleDistal,
            RightThumbProximal,
            RightThumbIntermediate,
            RightThumbDistal,
            RightIndexProximal,
            RightIndexIntermediate,
            RightIndexDistal,
            RightMiddleProximal,
            RightMiddleIntermediate,
            RightMiddleDistal,
            RightRingProximal,
            RightRingIntermediate,
            RightRingDistal,
            RightLittleProximal,
            RightLittleIntermediate,
            RightLittleDistal,
            LastBone,
            RightBreastRoot,
            RightBreastMid,
            RightBreastEnd,
            LeftBreastRoot,
            LeftBreastMid,
            LeftBreastEnd
        }
        static XElement xml;
        public Dictionary<string, Dictionary<string, string>> avatarBoneData = new Dictionary<string, Dictionary<string, string>>();

        public XMLLoader()
        {
            
        }
        public void LoadXML()
        {
            // 辞書データのクリア
            avatarBoneData.Clear();
            
            // GUIDからアセットの相対パスを取得
            string assetRelativePath = AssetDatabase.GUIDToAssetPath("d40dcc3ccc58a0844b1469e69de93942");
            string absoluteRootPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), assetRelativePath);

            if (!Directory.Exists(absoluteRootPath))
            {
                Debug.LogError("ディレクトリが存在しません: " + absoluteRootPath);
                return;
            }

            // 全てのXMLファイルを取得
            string[] xmlFiles = Directory.GetFiles(absoluteRootPath, "*.xml", SearchOption.AllDirectories);

            foreach (var file in xmlFiles)
            {
                try
                {
                    XElement root = XElement.Load(file);
                    XElement avatarElement = root.Name == "Avatar" ? root : root.Element("Avatar");

                    if (avatarElement == null)
                    {
                        Debug.LogWarning($"Avatar要素が見つかりません: {file}");
                        continue;
                    }

                    string nameValue = avatarElement.Element("NAME")?.Value ?? Path.GetFileNameWithoutExtension(file);

                    // 相対アセットパスに変換（Assets/～）
                    string assetFilePath = "Assets" + file.Substring(Application.dataPath.Length).Replace("\\", "/");

                    // assetRelativePath からの相対ディレクトリを取得
                    string relativeDirectory = Path.GetDirectoryName(assetFilePath).Replace("\\", "/");
                    if (relativeDirectory.StartsWith(assetRelativePath))
                    {
                        relativeDirectory = relativeDirectory.Substring(assetRelativePath.Length).TrimStart('/');
                    }

                    // アバター名を「ディレクトリ名/NAME値」として設定
                    string avatarName = string.IsNullOrEmpty(relativeDirectory)
                        ? nameValue
                        : $"{relativeDirectory}/{nameValue}";

                    Dictionary<string, string> boneMap = new Dictionary<string, string>();
                    foreach (var bone in avatarElement.Elements())
                    {
                        if (bone.Name.LocalName == "NAME") continue;
                        boneMap[bone.Name.LocalName] = bone.Value;
                    }

                    avatarBoneData[avatarName] = boneMap;

                    //Debug.Log($"読み込み成功: {avatarName}（{boneMap.Count}個のボーン）");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"XMLの読み込み失敗: {file}\n{ex.Message}");
                }
            }
        }


        /// <summary>
        /// 登録されているアバター名の配列を返す
        /// </summary>
        public string[] GetAvaterNames()
        {
            return avatarBoneData.Keys.ToArray();
        }

        /// <summary>
        /// 指定キーに対応するすべてのアバターのボーン名を取得する
        /// </summary>
        public string[] GetBoneNameFromKey(keys key)
        {
            string keyName = key.ToString();
            List<string> results = new List<string>();

            foreach (var kv in avatarBoneData)
            {
                if (kv.Value.TryGetValue(keyName, out string value))
                {
                    results.Add(value);
                }
                else
                {
                    results.Add(""); // 空文字で埋める（または "NONE" など任意で）
                }
            }

            return results.ToArray();
        }
        public Dictionary<string, string> GetBoneMapByAvatarName(string avatarName)
        {
            if (avatarBoneData.TryGetValue(avatarName, out var map))
            {
                return map;
            }
            return null;
        }

    }
}

