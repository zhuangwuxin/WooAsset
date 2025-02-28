﻿using System;
using UnityEditor;

namespace WooAsset
{
    public class PrepareTask : AssetTask
    {
        protected override async void OnExecute(AssetTaskContext context)
        {
            AssetsBuildOption option = AssetsEditorTool.option;


            IAssetStreamEncrypt encrypt = Activator.CreateInstance(option.GetStreamEncryptType()) as IAssetStreamEncrypt;
            IAssetBuild assetBuild = Activator.CreateInstance(option.GetAssetBuildType()) as IAssetBuild;
            context.ignoreTypeTreeChanges = option.ignoreTypeTreeChanges;
            context.forceRebuild = option.forceRebuild;
            context.compress = option.compress;



            context.assetBuild = assetBuild;
            context.encrypt = encrypt;
            context.buildTarget = AssetsEditorTool.buildTarget;
            context.outputPath = AssetsEditorTool.outputPath;
            context.historyPath = AssetsEditorTool.historyPath;

            context.remoteHashName = VersionBuffer.remoteHashName;
            context.localHashName = VersionBuffer.localHashName;

            context.buildTargetName = AssetsEditorTool.buildTargetName;
            context.streamBundleDirectory = AssetsHelper.streamBundleDirectory;

            context.versions = new AssetsVersionCollection() { };


            context.MaxCacheVersionCount = option.MaxCacheVersionCount;
            context.shaderVariantDirectory = option.shaderVariantDirectory;
            context.PlatformSetting = option.PlatformSetting;
            context.TextureSetting = option.GetTextureSetting();
            context.PackingSetting = option.GetPackingSetting();
            context.atlasPaths = option.atlasPaths.ToArray();
            context.serverDirectory = option.serverDirectory;
            context.buildGroups = option.buildGroups;
            context.historyVersionFilePath = AssetsHelper.CombinePath(context.historyPath, context.historyVersionFileName);
            context.cleanHistory = option.cleanHistory;
            context.buildInAssets = option.buildInAssets;
            if (context.MaxCacheVersionCount < 1)
                context.MaxCacheVersionCount = 1;
            for (int i = 0; i < context.buildGroups.Count; i++)
            {
                var item = context.buildGroups[i];
                if (string.IsNullOrEmpty(item.name))
                {
                    SetErr("buildGroup name can not be null");
                    InvokeComplete();
                    return;
                }
                if (!AssetsHelper.ExistsDirectory(item.path))
                {
                    SetErr("buildGroup path not exist");
                    InvokeComplete();
                    return;
                }

                if (context.buildGroups.FindAll(x => x.name == item.name || x.path == item.path).Count != 1)
                {
                    SetErr("same path or name build Group");
                    InvokeComplete();
                    return;
                };
            }


            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode;
            if (context.forceRebuild)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            if (context.ignoreTypeTreeChanges)
                opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
            if (context.compress == CompressType.LZ4)
                opt |= BuildAssetBundleOptions.ChunkBasedCompression;
            if (context.compress == CompressType.None)
                opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
            context.BuildOption = opt;

            string versionPath = context.historyVersionFilePath;
            if (AssetsHelper.ExistsFile(versionPath))
            {
                var reader = await AssetsHelper.ReadFile(versionPath, true);
                context.versions = VersionBuffer.ReadAssetsVersionCollection(reader.bytes, new NoneAssetStreamEncrypt());
            }
            context.version = assetBuild.GetVersion(option.version, context);
            context.pipelineFinishTasks = assetBuild.GetPipelineFinishTasks(context);


            InvokeComplete();
        }

    }
}
