using System.IO;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;



public static class ZipUtility
{

    public static List<string> GetListFullName = new List<string>();
    

    #region ZipCallback
    public abstract class ZipCallback
    {
        /// <summary>
        /// 压缩单个文件或文件夹前执行的回调
        /// </summary>
        /// <param name="_entry"></param>
        /// <returns>如果返回true，则压缩文件或文件夹，反之则不压缩文件或文件夹</returns>
        public virtual bool OnPreZip(ZipEntry _entry)
        {
            return true;
        }

        /// <summary>
        /// 压缩单个文件或文件夹后执行的回调
        /// </summary>
        /// <param name="_entry"></param>
        public virtual void OnPostZip(ZipEntry _entry) { }

        /// <summary>
        /// 压缩执行完毕后的回调
        /// </summary>
        /// <param name="_result">true表示压缩成功，false表示压缩失败</param>
        public virtual void OnFinished(bool _result) { }
    }
    #endregion

    #region UnzipCallback
    public abstract class UnzipCallback
    {
        /// <summary>
        /// 解压单个文件或文件夹前执行的回调
        /// </summary>
        /// <param name="_entry"></param>
        /// <returns>如果返回true，则压缩文件或文件夹，反之则不压缩文件或文件夹</returns>
        public virtual bool OnPreUnzip(ZipEntry _entry)
        {
            return true;
        }

        /// <summary>
        /// 解压单个文件或文件夹后执行的回调
        /// </summary>
        /// <param name="_entry"></param>
        public virtual void OnPostUnzip(ZipEntry _entry) { }

        /// <summary>
        /// 解压执行完毕后的回调
        /// </summary>
        /// <param name="_result">true表示解压成功，false表示解压失败</param>
        public virtual void OnFinished(bool _result) { }
    }
    #endregion

    /// <summary>
    /// 压缩文件和文件夹
    /// </summary>
    /// <param name="_fileOrDirectoryArray">文件夹路径和文件名</param>
    /// <param name="_outputPathName">压缩后的输出路径文件名</param>
    /// <param name="_password">压缩密码</param>
    /// <param name="_zipCallback">ZipCallback对象，负责回调</param>
    /// <returns></returns>
    public static bool Zip(string[] _fileOrDirectoryArray, string _outputPathName, string _password = null, ZipCallback _zipCallback = null)
    {
        if ((null == _fileOrDirectoryArray) || string.IsNullOrEmpty(_outputPathName))
        {
            if (null != _zipCallback)
                _zipCallback.OnFinished(false);

            return false;
        }

        

        ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(_outputPathName));
        zipOutputStream.SetLevel(6);    // 压缩质量和压缩速度的平衡点
        if (!string.IsNullOrEmpty(_password))
            zipOutputStream.Password = _password;

        for (int index = 0; index < _fileOrDirectoryArray.Length; ++index)
        {
            bool result = false;
            string fileOrDirectory = _fileOrDirectoryArray[index];
            if (Directory.Exists(fileOrDirectory))
                result = ZipDirectory(fileOrDirectory, string.Empty, zipOutputStream, _zipCallback);
            else if (File.Exists(fileOrDirectory))
                result = ZipFile(fileOrDirectory, string.Empty, zipOutputStream, _zipCallback);

            if (!result)
            {
                if (null != _zipCallback)
                    _zipCallback.OnFinished(false);

                return false;
            }
        }

        zipOutputStream.Finish();
        zipOutputStream.Close();

        if (null != _zipCallback)
            _zipCallback.OnFinished(true);

        return true;
    }

    public static bool ZipBytes(byte[] _bytes, string _outputPathName, string _password = null, ZipCallback _zipCallback = null)
    {
        if ((null == _bytes) || string.IsNullOrEmpty(_outputPathName))
        {

            Debug.Log(_bytes + "  文件流为空");
            return false;
        }
        if (!Directory.Exists(_outputPathName))
        {
            Directory.CreateDirectory(_outputPathName);
        }

        Debug.Log(_outputPathName);
        _outputPathName = PathRespace(_outputPathName);
        string _fileName = Path.Combine(_outputPathName, "ttjkz.zip");
        _fileName = PathRespace(_fileName);

        Debug.LogError(_fileName);

        ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(_fileName));
        zipOutputStream.SetLevel(6);    // 压缩质量和压缩速度的平衡点        

       
        ZipEntry entry = null;
        FileStream fileStream = null;
        try
        {
            string entryName = Path.Combine("/", Path.GetFileName(_fileName));
           
            entryName = PathRespace(entryName);
            Debug.Log("entryName:" + entryName);
            entry = new ZipEntry(entryName);
            entry.DateTime = System.DateTime.Now;

            if ((null != _zipCallback) && !_zipCallback.OnPreZip(entry))
                return true;    // 过滤
         

            entry.Size = _bytes.Length;

            //crc32.Reset();
            //crc32.Update(buffer);
            //entry.Crc = crc32.Value;

            zipOutputStream.PutNextEntry(entry);
            zipOutputStream.Write(_bytes, 0, _bytes.Length);
          
        }
        catch (System.Exception _e)
        {
            Debug.LogError("[ZipUtility.ZipFile]: " + _e.ToString());
            return false;
        }
        finally
        {
            if (null != fileStream)
            {
                fileStream.Flush();
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        if (null != _zipCallback)
            _zipCallback.OnPostZip(entry);

        //return true;
        zipOutputStream.Flush();
        
        zipOutputStream.Finish();
        zipOutputStream.Close();

        zipOutputStream.Dispose();

        return true;
    }

    /// <summary>
    /// 解压Zip包
    /// </summary>
    /// <param name="_filePathName">Zip包的文件路径名</param>
    /// <param name="_outputPath">解压输出路径</param>
    /// <param name="_password">解压密码</param>
    /// <param name="_unzipCallback">UnzipCallback对象，负责回调</param>
    /// <returns></returns>
    public static bool UnzipFile(string _filePathName, string _outputPath, string _password = null, UnzipCallback _unzipCallback = null)
    {
        if (string.IsNullOrEmpty(_filePathName) || string.IsNullOrEmpty(_outputPath))
        {
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);

            return false;
        }

        try
        {
            //Debug.Log("_filePathName:" + _filePathName);
            return UnzipFile(File.OpenRead(_filePathName), _outputPath, _password, _unzipCallback);
        }
        catch (System.Exception _e)
        {
            Debug.LogError("[ZipUtility.UnzipFile]: " + _e.ToString());

            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);

            return false;
        }
    }

    /// <summary>
    /// 解压Zip包
    /// </summary>
    /// <param name="_fileBytes">Zip包字节数组</param>
    /// <param name="_outputPath">解压输出路径</param>
    /// <param name="_password">解压密码</param>
    /// <param name="_unzipCallback">UnzipCallback对象，负责回调</param>
    /// <returns></returns>
    public static bool UnzipFile(byte[] _fileBytes, string _outputPath, string _password = null, UnzipCallback _unzipCallback = null)
    {
        if ((null == _fileBytes) || string.IsNullOrEmpty(_outputPath))
        {
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);

            return false;
        }

        

        bool result = UnzipFile(new MemoryStream(_fileBytes), _outputPath, _password, _unzipCallback);
        if (!result)
        {
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);
        }

        return result;
    }

    /// <summary>
    /// 解压Zip包
    /// </summary>
    /// <param name="_inputStream">Zip包输入流</param>
    /// <param name="_outputPath">解压输出路径</param>
    /// <param name="_password">解压密码</param>
    /// <param name="_unzipCallback">UnzipCallback对象，负责回调</param>
    /// <returns></returns>
    public static bool UnzipFile(Stream _inputStream, string _outputPath, string _password = null, UnzipCallback _unzipCallback = null)
    {
        if ((null == _inputStream) || string.IsNullOrEmpty(_outputPath))
        {
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);

            return false;
        }

        //Debug.Log(_inputStream.Length);

        // 创建文件目录
        if (!Directory.Exists(_outputPath))
            Directory.CreateDirectory(_outputPath);



        // 解压Zip包
        ZipEntry entry = null;
        using (ZipInputStream zipInputStream = new ZipInputStream(_inputStream))
        {
            if (!string.IsNullOrEmpty(_password))
                zipInputStream.Password = _password;

            while (null != (entry = zipInputStream.GetNextEntry()))
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                if ((null != _unzipCallback) && !_unzipCallback.OnPreUnzip(entry))
                    continue;   // 过滤

                string filePathName = Path.Combine(_outputPath, entry.Name);

                // 创建文件目录
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(filePathName);
                    continue;
                }

                // 写入文件
                try
                {
                    using (FileStream fileStream = File.Create(filePathName))
                    {
                        byte[] bytes = new byte[1024];
                        while (true)
                        {
                            int count = zipInputStream.Read(bytes, 0, bytes.Length);
                            if (count > 0)
                                fileStream.Write(bytes, 0, count);
                            else
                            {
                                if (null != _unzipCallback)
                                    _unzipCallback.OnPostUnzip(entry);

                                break;
                            }
                        }
                    }
                }
                catch (System.Exception _e)
                {
                    Debug.LogError("[ZipUtility.UnzipFile]: " + _e.ToString());

                    if (null != _unzipCallback)
                        _unzipCallback.OnFinished(false);

                    return false;
                }
            }
        }

        if (null != _unzipCallback)
            _unzipCallback.OnFinished(true);

        return true;
    }

    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="_filePathName">文件路径名</param>
    /// <param name="_parentRelPath">要压缩的文件的父相对文件夹</param>
    /// <param name="_zipOutputStream">压缩输出流</param>
    /// <param name="_zipCallback">ZipCallback对象，负责回调</param>
    /// <returns></returns>
    private static bool ZipFile(string _filePathName, string _parentRelPath, ZipOutputStream _zipOutputStream, ZipCallback _zipCallback = null)
    {
        //Crc32 crc32 = new Crc32();
        ZipEntry entry = null;
        FileStream fileStream = null;
        try
        {
            string entryName = _parentRelPath + '/' + Path.GetFileName(_filePathName);
            //Debug.Log("entryName:" + entryName);

            entry = new ZipEntry(entryName);
            entry.DateTime = System.DateTime.Now;

            if ((null != _zipCallback) && !_zipCallback.OnPreZip(entry))
                return true;    // 过滤

            fileStream = File.OpenRead(_filePathName);
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();

            entry.Size = buffer.Length;

            //crc32.Reset();
            //crc32.Update(buffer);
            //entry.Crc = crc32.Value;

            _zipOutputStream.PutNextEntry(entry);
            _zipOutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (System.Exception _e)
        {
            Debug.LogError("[ZipUtility.ZipFile]: " + _e.ToString());
            return false;
        }
        finally
        {
            if (null != fileStream)
            {
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        if (null != _zipCallback)
            _zipCallback.OnPostZip(entry);

        return true;
    }

    /// <summary>
    /// 压缩文件夹
    /// </summary>
    /// <param name="_path">要压缩的文件夹</param>
    /// <param name="_parentRelPath">要压缩的文件夹的父相对文件夹</param>
    /// <param name="_zipOutputStream">压缩输出流</param>
    /// <param name="_zipCallback">ZipCallback对象，负责回调</param>
    /// <returns></returns>
    private static bool ZipDirectory(string _path, string _parentRelPath, ZipOutputStream _zipOutputStream, ZipCallback _zipCallback = null)
    {
        ZipEntry entry = null;
        try
        {
            string entryName = Path.Combine(_parentRelPath, Path.GetFileName(_path) + '/');
            entry = new ZipEntry(entryName);
            entry.DateTime = System.DateTime.Now;
            entry.Size = 0;

            if ((null != _zipCallback) && !_zipCallback.OnPreZip(entry))
                return true;    // 过滤

            _zipOutputStream.PutNextEntry(entry);
            _zipOutputStream.Flush();

            string[] files = Directory.GetFiles(_path);
            for (int index = 0; index < files.Length; ++index)
                ZipFile(files[index], Path.Combine(_parentRelPath, Path.GetFileName(_path)), _zipOutputStream, _zipCallback);
        }
        catch (System.Exception _e)
        {
            Debug.LogError("[ZipUtility.ZipDirectory]: " + _e.ToString());
            return false;
        }

        string[] directories = Directory.GetDirectories(_path);
        for (int index = 0; index < directories.Length; ++index)
        {
            if (!ZipDirectory(directories[index], Path.Combine(_parentRelPath, Path.GetFileName(_path)), _zipOutputStream, _zipCallback))
                return false;
        }

        if (null != _zipCallback)
            _zipCallback.OnPostZip(entry);

        return true;
    }







    public static bool DeleteDir(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path)|| !Directory.Exists(path))
            {
                return true;  // 如果参数为空，则视为已成功清空
            }
            // 删除当前文件夹下所有文件
            foreach (string strFile in Directory.GetFiles(path))
            {
                //Debug.Log(strFile);
                File.Delete(strFile);
            }
            // 删除当前文件夹下所有子文件夹(递归)
            foreach (string strDir in Directory.GetDirectories(path))
            {
                //Debug.Log(strDir);
                //DeleteDir(strDir);
                Directory.Delete(strDir, true);
            }

            return true;
        }
        catch (System.Exception ex) // 异常处理
        {
            Debug.LogError("[ZipUtility.DeleteDir]: " + ex.ToString());
            return false;
        }

    }


    /// <summary>
    /// 获取文件路径
    /// </summary>
    /// <param name="path">文件夹</param>
    /// <param name="fileName">文件名.xx</param>
    /// <returns></returns>
    public static string FindFilePath(string path, string fileName)
    {
        try
        {
            DirectoryInfo theFolder = new DirectoryInfo(path);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                FileInfo[] fileInfo = NextFolder.GetFiles();
                foreach (FileInfo NextFile in fileInfo)  //遍历文件
                {
                    //Debug.Log(NextFile.Name);
                    if (NextFile.Name.Equals(fileName))
                    {
                        return PathRespace(NextFile.FullName);
                    }
                }
                //递归
                return FindFilePath(theFolder.FullName, fileName);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ZipUtility.FindFilePath]: " + ex.ToString());
            //return lst;
        }
        return "";
    }

    /// <summary>
    /// 根据后缀查找文件
    /// </summary>
    public static string FindFileByEndType(string path, string _type, out string _fileName)
    {
        _fileName = "";
        try
        {
            DirectoryInfo theFolder = new DirectoryInfo(path);
            DirectoryInfo[] dirInfo = theFolder.GetDirectories();
            //遍历文件夹
            foreach (DirectoryInfo NextFolder in dirInfo)
            {
                FileInfo[] fileInfo = NextFolder.GetFiles();
                foreach (FileInfo NextFile in fileInfo)  //遍历文件
                {
                    //Debug.Log(NextFile.Name);
                    if (NextFile.Name.EndsWith(_type))
                    {
                        _fileName = NextFile.Name;
                        return PathRespace(NextFile.FullName);
                    }
                }
                //递归
                return FindFilePath(theFolder.FullName, _type);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ZipUtility.FindFileByEndType]: " + ex.ToString());
            //return lst;
        }
        return "";
    }





    /// <summary>
    /// 在文件夹里查找模型文件(只支持.zip/.fbx/.gltf/.glb/.unity3d)
    /// </summary>
    /// <param name="folder">文件夹Path</param>
    /// <returns></returns>
    public static string FindModelFileFullName(string folder, out string format)
    {      
        folder = ZipUtility.PathRespace(folder);
        Debug.Log("文件夹路径  " + folder);

        DirectoryInfo theFolder = new DirectoryInfo(folder);
        //当前层检测是否找到
        FileInfo[] fileInfos = theFolder.GetFiles();
        for (int i = 0; i < fileInfos.Length; i++)
        {
            string fileName = fileInfos[i].Name.ToLower();
            if (fileName.EndsWith(".fbx"))
            {
                format = ".fbx";
                return PathRespace(fileInfos[i].FullName);
            }
            if (fileName.EndsWith(".gltf"))
            {
                format = ".gltf";
                return PathRespace(fileInfos[i].FullName);
            }
            if (fileName.EndsWith(".glb"))
            {
                format = ".glb";
                return PathRespace(fileInfos[i].FullName);
            }
            if (fileName.EndsWith(".unity3d"))
            {
                format = ".unity3d";
                return PathRespace(fileInfos[i].FullName);
            }
            if (fileName.EndsWith(".zip"))
            {
                format = ".zip";
                return PathRespace(fileInfos[i].FullName);
            }
        }

       

        //递归子文件夹查找
        DirectoryInfo[] childFolders = theFolder.GetDirectories();
        for (int i = 0; i < childFolders.Length; i++)
        {
            string found = FindModelFileFullName(childFolders[i].FullName, out string outEndFile);
            if (!string.IsNullOrEmpty(found))
            {
                format = outEndFile;
                return found;
            }
        }

        Debug.Log("资源没找到  " + folder);

        format = "";
        return "";
    }
    public static string[] FindModelFileFullNameList(string folder, out string format)
    {
        if (!Directory.Exists(folder))
        {
            format = "";
            return null;
        }

        DirectoryInfo theFolder = new DirectoryInfo(folder);
        //当前层检测是否找到
        FileInfo[] fileInfos = theFolder.GetFiles();
        for (int i = 0; i < fileInfos.Length; i++)
        {
            string fileName = fileInfos[i].Name.ToLower();
            if (fileName.EndsWith(".fbx"))
            {
                format = ".fbx";
                GetListFullName.Add(fileInfos[i].FullName);
            }
            if (fileName.EndsWith(".gltf"))
            {
                format = ".gltf";
                GetListFullName.Add(fileInfos[i].FullName);
            }
            if (fileName.EndsWith(".glb"))
            {
                format = ".glb";
                GetListFullName.Add(fileInfos[i].FullName);
            }
            if (fileName.EndsWith(".unity3d"))
            {
                format = ".unity3d";
                GetListFullName.Add(fileInfos[i].FullName);
            }
            if (fileName.EndsWith(".zip"))
            {
                format = ".zip";
                GetListFullName.Add(fileInfos[i].FullName);
            }
            if (fileName.EndsWith(".meta"))
            {
                continue;
            }
        }



        //递归子文件夹查找
        DirectoryInfo[] childFolders = theFolder.GetDirectories();
        for (int i = 0; i < childFolders.Length; i++)
        {
            string[] found = FindModelFileFullNameList(childFolders[i].FullName, out string outEndFile);
            if (found != null)
            {
                format = outEndFile;
                return found;
            }
        }

        format = "";
        return null;
    }

    /// <summary>
    /// 递归查找文件夹下的文件
    /// </summary>
    /// <param name="folder">文件夹</param>
    /// <param name="fileName">文件名(可不带格式)</param>
    /// <param name="ingoreSize">是否忽略大小写</param>
    /// <returns></returns>
    public static string FindFileFullName(string folder, string fileName, bool ingoreSize = false)
    {
        if (!Directory.Exists(folder))
        {
            return "";
        }

        DirectoryInfo theFolder = new DirectoryInfo(folder);
        //当前层检测是否找到
        FileInfo[] fileInfos = theFolder.GetFiles();
        for (int i = 0; i < fileInfos.Length; i++)
        {
            string check = fileInfos[i].Name;
            //忽略大小写
            if (ingoreSize)
            {
                if (check.ToLower().Contains(fileName.ToLower()))
                {
                    return PathRespace(fileInfos[i].FullName);
                }
            }
            else
            {
                if (check.Contains(fileName))
                {
                    return PathRespace(fileInfos[i].FullName);
                }
            }
        }

        //递归子文件夹查找
        DirectoryInfo[] childFolders = theFolder.GetDirectories();
        for (int i = 0; i < childFolders.Length; i++)
        {
            string found = FindFileFullName(childFolders[i].FullName, fileName, ingoreSize);
            if (!string.IsNullOrEmpty(found))
            {
                return found;
            }
        }

        return "";
    }
    /// <summary>
    /// 查找文件路径并返回查找到的第一个
    /// </summary>
    /// <param name="folder">文件夹Path</param>
    /// <param name="format">文件格式(.xxx)</param>
    /// <returns></returns>
    public static string FindFileFullNameByEndFile(string folder, string format)
    {

        if (string.IsNullOrEmpty(folder))
            return "";

        DirectoryInfo theFolder = new DirectoryInfo(folder);
        //当前层检测是否找到
        FileInfo[] fileInfos = theFolder.GetFiles();
        for (int i = 0; i < fileInfos.Length; i++)
        {
            string fileName = fileInfos[i].Name.ToLower();
            if (fileName.EndsWith(format))
            {
                return PathRespace(fileInfos[i].FullName);
            }
        }


        //递归子文件夹查找
        DirectoryInfo[] childFolders = theFolder.GetDirectories();
        for (int i = 0; i < childFolders.Length; i++)
        {
            string found = FindFileFullNameByEndFile(childFolders[i].FullName, format);
            if (!string.IsNullOrEmpty(found))
                return found;
        }

        return "";
    }










    public static string PathRespace(string _target)
    {
        string newString = _target.Replace(@"\", "/");
        return newString;
    }

}

