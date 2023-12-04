using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class ABUpdateManager : MonoBehaviour
{
    private static ABUpdateManager instance;

    //存储远端AB包字典
    public Dictionary<string,ABInfo> RemoteAbInfoDic= new Dictionary<string,ABInfo>();

    /// <summary>
    /// 待下载AB列表
    /// </summary>
    public List<string> downLoadList = new List<string>();

    public int RetryCount = 5;

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
    /// <summary>
    /// 下载AB包至本地
    /// </summary>
    public async void DownLoadABFlie(UnityAction<bool> overAction)
    {
        foreach (string name in RemoteAbInfoDic.Keys)
        {
            //这里要对比是否相同，不同的放入列表
            downLoadList.Add(name);
        }

        string localPath = Application.persistentDataPath + "/" + "HotUpdate" +"/";
        List<string> FinshList = new List<string>();//缓存加载成功的资源
        bool isOver = false;
        int downLoadCurrentCount = 0;
        int downloadCount = downLoadList.Count;
        int reDownLoadCount = RetryCount;
        while (reDownLoadCount > 0 && downLoadList.Count > 0)
        {
            for (int i = 0; i < downLoadList.Count; i++)
            {
                await Task.Run(() =>
                {
                    FtpMgr.Instance.DownLoadFile(downLoadList[i], localPath + downLoadList[i], out isOver);
                });
                if (isOver)
                {
                    print("下载进度" + ++downLoadCurrentCount + "/" + downloadCount);
                    FinshList.Add(downLoadList[i]);
                }
            }
            for (int i = 0; i < FinshList.Count; i++)
            {
                downLoadList.Remove(FinshList[i]);
                print("移除");
            }
            --reDownLoadCount;
        }

        overAction(downLoadList.Count == 0);
    }
    /// <summary>
    /// 下载AB包对比文件
    /// </summary>
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