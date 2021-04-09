using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;

namespace AVRModelViewer
{
    public class TextureLoader : ResLoader
    {

        private Action<Sprite> onLoadSprite;

        private Texture2D tex2D;



        public TextureLoader(Action<Sprite> _loaded)
        {
            onLoadSprite = _loaded;
        }

        public override void LoadResSync(string _path)
        {
            if (_path.ToLower().Contains("http"))
            {
                Debug.LogError("---同步不支持http!");
                return;
            }

            LoadLocalImage(_path);
        }

        public override void LoadResASync(string _path)
        {
            AVRViewer.instance.StartCoroutine(IELoadTexture(_path));
        }



        private IEnumerator IELoadTexture(string url)
        {
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
            {
                yield return req.SendWebRequest();
                if (req.isNetworkError || req.isHttpError)
                {
                    Debug.LogError(req.error);

                    isLoading = false;
                    onLoadSprite?.Invoke(null);
                }
                else
                {
                    tex2D = DownloadHandlerTexture.GetContent(req);
                    Sprite sp = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.one * 0.5f);        
                    isLoading = false;
                    onLoadSprite?.Invoke(sp);
                }
            }

        }



        private void LoadLocalImage(string _path)
        {
            try
            {
                using (FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.Read))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] texBytes = new byte[fs.Length];
                    fs.Read(texBytes, 0, (int)fs.Length);
                    fs.Close();
                    fs.Dispose();

                    tex2D = new Texture2D(512, 512);
                    tex2D.LoadImage(texBytes);

                    Sprite sp = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.one * 0.5f);
                    isLoading = false;
                    onLoadSprite?.Invoke(sp);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                isLoading = false;
                onLoadSprite?.Invoke(null);
            }
        }



    }

}
