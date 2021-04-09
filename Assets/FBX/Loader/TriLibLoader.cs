using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using TriLibCore;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
namespace AVRModelViewer
{
    
    public class TriLibLoader : ModelLoader
    {
        
        //材质球贴图命名规则
        public enum LoadedTextureType
        {
            Nome,
            _Albedo,
            _AlbedoTransparency,
            _MetallicSmoothness,
            _Metallic,
            _Normal,
            _ao
        }



        private bool Async;

        private string modelPath;

        public TriLibLoader(Action<GameObject, AnimationClip[]> _loaded)
        {
            onLoaded = _loaded;
        }

        /// <summary>
        /// 本地同步加载
        /// </summary>
        /// <param name="_path"></param>
        public override void LoadResSync(string _path)
        {
            isLoading = true;
            modelPath = _path;

            Async = false;

            var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            //assetLoaderOptions.AddSecondAlphaMaterial = false;
            AssetLoader.LoadModelFromFile(modelPath, OnAssetLoaded, OnMaterialsLoad, OnSetProgress, OnError, null, assetLoaderOptions);
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="_path"></param>
        public override void LoadResASync(string _path)
        {
            Debug.Log("---LoadResASync还没实现");
            Async = true;
        }


        void OnSetProgress(AssetLoaderContext assetLoaderContext, float _progress)
        {
            progress = _progress;
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
            string matPath = System.IO.Path.GetDirectoryName(modelPath);
            string result = String.Empty;
            matPath = ZipUtility.PathRespace(matPath);
            int _index= matPath.LastIndexOf(@"/");
            Debug.Log(_index +"   "+ matPath);
            result = matPath.Substring(matPath.LastIndexOf(@"/"));


            Debug.Log(result);
            string matFloderPath = matPath.TrimEnd(result.ToCharArray());
            foreach (Material mat in assetLoaderContext.LoadedMaterials.Values)
            {
                LoadSprite(mat, matFloderPath);
            }
            loadObj = assetLoaderContext.RootGameObject;
            OnLoadFinish();
        }

        void LoadSprite(Material mat, string floderPath)
        {
            LoadLocalImage(mat, floderPath, LoadedTextureType._Albedo);
            LoadLocalImage(mat, floderPath, LoadedTextureType._AlbedoTransparency);
            LoadLocalImage(mat, floderPath, LoadedTextureType._Metallic);
            LoadLocalImage(mat, floderPath, LoadedTextureType._MetallicSmoothness);
            LoadLocalImage(mat, floderPath, LoadedTextureType._Normal);
            LoadLocalImage(mat, floderPath, LoadedTextureType._ao);
        }
        private static byte[] GetImageByte(string imagePath)
        {
            FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();
            return buffer;
        }
        
        private void LoadLocalImage(Material mat, string floderPath, LoadedTextureType texType)
        {
            //递归查找贴图
            string texturePath = ZipUtility.FindFileFullName(floderPath, mat.name + texType.ToString());
            if (string.IsNullOrEmpty(texturePath))
            {
                return;
            }
            
           // Debug.LogFormat("<color=#ff0000>找到贴图:{0}</color>", texturePath);
            FileInfo textureFI = new FileInfo(texturePath);
            byte[] texBytes;
            //System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(16, 16);
            //if (textureFI.Extension.Equals(".tga", StringComparison.OrdinalIgnoreCase))
            //{
            //    //TGA tgaMap = new TGA(texturePath);
            //    TGA2PNG.OpenTGAFile(GetImageByte(texturePath), ref bitmap);
            //    //texturePath = $"{textureFI.Directory}\\{textureFI.Name}.png";
            //    MemoryStream ms = new MemoryStream();
            //    //tgaMap.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //    texBytes = ms.ToArray();
            //    //ms.Dispose();
            //}
            //else
            {
                texBytes = File.ReadAllBytes(texturePath);
            }

            //UnityWebRequest request = UnityWebRequestTexture.GetTexture($"file:///{texturePath}");
            //await request.SendWebRequest();
            //Texture2D tex2D = DownloadHandlerTexture.GetContent(request);

            Texture2D tex2D = new Texture2D(16, 16);
            tex2D.LoadImage(texBytes);
            tex2D.Compress(false);
            //Debug.Log("材质球 - " + mat.name);

            switch (texType)
            {
                case LoadedTextureType._Albedo:
                    if (mat.HasProperty("_MainTex"))
                    {
                        mat.SetTexture("_MainTex", tex2D);
                    }
                    break;
                case LoadedTextureType._AlbedoTransparency:
                    if (mat.HasProperty("_MainTex"))
                    {
                        mat.SetTexture("_MainTex", tex2D);
                    }
                    break;
                case LoadedTextureType._Metallic:
                    if (mat.HasProperty("_MetallicGlossMap"))
                    {
                        mat.color = Color.white;
                        mat.SetTexture("_MetallicGlossMap", tex2D);
                        //if (mat.HasProperty("_GlossMapScale"))
                        //{
                        //    //float glossValue = mat.GetFloat("_Glossiness");
                        //    //float setGlossValue = glossValue <= 0 ? 0.5f : glossValue;
                        //    mat.SetFloat("_GlossMapScale", 1f);
                        //}
                    }
                    break;
                case LoadedTextureType._MetallicSmoothness:
                    if (mat.HasProperty("_MetallicGlossMap"))
                    {
                        mat.color = Color.white;
                        mat.SetTexture("_MetallicGlossMap", tex2D);
                        //if (mat.HasProperty("_GlossMapScale"))
                        //{
                        //    //float glossValue = mat.GetFloat("_Glossiness");
                        //    //float setGlossValue = glossValue <= 0 ? 0.5f : glossValue;
                        //    mat.SetFloat("_GlossMapScale", 1f);
                        //}
                    }
                    break;
                case LoadedTextureType._Normal:
                    if (mat.HasProperty("_BumpMap"))
                    {
                        mat.SetTexture("_BumpMap", tex2D);
                    }
                    break;
                case LoadedTextureType._ao:
                    if (mat.HasProperty("_OcclusionMap"))
                    {
                        mat.SetTexture("_OcclusionMap", tex2D);
                    }
                    break;
                default:
                    break;
            }
        }


        protected override void OnLoadFinish()
        {
            isLoading = false;
            progress = 1f;

            if (loadObj != null)
            {
                loadObj.SetActive(true);

                Animation anim = loadObj.GetComponent<Animation>();
                if (anim != null)
                {
                    List<AnimationClip> resultClip = new List<AnimationClip>();
                    foreach (AnimationState state in anim)
                    {
                        resultClip.Add(state.clip);
                    }
                    clips = resultClip.ToArray();
                    //GameObject.DestroyImmediate(anim);
                }
            }

            base.OnLoadFinish();
        }



    }

}
