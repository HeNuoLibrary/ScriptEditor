using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using System.Linq;

/// <summary>
/// 自定义脚本模板助手
/// </summary>
public static class ScriptsTemplateHelper
{
    // 获取文件夹路径  单个
    public static Dictionary<string, string> GetSelectedDirPathSingle(string newScriptName)
    {
        // Object[] GetFiltered(Type type, SelectionMode mode)
        // type ---> 只会检索此类型的对象
        // mode ---> SelectionMode.Assets 仅返回 Asset 目录中的资产对象
        var selections = Selection.GetFiltered(typeof(Object), SelectionMode.Assets); // Object ---> UnityEngine.Object

        foreach (var obj in selections)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (Directory.Exists(path))
            {
                return new Dictionary<string, string>()
                {
                    { "name", newScriptName },
                    { "path", AssetDatabase.GetAssetPath(obj) }
                };
            }
        }

        return new Dictionary<string, string>()
        {
            { "name", newScriptName },
            { "path", Application.dataPath }
        };
    }

    // 创建文件  单个
    public static void CreateScriptFileSingle(Dictionary<string, string> path, string templatePath)
    {
        const int instanceId = 0;
        var endAction = ScriptableObject.CreateInstance<NameByEnterOrUnfocus>();
        var pathName = $"{path["path"]}/{path["name"]}.cs";
        /*
            *参数1：instanceId 已编辑资源的实例 ID。
            *参数2：endAction 监听编辑名称的类的实例化
            *参数3：pathName 创建的文件路径（包括文件名）
            *参数4：icon 图标
            *参数5：resourceFile 模板路径
            *
             endAction 直接使用 new NameByEnterOrUnfocus() 出现以下警告：
             NameByEnterOrUnfocus must be instantiated using the ScriptableObject.CreateInstance method instead of new NameByEnterOrUnfocus.
             必须使用ScriptableObject实例化NameByEnterOrUnfocus。CreateInstance方法，而不是新的NameByEnterOrUnfocus。
         */
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(instanceId, endAction, pathName, null, templatePath);
    }


    // 获取文件夹路径  多个
    public static List<Dictionary<string, string>> GetSelectedDirPath(string newScriptName)
    {
        // Object[] GetFiltered(Type type, SelectionMode mode)
        // type ---> 只会检索此类型的对象
        // mode ---> SelectionMode.Assets 仅返回 Asset 目录中的资产对象
        var selections = Selection.GetFiltered(typeof(Object), SelectionMode.Assets); // Object ---> UnityEngine.Object

        // 路径、文件名称 集合
        return (from obj in selections
                let path = AssetDatabase.GetAssetPath(obj)
                where Directory.Exists(path)
                select new Dictionary<string, string>
        {
            { "name", newScriptName },
            { "path", AssetDatabase.GetAssetPath(obj) }
        }).ToList();
    }

    // 创建文件  多个
    public static void CreateScriptFile(List<Dictionary<string, string>> paths, string templatePath)
    {
        foreach (var pathInfo in paths.Where(pathInfo => pathInfo["name"] != ""))
        {
            const int instanceId = 0;
            var endAction = ScriptableObject.CreateInstance<NameByEnterOrUnfocus>();
            var pathName = $"{pathInfo["path"]}/{pathInfo["name"]}.cs";
            /*
                *参数1：instanceId 已编辑资源的实例 ID。
                *参数2：endAction 监听编辑名称的类的实例化
                *参数3：pathName 创建的文件路径（包括文件名）
                *参数4：icon 图标
                *参数5：resourceFile 模板路径
                *
                 endAction 直接使用 new NameByEnterOrUnfocus() 出现以下警告：
                 NameByEnterOrUnfocus must be instantiated using the ScriptableObject.CreateInstance method instead of new NameByEnterOrUnfocus.
                 必须使用ScriptableObject实例化NameByEnterOrUnfocus。CreateInstance方法，而不是新的NameByEnterOrUnfocus。
             */
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(instanceId, endAction, pathName, null, templatePath);
        }
    }
}
internal class NameByEnterOrUnfocus : EndNameEditAction
{
    /// <summary>
    /// 当编辑器接收一个文件编辑好名称的时候调用
    /// 当用户通过按下 Enter 键或失去键盘输入焦点接受编辑后的名称时，Unity 调用此函数
    /// </summary>
    /// <param name="instanceId">已编辑资源的实例 ID。</param>
    /// <param name="pathName">资源的路径。</param>
    /// <param name="resourceFile">传递给ProjectWindowUtil.StartNameEditingIfProjectWindowExists的资源文件字符串参数</param>
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        var obj = CreateScript(pathName, resourceFile);
        // 创建并展示
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }

    private static Object CreateScript(string pathName, string resourceFile)
    {
        // 创建时间
        string createTime = string.Format("CreateTime: {0}/{1}/{2}   {3}:{4}:{5}",
            System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day,
            System.DateTime.Now.Hour, System.DateTime.Now.Minute, System.DateTime.Now.Second);

        // 读取模板文件内容
        var streamReader = new StreamReader(resourceFile);
        var templateText = streamReader.ReadToEnd();
        streamReader.Close();

        // 获取创建的脚本文件名称
        var fileName = Path.GetFileNameWithoutExtension(pathName);
        // 正则替换文本内自定义的变量
        var scriptText = Regex.Replace(templateText, "#CREATETIME#", createTime);
        scriptText = Regex.Replace(scriptText, "#SCRIPTNAME#", fileName);


        // 写入脚本
        var streamWriter = new StreamWriter(pathName);
        streamWriter.Write(scriptText);
        streamWriter.Close();

        // 在路径导入资源
        AssetDatabase.ImportAsset(pathName);
        // 返回给定路径assetPath类型类型的第一个资源对象
        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
    }
}
