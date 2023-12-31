﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ABUpdateManager : MonoBehaviour
{
    private static ABUpdateManager instance;

    //存储远端AB包字典
    public Dictionary<string, ABInfo> RemoteAbInfoDic = new Dictionary<string, ABInfo>();

    //存储本地AB包字典
    public Dictionary<string, ABInfo> LocalAbInfoDic = new Dictionary<string, ABInfo>();

    /// <summary>
    /// 待下载AB列表
    /// </summary>
    public List<string> downLoadList = new List<string>();


    public static string localPath =Application.persistentDataPath + "/" + "HotUpdate" + "/";

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
    /// 检查更新
    /// </summary>
    /// <param name="overCallBack"></param>
    /// <param name="updateInfoCallBack"></param>
    public void CheckUpdate(UnityAction<bool> overCallBack, UnityAction<string> updateInfoCallBack)
    {
        //为了避免上次出现问题 而清除缓存
        RemoteAbInfoDic.Clear();
        LocalAbInfoDic.Clear();
        //1.加载远端资源对比文件
        DownLoadABCompareFile((isOver) =>
        {
            updateInfoCallBack("开始更新资源");
            if (isOver)
            {
                updateInfoCallBack("对比文件下载结束");
                string remoteInfo = File.ReadAllText(localPath + "ABCompareInfo_TMP.txt");
                updateInfoCallBack("解析远端对比文件");
                ReadABCompareFileInfo(remoteInfo, RemoteAbInfoDic);
                updateInfoCallBack("解析远端对比文件完成");

                //2.加载本地资源对比文件
                GetLocalABCompareFileInfo((isOver) =>
                {
                    if (isOver)
                    {
                        updateInfoCallBack("解析本地对比文件完成");
                        //3.对比他们 然后进行AB包下载
                        foreach (var abName in RemoteAbInfoDic.Keys)//本地没有
                        {
                            if (!LocalAbInfoDic.ContainsKey(abName))
                            {
                                downLoadList.Add(abName);
                            }
                            else
                            {
                                if (LocalAbInfoDic[abName].md5 != RemoteAbInfoDic[abName].md5)//本地需要更新
                                {
                                    downLoadList.Add(abName);
                                }
                                //如果一样就不用更新 

                                //不管MD5相不相同 都移除本地的字典信息 剩下没移除的就是 远程没有的 就可以卸载了 等下次热更时会自动更新本地字典
                                LocalAbInfoDic.Remove(abName);
                            }
                        }
                        updateInfoCallBack("对比完成 删除多余资源");
                        foreach (string abName in LocalAbInfoDic.Keys)
                        {
                            //如果可读写文件夹中有内容 我们就删除它 
                            //默认资源中的 信息 我们没办法删除
                            if (File.Exists((localPath + abName)))
                            {
                                File.Delete(localPath + abName);
                            }
                        }
                        updateInfoCallBack("下载和更新AB包文件");
                        //下载待更新列表中的所有AB包
                        //下载
                        DownLoadABFlie((isOver) =>
                        {
                            if (isOver)
                            {
                                //下载完所有AB包文件后
                                //把本地的AB包对比文件 更新为最新
                                //把之前读取出来的 远端对比文件信息 存储到 本地 
                                updateInfoCallBack("更新本地AB包对比文件为最新");
                                File.WriteAllText(localPath + "ABCompareInfo.txt", remoteInfo);
                            }
                            overCallBack(isOver);
                        });
                    }
                    else
                        overCallBack(false);
                });
            }
            else
            {
                overCallBack(false);
            }
        });
    }



    /// <summary>
    /// 从远端下载下载AB包至本地
    /// </summary>
    public async void DownLoadABFlie(UnityAction<bool> overAction)
    {
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
    /// 从远端下载AB包对比文件
    /// </summary>
    public async void DownLoadABCompareFile(UnityAction<bool> overcallback)
    {
        print(localPath);
        bool isOver = false;
        int reDownLoadCount = RetryCount;
        while (reDownLoadCount > 0 && !isOver)
        {
            await Task.Run(() => { FtpMgr.Instance.DownLoadFile("ABCompareInfo.txt", localPath + "ABCompareInfo_TMP.txt", out isOver); });
            --reDownLoadCount;
        }
        overcallback.Invoke(isOver);
    }

    /// <summary>
    /// 读取远端下载下来的对比文件信息
    /// </summary>
    public void ReadABCompareFileInfo(string info, Dictionary<string, ABInfo> ABInfo)
    {
        string[] strs = info.Split('|');
        string[] infos = null;
        for (int i = 0; i < strs.Length; i++)
        {
            infos = strs[i].Split(' ');

            ABInfo.Add(infos[0], new ABInfo(infos[0], infos[1], infos[2]));
        }

        print("读取对比文件结束");
    }


    /// <summary>
    /// 获取本地AB包对比文件
    /// </summary>
    public void GetLocalABCompareFileInfo(UnityAction<bool> overCallBack = null)
    {
        //如果可读可写文件夹中 存在对比文件 说明之前我们已经下载更新过了
        if (File.Exists(localPath + "ABCompareInfo.txt"))
        {
            StartCoroutine(GetLocalABCompareFileInfo(localPath + "ABCompareInfo.txt", overCallBack));
        }
        //只有当可读可写中没有对比文件时  才会来加载默认资源（第一次进游戏时才会发生）
        else if (File.Exists(Application.streamingAssetsPath + "/ABCompareInfo.txt"))
        {
            string path =
#if UNITY_ANDROID
"Application.streamingAssetsPath"
#else
 "file:///" + Application.streamingAssetsPath;
#endif
            StartCoroutine(GetLocalABCompareFileInfo(path + "/ABCompareInfo.txt", overCallBack));
        }
        //如果两个都不进 证明第一次并且没有默认资源 
        else
            overCallBack(true);
     }
    /// <summary>
    ///  加载本地对比文件信息 并且解析存入字典
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private IEnumerator GetLocalABCompareFileInfo(string filePath, UnityAction<bool> overCallBack)
    {
        //通过 UnityWebRequest 去加载本地文件
        print(filePath);
        UnityWebRequest req = UnityWebRequest.Get(filePath);
        yield return req.SendWebRequest();
        print(req.downloadHandler.text);
        //获取文件成功 继续往下执行
        if (req.result == UnityWebRequest.Result.Success)
        {
            ReadABCompareFileInfo(req.downloadHandler.text, LocalAbInfoDic);
            overCallBack(true);
        }
        else
            overCallBack(false);
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
        public ABInfo(string name, string size, string md5)
        {
            this.name = name;
            this.size = long.Parse(size);
            this.md5 = md5;
        }
    }
}


