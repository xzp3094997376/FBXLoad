using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AVRModelViewer
{

    public class ABLoader : ResLoader
    {
        UnityWebRequest req;
        protected bool isSuc = false;

        protected virtual void DownLoadAB(string url, System.Action<Object[]> onLoad)
        {
            AVRViewer.instance.StartCoroutine(IELoadAB(url, onLoad));
        }
        protected IEnumerator IELoadAB(string url, System.Action<Object[]> onLoad)
        {
            isSuc = false;
            using (req = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                yield return req.SendWebRequest();
                if (req.isNetworkError || req.isHttpError)
                {
                    Debug.LogError(req.error);
                    onLoad?.Invoke(null);
                }
                else
                {
                    AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(req);
                    if (assetBundle != null)
                    {
                        string[] names = assetBundle.GetAllAssetNames();
                        Object[] loadObjs = new Object[names.Length];
                        for (int i = 0; i < names.Length; i++)
                        {
                            loadObjs[i] = assetBundle.LoadAsset(names[i]);
                        }
                        isSuc = true;
                        onLoad?.Invoke(loadObjs);
                    }
                    assetBundle.Unload(false);
                }
            }
        }

        public override void OnUpdate()
        {
            if (req != null)
            {
                progress = req.downloadProgress;
            }
        }

    }



    public class ABModelLoader : ABLoader
    {

        private System.Action<GameObject> onInsObj;

        public ABModelLoader(System.Action<GameObject> _loaded)
        {
            onInsObj = _loaded;
        }


        public override void LoadResSync(string _path)
        {
            DownLoadAB(_path, OnLoadFinish);
        }

        void OnLoadFinish(Object[] _objs)
        {
            if (_objs != null && _objs.Length > 0)
            {
                GameObject mgo = GameObject.Instantiate(((GameObject)_objs[0]), Vector3.zero, Quaternion.identity);
                onInsObj?.Invoke(mgo);
            }
            else
            {
                onInsObj?.Invoke(null);
            }
        }


    }



}
