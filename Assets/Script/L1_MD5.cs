using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;

public class L1_MD5 : MonoBehaviour
{
    //流程 1.根据文件路径，获取文件信息流
    //2.利用MD5对象根据信息流，计算出MD5
    //3.将字节数组形式的MD5转为16进制

    private void Start()
    {
        print(GetMD5(Application.dataPath + "/ArtRes/AB/PC/lua"));
    }

    private string GetMD5(string filepath)
    {
        using (FileStream file = new FileStream(filepath, FileMode.Open))
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            //计算出MD5
            byte[] md5info = md5.ComputeHash(file);

            file.Close();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < md5info.Length; i++)
            {
                stringBuilder.Append(md5info[i].ToString("X2"));//转换为16进制
            }
            return stringBuilder.ToString();
        }
    }

}
