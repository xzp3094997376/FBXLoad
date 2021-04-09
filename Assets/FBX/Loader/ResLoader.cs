using System;
using UnityEngine;



namespace AVRModelViewer
{

    //public enum LoadResType
    //{
    //    FBX,
    //    GLTF,
    //    UNITY3D
    //}



    public class ResLoader
    {
        public float progress = 0;
        public bool isLoading = false;

        public virtual void LoadResSync(string _path)
        { }
        public virtual void LoadResASync(string _path)
        { }
        public virtual void OnUpdate()
        { }
    }


    public class ModelLoader : ResLoader
    {
        protected GameObject loadObj;
        protected AnimationClip[] clips;
        protected Action<GameObject, AnimationClip[]> onLoaded;

        protected virtual void OnLoadFinish()
        {
            onLoaded?.Invoke(loadObj, clips);
        }


    }


}
