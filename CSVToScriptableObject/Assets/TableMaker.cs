using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class TableMaker : MonoBehaviour
{    
    public static string CSVFolderPath = "Assets/Tables";
    public static string ScriptableFolderPath = "Assets/Resources/ScriptableObject";


    public static void MakeTableScript()
    {
        try
        {
            string[] guids = AssetDatabase.FindAssets("", new string[] { CSVFolderPath });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                if (asset == null || !path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    continue;

                List<Dictionary<string, object>> tableDataList = TableCSVReader.Read(asset, out string[] header, out string[] types);

                if (header.Length == 0 || types.Length == 0)
                    throw new Exception($"{nameof(TableMaker)} : Table Header or Type Error");

                WriteCode(asset.name, header, types);
            }

            EditorUtility.SetDirty(scriptableObj);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
        catch(Exception e)
        {
            Debug.LogError($"{nameof(TableMaker)} : {e.Message}");
        }
    }


    private static void WriteCode(string tableName, string[] header, string[] types)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();

        sb.AppendLine($"public class {tableName} : SingletonScriptableObject<{tableName}>");
        sb.AppendLine("{");

        sb.AppendLine("\t[SerializeField]");
        sb.AppendLine("\tpublic List<TableData> datas = new List<TableData>();");
        sb.AppendLine();

        sb.AppendLine("\tpublic TableData this[int index]");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tget");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t\t\treturn datas.Find(x => x.ID == index);");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");

        sb.AppendLine();

        sb.AppendLine("\t[Serializable]");
        sb.AppendLine("\tpublic class TableData");
        sb.AppendLine("\t{");

        for (int i = 0; i < header.Length; i++)
        {
            sb.AppendLine($"\t\tpublic {types[i]} {header[i]};");
        }

        sb.AppendLine("\t}");
        sb.AppendLine();

        sb.AppendLine("\tpublic void AddData(TableData data)");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tdatas.Add(data);");
        sb.AppendLine("\t}");

        sb.AppendLine("}");
        sb.AppendLine();


        string textsaver = $"Assets/{tableName}.cs";

        if (File.Exists(textsaver))
        {
            File.Delete(textsaver);
        }

        File.AppendAllText(textsaver, sb.ToString());
    }


    public static void MakeScriptableObject()
    {
        try
        {
            string[] guids = AssetDatabase.FindAssets("", new string[] { CSVFolderPath });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

                if (asset == null || !path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    continue;

                string name = path.Substring(path.LastIndexOf('/') + 1);
                name = name.Substring(0, name.IndexOf('.'));

                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>($"{ScriptableFolderPath}/{name}.asset") != null)
                    AssetDatabase.DeleteAsset($"{ScriptableFolderPath}/{name}.asset");


                Type tableType = Type.GetType(name);

                ScriptableObject scriptableObj = ScriptableObject.CreateInstance(tableType);
                AssetDatabase.CreateAsset(scriptableObj, $"{ScriptableFolderPath}/{name}.asset");

                List<Dictionary<string, object>> tableDataList = TableCSVReader.Read(asset);

                Type innerTableData = tableType.GetNestedType("TableData");

                for (int i = 0; i < tableDataList.Count; i++)
                {
                    object tableDataInstance = Activator.CreateInstance(innerTableData);

                    foreach (string key in tableDataList[i].Keys)
                    {
                        FieldInfo fieldInfo = innerTableData.GetField(key);
                        fieldInfo.SetValue(tableDataInstance, tableDataList[i][key]);
                    }

                    MethodInfo methodInfo = tableType.GetMethod("AddData");
                    methodInfo.Invoke(scriptableObj, new object[] { tableDataInstance });
                }
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(TableMaker)} : {e.Message}" );
        }        
    }
}


#if UNITY_EDITOR

public class TableMakerWindow : EditorWindow
{
    [MenuItem("Custom/TableMakerWindow")]
    public static void Init()
    {
        TableMakerWindow window = (TableMakerWindow)EditorWindow.GetWindow(typeof(TableMakerWindow));
        window.minSize = new Vector2(500, 300);
        window.Show();
    }


    public void OnGUI()
    {
        GUILayout.Label("Path Settings", EditorStyles.boldLabel);

        TableMaker.CSVFolderPath = EditorGUILayout.TextField("CSV Folder Path", TableMaker.CSVFolderPath);
        TableMaker.ScriptableFolderPath = EditorGUILayout.TextField("Scriptable Folder Path", TableMaker.ScriptableFolderPath);

        EditorGUILayout.Space(20);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Make Table Script"))
        {
            TableMaker.MakeTableScript();
        }
        else if (GUILayout.Button("Make Scriptable Object"))
        {
            TableMaker.MakeScriptableObject();
        }

        GUILayout.EndHorizontal();
    }
}

#endif
