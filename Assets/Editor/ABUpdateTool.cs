using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ABUpdateTool : EditorWindow
{

    private static int nowIndex;
    private static string[] targetPlatform = new string[3] { "PC", "IOS", "ANDROID" };
    private static string serverIP = "ftp://10.0.18.39/AB/" + targetPlatform[nowIndex] + "/";
    [MenuItem("AB包工具/打开工具栏")]
    public static void OpenWindow()
    {
        //获取一个ABTools 编辑器窗口对象
        ABUpdateTool window = GetWindowWithRect<ABUpdateTool>(new Rect(0, 0, 600, 300));
        window.Show();
    }

    public void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 15), "平台选择");
        nowIndex = GUI.Toolbar(new Rect(10, 30, 250, 20), nowIndex, targetPlatform);
        GUI.Label(new Rect(10, 60, 150, 15), "服务器资源地址");
        serverIP = GUI.TextField(new Rect(10, 80, 300, 20), serverIP);
        //创建对比文件 按钮
        if (GUI.Button(new Rect(10, 110, 150, 30), "创建对比文件"))
        {
            CreateABCompareFile();
        }
        //保存默认资源到StreamingAssets
        if (GUI.Button(new Rect(170, 110, 200, 30), "保存默认资源到StreamingAssets"))
        {
            MoveABToStreamingAssets();
        }
        //上传对比文件和AB包
        if (GUI.Button(new Rect(380, 110, 150, 30), "上传对比文件和AB包"))
        {
            UpLoadAllABFile();
        }
    }

    /// <summary>
    /// 创建对比文件
    /// </summary>
    public void CreateABCompareFile()
    {
        //获取文件夹信息
        DirectoryInfo directoryInfo = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + targetPlatform[nowIndex] + "/");
        if (directoryInfo != null)
        {
            FileInfo[] fileInfos = directoryInfo.GetFiles();

            if (fileInfos.Length == 0)
            {
                Debug.LogWarning("此文件中没有字符");
                return;
            }
            string abCompareInfo = "";

            foreach (FileInfo Info in fileInfos)
            {
                if (Info.Extension == "")//没有后缀的才是AB包
                {
                    abCompareInfo += Info.Name + " " + Info.Length + " " + GetMD5(Info.FullName);
                    abCompareInfo += '|';
                }
            }
            abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1);

            File.WriteAllText(Application.dataPath + "/ArtRes/AB/" + targetPlatform[nowIndex] + "/ABCompareInfo.txt", abCompareInfo);

            //AssetDatabase.Refresh();

            Debug.Log("对比文件生成成功");
        }

    }
    /// <summary>
    /// 获取资源MD5码
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string GetMD5(string path)
    {
        using (FileStream fileStream = new FileStream(path, FileMode.Open))
        {
            MD5 md5Code = new MD5CryptoServiceProvider();

            byte[] md5Ifo = md5Code.ComputeHash(fileStream);

            fileStream.Close();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < md5Ifo.Length; i++)
            {
                stringBuilder.Append(md5Ifo[i].ToString("X2"));//转换为16进制
            }
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// 将资源移动至StreamingAssets
    /// </summary>
    private void MoveABToStreamingAssets()
    {

        Object[] selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        if (selectedAsset.Length == 0)
            return;

        string abCompareInfo = "";

        foreach (Object asset in selectedAsset)
        {

            string assetPath = AssetDatabase.GetAssetPath(asset);

            string fileName = assetPath.Substring(assetPath.LastIndexOf('/'));


            if (fileName.IndexOf('.') != -1)
                continue;


            AssetDatabase.CopyAsset(assetPath, "Assets/StreamingAssets" + fileName);

            FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + fileName);

            abCompareInfo += fileInfo.Name + " " + fileInfo.Length + " " + CreateABCompare.GetMD5(fileInfo.FullName);

            abCompareInfo += "|";
        }

        abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1);

        File.WriteAllText(Application.streamingAssetsPath + "/ABCompareInfo.txt", abCompareInfo);

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 上传AB包以及对比文件至远端
    /// </summary>
    private static void UpLoadAllABFile()
    {
        //获取文件夹信息
        DirectoryInfo directoryInfo = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + targetPlatform[nowIndex] + "/");
        if (directoryInfo != null)
        {
            FileInfo[] fileInfos = directoryInfo.GetFiles();

            if (fileInfos.Length == 0)
            {
                Debug.LogWarning("此文件中没有字符");
                return;
            }

            List<string> filelist = new List<string>();
            //清空文件
            if (FtpMgr.Instance.GetFileList() != null)
            {
                filelist = FtpMgr.Instance.GetFileList();
            }
            for (int i = 0; i < filelist.Count; i++)
            {
                FtpMgr.Instance.DeleteFile(filelist[i]);
            }

            foreach (FileInfo Info in fileInfos)
            {
                if (Info.Extension == "" || Info.Extension == ".txt")//没有后缀的才是AB包 或者是对比文件
                {
                    FtpMgr.Instance.UpLoadFile(Info.Name, Info.FullName);
                }
            }
        }
    }
}
