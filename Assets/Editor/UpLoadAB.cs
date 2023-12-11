using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class UpLoadAB : MonoBehaviour
{
    //[MenuItem("AB包工具/上传AB包和对比文件")]
    private static void UpLoadAllABFile()
    {
        //获取文件夹信息
        DirectoryInfo directoryInfo = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/PC/");
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

