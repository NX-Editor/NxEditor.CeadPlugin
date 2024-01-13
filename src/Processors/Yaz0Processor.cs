﻿using NxEditor.PluginBase.Models;
using NxEditor.PluginBase.Services;
using Yaz0Library;

namespace NxEditor.EpdPlugin.Processors;

public class Yaz0Processor : IProcessingService
{
    public bool IsValid(IFileHandle handle)
    {
        return handle.Data.Length >= 4
            && handle.Data.AsSpan()[0..4].SequenceEqual("Yaz0"u8);
    }

    public IFileHandle Process(IFileHandle handle)
    {
        handle.Data = Yaz0.Decompress(handle.Data).ToArray();
        return handle;
    }

    public IFileHandle Reprocess(IFileHandle handle)
    {
        handle.Data = Yaz0.Compress(handle.Data, out Yaz0SafeHandle safeHandle, level: Convert.ToInt32(EpdConfig.Shared.Yaz0CompressionLevel)).ToArray();
        safeHandle.Dispose();
        return handle;
    }
}
