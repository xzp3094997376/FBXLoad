using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

namespace AVRModelViewer
{   
    public class AVRViewer : MonoBehaviour
    {
        public static AVRViewer instance;      
        public GameObject CurModelGameObject;

        public GameObject avrRoot;     

        public string resPath;//资源路径

        public string strModelResPath;//FBX路径

        private AnimationDatas[] animationDatas;
        void Awake()
        {
            instance = this;           
            //windowCtrl.SetWinTop();
        }

        // Start is called before the first frame update
        void Start()
        {
            avrRoot = new GameObject("avrRoot");
            //获取StreamingAssets资源文件夹          
            //StartCoroutine(Copy());
            PathSet();
        }

        public string pauseZipPath;
        public string sourcePath;
        void PathSet()
        {
            //解压路径
            pauseZipPath = Path.Combine(Application.persistentDataPath, "AVRViewrChache");
            pauseZipPath = ZipUtility.PathRespace(pauseZipPath);
            if (!Directory.Exists(pauseZipPath))//模型解压路径
            {
                Directory.CreateDirectory(pauseZipPath);
            }
            Debug.Log("解压路径:  " + pauseZipPath);

            //streaming 路径
            sourcePath = Path.Combine(Application.streamingAssetsPath, "FBX");
            sourcePath = Path.Combine(sourcePath, "ttjkz.zip");
            sourcePath = ZipUtility.PathRespace(sourcePath);

            //模型压缩包放置路径
            resPath = Application.persistentDataPath;
            resPath = ZipUtility.PathRespace(resPath);

            Debug.Log("模型压缩包放置路径:  " + resPath);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("模型copy");
                StartCoroutine(Copy());
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("FBX模型加载");
                ModelLoad();
            }
        }

        IEnumerator Copy()
        {          
#if UNITY_IOS
            src = "file://" + src;    
#endif
            UnityWebRequest www = UnityWebRequest.Get(sourcePath);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {              
                Debug.Log(www.downloadHandler.data.Length);

                byte[] bytes = www.downloadHandler.data;
                string _path = Path.Combine(resPath, "1.zip");
                _path = ZipUtility.PathRespace(_path);
                File.WriteAllBytes(_path, bytes);
                yield return null;

            }
            www.Dispose();
        }
        
        void ModelLoad()
        {                       
            //string modelPath = ZipUtility.FindModelFileFullName(resPath, out string format);                                 
            //Debug.Log("---测试模型:" + modelPath);
            string[] foundFilePathlist = ZipUtility.FindModelFileFullNameList(resPath, out string fomat);
            for (int i = 0; i < ZipUtility.GetListFullName.Count; i++)
            {
                string modelPath = ZipUtility.GetListFullName[i];
                Debug.Log("---测试模型:" + modelPath);
                MiniLoadModel(modelPath);
            }          
        }


        //ppt编辑器传参读取数据              
        ResLoader modelLoader;
        void MiniLoadModel(string _filePath)
        {                       
            string strCheckFile = _filePath.ToLower();
            _filePath = ZipUtility.PathRespace(_filePath);
            Debug.Log("---开始加载:" + _filePath);
            strModelResPath = strCheckFile;
            if (strCheckFile.EndsWith(".fbx"))
            {
                Debug.Log("---fbx");
                //ps:使用TriLib插件加载
                modelLoader = new TriLibLoader(OnLoadFinish);
            }          
            if (modelLoader != null)
            {               
                modelLoader.LoadResSync(_filePath);
            }
            else
            {
                Debug.LogError("---ResLoader is null! FilePath:" + _filePath);
            }
        }


        //多模型加载
        void OnLoadsFinish(TriLibFloderLoader.LoadSucInfo[] _loadSucInfos)
        {
            if (_loadSucInfos.Length > 0)
            {
                //具体的显示方式未定...
                OnLoadFinish(_loadSucInfos[0].loadObj, _loadSucInfos[0].objAnimationClips);
            }
        }



        //fbx gltf glb 模型加载完成
        void OnLoadFinish(GameObject _go, AnimationClip[] _clips)
        {          

            List<AnimationClip> getClips = new List<AnimationClip>();
            List<AnimationDatas> animationDataList = new List<AnimationDatas>();

            //fbx的模型默认全部动画
            if (strModelResPath.Contains(".fbx"))
            {
                if (_clips != null && _clips.Length > 0)
                {
                    for (int i = 0; i < _clips.Length; i++)
                    {
                        getClips.Add(_clips[i]);
                        AnimationDatas adata = new AnimationDatas();
                        adata.showName = _clips[i].name;
                        adata.clipName = _clips[i].name;
                        adata.animationClip = _clips[i];
                        animationDataList.Add(adata);
                    }
                }
            }           

            if (getClips.Count > 0)
            {
                //trilib插件自带有Animation组件
                if (!strModelResPath.Contains(".fbx"))
                {
                    Animation anim = _go.GetComponent<Animation>();
                    if (anim == null)
                    {
                        anim = _go.AddComponent<Animation>();
                    }
                    for (int i = 0; i < getClips.Count; i++)
                    {
                        getClips[i].legacy = true;
                        anim.AddClip(getClips[i], getClips[i].name);
                    }
                    anim.clip = getClips[0];
                    anim.playAutomatically = false;
                }

                animationDatas = animationDataList.ToArray();
            }

            InitLoadGameObj(_go);
        }

        AnimationClip FindClip(AnimationClip[] _clips, string _name)
        {
            for (int i = 0; i < _clips.Length; i++)
            {
                if (_clips[i] != null && (_clips[i].name.CompareTo(_name) == 0))
                {
                    return _clips[i];
                }
            }
            return null;
        }        
        /// <summary>
        /// 初始化加载成功的Obj
        /// </summary>
        void InitLoadGameObj(GameObject _mgo)
        {
            GameObject mgo = _mgo;
            if (mgo == null)
            {
                Debug.LogError("---加载模型失败了!");
                return;
            }


            // TODO:设置模型位置
            mgo.transform.SetParent(avrRoot.transform);
            mgo.transform.localPosition = Vector3.zero;
            //mgo.gameObject.AddComponent<ModelControl>();
            //mgo.AddComponent<TestDny>();
        }        
    }

}