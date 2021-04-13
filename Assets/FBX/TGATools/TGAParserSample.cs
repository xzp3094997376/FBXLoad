/**
 * Copyright (c) 2021 Sowyer(http://gitee.com/sowyer) All rights reserved.
*/
using System.IO;
using UnityEngine;

public class TGAParserSample : MonoBehaviour
{
    void Start()
    {
        // load tga
        TGAInfo tgaInfo = TGAParser.loadTGAData(File.ReadAllBytes("wtf.tga"));

        Texture2D texture = new Texture2D(1, 1);
        // 判断用哪种纹理格式
        TextureFormat format = texture.format;
        if (tgaInfo.bitDepth == 24)
        {
            format = TextureFormat.RGB24;
        }
        else if (tgaInfo.bitDepth == 32)
        {
            format = TextureFormat.ARGB32;
        }
        texture.Resize(tgaInfo.width, tgaInfo.height, format, false); // 主要为了设置一下format
        texture.SetPixels32(tgaInfo.data);
        texture.Apply();
    }
}