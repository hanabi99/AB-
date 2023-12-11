using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CreateABCompare
{
    //[MenuItem("AB包工具/创建对比文件")]
    public static void CreateABCompareFile()
    {
        //获取文件夹信息
        DirectoryInfo directoryInfo = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/PC/");
        if (directoryInfo != null)
        {
            FileInfo[] fileInfos = directoryInfo.GetFiles();

            if(fileInfos.Length == 0)
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

            File.WriteAllText(Application.dataPath + "/ArtRes/AB/PC/ABCompareInfo.txt", abCompareInfo);

            //AssetDatabase.Refresh();

            Debug.Log("对比文件生成成功");
        }

    }

    public static string GetMD5(string path)
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
}
