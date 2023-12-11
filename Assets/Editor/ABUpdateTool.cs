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
    [MenuItem("AB������/�򿪹�����")]
    public static void OpenWindow()
    {
        //��ȡһ��ABTools �༭�����ڶ���
        ABUpdateTool window = GetWindowWithRect<ABUpdateTool>(new Rect(0, 0, 600, 300));
        window.Show();
    }

    public void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 15), "ƽ̨ѡ��");
        nowIndex = GUI.Toolbar(new Rect(10, 30, 250, 20), nowIndex, targetPlatform);
        GUI.Label(new Rect(10, 60, 150, 15), "��������Դ��ַ");
        serverIP = GUI.TextField(new Rect(10, 80, 300, 20), serverIP);
        //�����Ա��ļ� ��ť
        if (GUI.Button(new Rect(10, 110, 150, 30), "�����Ա��ļ�"))
        {
            CreateABCompareFile();
        }
        //����Ĭ����Դ��StreamingAssets
        if (GUI.Button(new Rect(170, 110, 200, 30), "����Ĭ����Դ��StreamingAssets"))
        {
            MoveABToStreamingAssets();
        }
        //�ϴ��Ա��ļ���AB��
        if (GUI.Button(new Rect(380, 110, 150, 30), "�ϴ��Ա��ļ���AB��"))
        {
            UpLoadAllABFile();
        }
    }

    /// <summary>
    /// �����Ա��ļ�
    /// </summary>
    public void CreateABCompareFile()
    {
        //��ȡ�ļ�����Ϣ
        DirectoryInfo directoryInfo = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + targetPlatform[nowIndex] + "/");
        if (directoryInfo != null)
        {
            FileInfo[] fileInfos = directoryInfo.GetFiles();

            if (fileInfos.Length == 0)
            {
                Debug.LogWarning("���ļ���û���ַ�");
                return;
            }
            string abCompareInfo = "";

            foreach (FileInfo Info in fileInfos)
            {
                if (Info.Extension == "")//û�к�׺�Ĳ���AB��
                {
                    abCompareInfo += Info.Name + " " + Info.Length + " " + GetMD5(Info.FullName);
                    abCompareInfo += '|';
                }
            }
            abCompareInfo = abCompareInfo.Substring(0, abCompareInfo.Length - 1);

            File.WriteAllText(Application.dataPath + "/ArtRes/AB/" + targetPlatform[nowIndex] + "/ABCompareInfo.txt", abCompareInfo);

            //AssetDatabase.Refresh();

            Debug.Log("�Ա��ļ����ɳɹ�");
        }

    }
    /// <summary>
    /// ��ȡ��ԴMD5��
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
                stringBuilder.Append(md5Ifo[i].ToString("X2"));//ת��Ϊ16����
            }
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// ����Դ�ƶ���StreamingAssets
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
    /// �ϴ�AB���Լ��Ա��ļ���Զ��
    /// </summary>
    private static void UpLoadAllABFile()
    {
        //��ȡ�ļ�����Ϣ
        DirectoryInfo directoryInfo = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + targetPlatform[nowIndex] + "/");
        if (directoryInfo != null)
        {
            FileInfo[] fileInfos = directoryInfo.GetFiles();

            if (fileInfos.Length == 0)
            {
                Debug.LogWarning("���ļ���û���ַ�");
                return;
            }

            List<string> filelist = new List<string>();
            //����ļ�
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
                if (Info.Extension == "" || Info.Extension == ".txt")//û�к�׺�Ĳ���AB�� �����ǶԱ��ļ�
                {
                    FtpMgr.Instance.UpLoadFile(Info.Name, Info.FullName);
                }
            }
        }
    }
}
