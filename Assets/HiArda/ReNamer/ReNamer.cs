using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace HiArda
{
    public class ReNamerWindow : EditorWindow
    {
        // 現在対象としているパス以下に存在するファイルリスト
        List<string> files = new List<string>();

        // ファイルリストを正規表現によって置換したプレビュー
        List<string> previews = new List<string>();

        // 現在対象としているパス
        string selectPathPrevious = "";
        string selectPath = "";

        // 正規表現テキスト
        string patternTextPrevious = "";
        string patternText = "";

        // 置換後テキスト
        string replaceTextPrevious = "";
        string replaceText = "";

        // スクロールビューの現在地
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPreview = Vector2.zero;

        // メニューから呼ばれる関数
        [MenuItem("Assets/ReNamer/File ReNamer")]
        public static void ShowFileRenamerWindow()
        {
            var window = EditorWindow.GetWindow<ReNamerWindow>("File ReNamer");
            window.selectPath = AssetDatabase.GetAssetPath(Selection.GetFiltered(typeof(DefaultAsset), SelectionMode.Assets)[0]);
            window.EnumerateFiles(window.selectPath);
        }

        void OnGUI()
        {
            // 現在対象としているパスを表示
            GUILayout.Label("Path:");
            selectPath = EditorGUILayout.TextField("", selectPath);
            if (selectPath != selectPathPrevious)
            {
                EnumerateFiles(selectPath);
                selectPathPrevious = selectPath;
            }
            GUILayout.Space(20);

            // 正規表現のパターンを入力するフィールド
            EditorGUILayout.LabelField("Pattern:", EditorStyles.boldLabel);
            patternText = EditorGUILayout.TextField("", patternText);
            if (patternText != patternTextPrevious)
            {
                Preview();
                patternTextPrevious = patternText;
            }
            GUILayout.Space(10);

            // 置換後のテキストを入力するフィールド
            EditorGUILayout.LabelField("Replace:", EditorStyles.boldLabel);
            replaceText = EditorGUILayout.TextField("", replaceText);
            if (replaceText != replaceTextPrevious)
            {
                Preview();
                replaceTextPrevious = replaceText;
            }
            GUILayout.Space(10);

            // 置換実行
            if (GUILayout.Button("Replace"))
            {
                Debug.Log("[FileReNamer] Start Replace");
                foreach (var file in files)
                {
                    if (string.IsNullOrEmpty(file) == false)
                    {
                        string newPath = Regex.Replace(file, patternText, replaceText);
                        string newFile = Path.GetFileNameWithoutExtension(newPath);
                        AssetDatabase.RenameAsset(file, newFile);
                        Debug.Log("[FileReNamer] Replace old=" + file + " new=" + newPath);
                    }
                }
                AssetDatabase.SaveAssets();
                EnumerateFiles(selectPath);
                Debug.Log("[FileReNamer] End Replace");
            }
            GUILayout.Space(20);

            // 現在置換対象となるファイル達を列挙する
            EditorGUILayout.LabelField("Target Files:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var file in files)
            {
                GUILayout.Label(file);
            }
            EditorGUILayout.EndScrollView();

            // 置換ボタンを押した時に変換される結果のプレビューを表示
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Preview Files:", EditorStyles.boldLabel);
            scrollPreview = EditorGUILayout.BeginScrollView(scrollPreview);
            foreach (var preview in previews)
            {
                GUILayout.Label(preview);
            }
            EditorGUILayout.EndScrollView();
        }

        // 置換結果プレビュー構築
        void Preview()
        {
            previews.Clear();
            foreach (var file in files)
            {
                previews.Add(Regex.Replace(file, patternText, replaceText));
            }
        }

        // 指定パス以下のファイルを列挙
        void EnumerateFiles(string directoryPath)
        {
            files.Clear();
            if (Directory.Exists(directoryPath))
            {
                string[] filePaths = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                foreach (string filePath in filePaths)
                {
                    if (filePath.EndsWith(".meta") == false)
                    {
                        files.Add(filePath);
                    }
                }
            }
            Preview();
        }
    }
}