/**
 * Copyright (c) 2021 Sowyer(http://gitee.com/sowyer) All rights reserved.
*/

using UnityEngine;
/// <summary>
/// Desc: TGA信息
/// Author: Sowyer (email: sowyer2010@qq.com)
/// </summary>
public class TGAInfo
{
    // 图片的大小, 无视小数， 如果有， 强转！！
    public int width;
    public int height;
    public int bitDepth; // 位深信息
    public Color32[] data;
}
