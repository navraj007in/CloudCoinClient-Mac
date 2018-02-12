using System;
using System.Collections.Generic;
using CloudCoinCore;

namespace CloudCoinClientMAC.CoreClasses
{
    public class FileSystem : IFileSystem
    {
        public FileSystem()
        {
        }

        public override void ClearCoins(string FolderName)
        {
            throw new NotImplementedException();
        }

        public override bool CreateFolderStructure()
        {
            throw new NotImplementedException();
        }

        public override void DetectPreProcessing()
        {
            throw new NotImplementedException();
        }

        public override void LoadFileSystem()
        {
            throw new NotImplementedException();
        }

        public override void ProcessCoins(IEnumerable<CloudCoin> coins)
        {
            throw new NotImplementedException();
        }
    }
}
