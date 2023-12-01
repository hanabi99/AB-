using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ABUpdateManager : MonoBehaviour
{
    private static ABUpdateManager instance;

    //存储远端AB包字典
    public Dictionary<string,ABInfo> RemoteAbInfoDic= new Dictionary<string,ABInfo>();

    public static ABUpdateManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.name = typeof(ABUpdateManager).ToString();
                instance = gameObject.AddComponent<ABUpdateManager>();
            }
            return instance;
        }
    }

    public void DownLoadABCompareFile()
    {
        FtpMgr.Instance.DownLoadFile("ABCompareInfo.txt",Application.persistentDataPath + "/HotUpdate/ABCompareInfo.txt");
        string compareInfo = File.ReadAllText(Application.persistentDataPath + "/HotUpdate/ABCompareInfo.txt");
        string[] Info = compareInfo.Split('|');
        string[] InfoItem = null;
        for (int i = 0; i < Info.Length; i++)
        {
            InfoItem = Info[i].Split(' ');
            ABInfo abInfo = new ABInfo(InfoItem[0], InfoItem[1], InfoItem[2]);
            RemoteAbInfoDic.Add(InfoItem[0],abInfo);
        }

        foreach (ABInfo item in RemoteAbInfoDic.Values)
        {
            Debug.Log(item.name);
        }


    }

    private void OnDestroy()
    {
        instance = null;
    }

    public class ABInfo
    {
        public string name;
        public long size;
        public string md5;
        public ABInfo(string name,string size,string md5)
        {
            this.name = name;
            this.size = long.Parse(size);
            this.md5 = md5;
        }
    }

}