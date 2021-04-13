/**
 * Copyright (c) 2021 Sowyer(http://gitee.com/sowyer) All rights reserved.
*/

using System.IO;
using UnityEngine;

/// <summary>
/// Desc: TGA解析类，暂时只解析24位和32位
/// Author: Sowyer
/// 感谢大神解析TGA：https://blog.csdn.net/m0_46338411/article/details/105747165
/// </summary>
public static class TGAParser
{
    public static TGAInfo loadTGAData(byte[] data)
    {
        if (data.Length < 1)
        {
            Debug.LogError("没数据");
            return new TGAInfo();
        }
        // 读入流后开始装逼
        MemoryStream str = new MemoryStream(data);
        // 开始读取数据
        BinaryReader binReader = new BinaryReader(str);
        // 跳过文件头的12个字节
        binReader.BaseStream.Seek(12, SeekOrigin.Begin);

        // 图片宽高数据, 8个字节
        int width = binReader.ReadInt16();
        int height = binReader.ReadInt16();
        // 像素深度, 1字节
        int bitDepth = binReader.ReadByte();
        // 图像描述符, 1字节， 没啥卵用， 跳过
        binReader.BaseStream.Seek(1, SeekOrigin.Current);
        // 像素数据
        Color32[] pxData = new Color32[width * height];
        // 要看TGA的位深是24还是32， 处理时要考虑alpha通道的数值
        if (bitDepth == 24)
        {
            for (int i = 0; i < width * height; i++)
            {
                // 坑爹， bgr
                byte b = binReader.ReadByte();
                byte g = binReader.ReadByte();
                byte r = binReader.ReadByte();
                pxData[i] = new Color32(r, g, b, 1);
            }
        }
        else if (bitDepth == 32)
        {
            for (int i = 0; i < width * height; i++)
            {
                // bgra
                byte b = binReader.ReadByte();
                byte g = binReader.ReadByte();
                byte r = binReader.ReadByte();
                byte a = binReader.ReadByte();
                pxData[i] = new Color32(r, g, b, a);
            }
        }
        // ** 底下其实还有一堆数据， 啥开发者区域、拓展区域、注脚啥的

        // 返回TGA的数据
        TGAInfo ret = new TGAInfo();
        ret.width = width;
        ret.height = height;
        ret.data = pxData;
        ret.bitDepth = bitDepth;

        return ret;
    }
}
