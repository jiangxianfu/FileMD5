﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FileMD5
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("请输入目录名称");
                return;
            }
            string folder = args[0];
            DirectoryInfo root = new DirectoryInfo(folder);
            if (!root.Exists)
            {
                Console.WriteLine("目录不存在");
                return;
            }
            DirectoryInfo curr = new DirectoryInfo("repeat_files");
            if (!curr.Exists)
            {
                curr.Create();
            }
            var allfiles = Directory.EnumerateFiles(root.FullName, "*.*", SearchOption.AllDirectories);
            Dictionary<string, string> checklist = new Dictionary<string, string>();
            File.AppendAllText("log.txt", DateTime.Now.ToString() + "\n--------------\n");
            foreach (var item in allfiles)
            {
                string hash = GetMD5Hash(item);
                if (string.IsNullOrEmpty(hash))
                    continue;
                if (checklist.ContainsKey(hash))
                {
                    Console.WriteLine("重复{0}", item);
                    string logitem = string.Format("已存在:{0}-->被移除:{1}\n", checklist[hash], item);
                    File.AppendAllText("log.txt", logitem);
                    try
                    {
                        File.Move(item, Path.Combine(curr.FullName, Path.GetFileName(item)));
                    }
                    catch
                    {
                        try
                        {
                            File.Move(item, Path.Combine(curr.FullName, Path.GetFileNameWithoutExtension(item) + "_" + Guid.NewGuid().ToString("N") + Path.GetExtension(item)));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("移动文件失败" + ex.Message);
                        }
                    }
                }
                else
                {
                    checklist.Add(hash, item);
                }
            }
        }

        /// <summary>
        /// 计算文件的MD5码
        /// </summary>
        /// <param name="fullname"></param>
        /// <returns></returns>
        private static string GetMD5Hash(string fullname)
        {
            string strResult = "";
            string strHashData = "";
            byte[] arrbytHashValue;
            MD5CryptoServiceProvider oMD5Hasher = new MD5CryptoServiceProvider();
            try
            {
                using (var oFileStream = new FileStream(fullname, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    //计算指定Stream 对象的哈希值
                    arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);
                }
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                strHashData = System.BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData;
            }
            catch
            {
                return "";
            }

            return strResult;
        }
    }
}
