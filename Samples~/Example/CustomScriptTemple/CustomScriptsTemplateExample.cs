using UnityEditor;
using System.IO;


public class CustomScriptsTemplateExample
{
    public const string ScriptTemplatePath = "Assets/ScriptTemplateExample.txt";

    [MenuItem("Assets/Create/C# CreateCustomScript1", false, 72)]
    private static void CreateCustomScript1()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(ScriptTemplatePath);
        if (asset == null)
        {
            EditorUtility.DisplayDialog("提示", $"模板文件丢失 : {ScriptTemplatePath}", "确定");
        }
        else
        {
            var paths = ScriptsTemplateHelper.GetSelectedDirPathSingle("NewScriptTemplateExample");
            ScriptsTemplateHelper.CreateScriptFileSingle(paths, ScriptTemplatePath);
        }
    }


    [MenuItem("Assets/Create/C# CreateCustomScript2", false, 71)]
    private static void CreateCustomScript2()
    {
        var paths = ScriptsTemplateHelper.GetSelectedDirPath("NewScriptTemplateExample");
        if (paths.Count != 0) ScriptsTemplateHelper.CreateScriptFile(paths, ScriptTemplatePath);
        else EditorUtility.DisplayDialog("提示", "请选择文件夹", "确定");
    }
}

