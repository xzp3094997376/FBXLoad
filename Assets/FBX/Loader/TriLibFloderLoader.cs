using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using TriLibCore;
using TriLibCore.Extensions;

namespace AVRModelViewer
{

    /// <summary>
    /// 加载文件夹下所有fbx模型文件
    /// </summary>
    public class TriLibFloderLoader : ModelLoader
    {
        /// <summary>
        /// obj信息
        /// </summary>
        public class LoadSucInfo
        {
            public GameObject loadObj;
            public AnimationClip[] objAnimationClips;
        }

        //callback action
        private Action<LoadSucInfo[]> onLoaderCallback;
        //obj信息
        private LoadSucInfo[] loadSucInfos;
        private string[] loadFiles;
        private int loadIndex = 0;
        private float lastTime;


        public TriLibFloderLoader(Action<LoadSucInfo[]> _loaded)
        {
            onLoaderCallback = _loaded;
        }

        /// <summary>
        /// 本地同步加载
        /// </summary>
        /// <param name="_floder">文件夹</param>
        public override void LoadResSync(string _floder)
        {
            isLoading = true;

            lastTime = Time.realtimeSinceStartup;
            loadIndex = 0;

            loadFiles = Directory.GetFiles(_floder);
            loadSucInfos = new LoadSucInfo[loadFiles.Length];
            if (loadFiles.Length > loadIndex)
            {
                StartLoadModel(loadFiles[loadIndex]);
            }

        }

        void StartLoadModel(string modelFile)
        {
            Debug.LogFormat("<color=#0000ff>{0}{1}</color>", "开始加载:", modelFile);
            var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            assetLoaderOptions.AddSecondAlphaMaterial = false;
            AssetLoader.LoadModelFromFile(modelFile, OnAssetLoaded, OnMaterialsLoad, null, OnError, null, assetLoaderOptions);
        }


        void OnAssetLoaded(AssetLoaderContext assetLoaderContext)
        {
            //这里完成后只有白模，先隐藏
            assetLoaderContext.RootGameObject.SetActive(false);
        }
        void OnError(IContextualizedError obj)
        {
            Debug.LogError(obj.GetInnerException());
            loadObj = null;
            OnLoadFinish();
        }

        void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {

            loadSucInfos[loadIndex] = new LoadSucInfo();
            loadSucInfos[loadIndex].loadObj = assetLoaderContext.RootGameObject;

            loadIndex++;

            if (loadIndex <= loadFiles.Length-1)
            {          
                StartLoadModel(loadFiles[loadIndex]);
            }
            else
            {
                OnLoadFinish();
            }
            
        }




        protected override void OnLoadFinish()
        {
            isLoading = false;
            progress = 1f;

            if (loadSucInfos != null && loadSucInfos.Length > 0)
            {
                //设置获取动画clip
                for (int i = 0; i < loadSucInfos.Length; i++)
                {
                    loadSucInfos[i].loadObj.SetActive(true);
                    Animation anim = loadSucInfos[i].loadObj.GetComponent<Animation>();
                    if (anim != null)
                    {
                        List<AnimationClip> resultClip = new List<AnimationClip>();
                        foreach (AnimationState state in anim)
                        {
                            resultClip.Add(state.clip);
                        }
                        loadSucInfos[i].objAnimationClips = resultClip.ToArray();
                        GameObject.DestroyImmediate(anim);
                    }
                }


            }
            onLoaderCallback?.Invoke(loadSucInfos);
        }


        float progressTest = 0.3f;
        public override void OnUpdate()
        {
            //模拟进度，每progressTest秒进度+0.01
            if (isLoading)
            {
                if ((Time.realtimeSinceStartup - lastTime) >= progressTest && progress < 0.9f)
                {
                    progress += 0.01f;
                    lastTime = Time.realtimeSinceStartup;
                }
            }
        }


    }

}