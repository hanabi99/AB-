using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ABManager :Singleton<ABManager>
{
    //AB包管理器 目的是
    //让外部更方便的进行资源加载
    
    //主包 
    private AssetBundle mainAB = null;
    private AssetBundleManifest manifest = null;

    //AB包不能够重复加载 重复加载会报错
    //用字典来储存 加载过的AB包
   public Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    private string PathURL
    {
        get { return Application.streamingAssetsPath + "/"; }
    }

    private string MainABName
    {
        get
        {
#if   UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android"
#else 
            return "PC";
#endif
        }
    }


   public void LoadAB(string abname)
    {
        //加载AB包

             
        if (mainAB == null)
        {
            mainAB = AssetBundle.LoadFromFile(PathURL + MainABName);
            manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        AssetBundle ab = null;
        string[] strs = manifest.GetAllDependencies(abname);
        for (int i = 0; i < strs.Length; i++)
        {
            if (!abDic.ContainsKey(strs[i]))//先判断在加载
            {
                ab = AssetBundle.LoadFromFile(PathURL + strs[i]);
                abDic.Add(strs[i], ab);
            }
        }
        //获取依赖包相关信息
        //加载主包


        if (!abDic.ContainsKey(abname))
        {
            ab = AssetBundle.LoadFromFile(PathURL + abname);
            abDic.Add(abname, ab);
        }

      
        //加载主包关键配置文件 获取依赖包
        //加载依赖包
        //加载目标包
    }


    //同步加载
    public Object LoadRes(string abname,string resName)
    {

        LoadAB(abname);

        Object obj = abDic[abname].LoadAsset(resName);
        if (obj is GameObject)
            return Instantiate(obj);
        else
            return obj;
       
    }
    //同步加载 typeof
    public Object LoadRes(string abname, string resName,System.Type type)
    {
        LoadAB(abname);

        Object obj = abDic[abname].LoadAsset(resName,type);
        if (obj is GameObject)
            return Instantiate(obj);
        else
            return obj;

    }

    //同步加载 泛型
    public T LoadRes<T>(string abname, string resName) where T : Object
    {
        LoadAB(abname);

       T obj = abDic[abname].LoadAsset<T>(resName);
        if (obj is GameObject) 
            return Instantiate(obj);
        else
            return obj;

    }

    // 异步加载方法
    public void LoadResAsync(string abname,string resname,UnityAction<Object> callback)
    {
      StartCoroutine(ReallyLoadResAsync(abname, resname, callback));
    }
    private IEnumerator ReallyLoadResAsync(string abname, string resname, UnityAction<Object> callback)
    {
        LoadAB(abname);
        AssetBundleRequest abr  = abDic[abname].LoadAssetAsync(resname);
        yield return abr;

        if (abr.asset is GameObject)
            callback(Instantiate(abr.asset));
        else
            callback(abr.asset);
    }

    //异步加载重载,type
    public void LoadResAsync(string abname, string resname, UnityAction<Object> callback,System.Type type)
    {
        StartCoroutine(ReallyLoadResAsync(abname, resname, callback,type));
    }
    private IEnumerator ReallyLoadResAsync(string abname, string resname, UnityAction<Object> callback, System.Type type)
    {
        LoadAB(abname);
        AssetBundleRequest abr = abDic[abname].LoadAssetAsync(resname,type);
        yield return abr;

        if (abr.asset is GameObject)
            callback(Instantiate(abr.asset));
        else
            callback(abr.asset);
    }

    //泛型异步加载
    
    public void LoadResAsync<T>(string abname, string resname, UnityAction<T> callback) where T :Object
    {
        StartCoroutine(ReallyLoadResAsync<T>(abname, resname, callback));
    }
    private IEnumerator ReallyLoadResAsync<T>(string abname, string resname, UnityAction<T> callback) where T : Object
    {
        LoadAB(abname);
        AssetBundleRequest abr = abDic[abname].LoadAssetAsync<T>(resname);
        yield return abr;

        if (abr.asset is GameObject)
            callback(Instantiate(abr.asset) as T);
        else
            callback(abr.asset as T);

    }



    //单个包卸载

    public void UnLoad(string abname)
    {
        if (abDic.ContainsKey(abname))
        {
            abDic[abname].Unload(false);
            abDic.Remove(abname);
        }
    }
    public void ClearAb()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        abDic.Clear();
        mainAB = null;
        manifest = null;
    }


}
