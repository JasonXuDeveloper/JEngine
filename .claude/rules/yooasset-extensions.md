# YooAsset Extension Sample Maintenance

YooAsset ships Extension Samples under `Assets/Samples/YooAsset/`. These are vendor-provided examples that JEngine customizes. When YooAsset is updated and its Extension Samples change, our customizations may be overwritten.

## Custom Modifications to Track

### PreprocessBuildCatalog (Decryption Support)

**File**: `Assets/Samples/YooAsset/*/Extension Sample/Editor/PreprocessBuild/PreprocessBuildCatalog.cs`

**What we changed**: The upstream code calls `CatalogTools.CreateCatalogFile(null, ...)` with a `//TODO 自行处理解密` comment, which causes a NRE when manifests are encrypted. We replaced this with a try-all approach that attempts each `IManifestRestoreServices` from `EncryptionMapping.Mapping` until one succeeds.

**Why**: See [YooAsset#730](https://github.com/tuyoogame/YooAsset/issues/730). The YooAsset author confirmed this is an intentional extension point for consumers to customize.

**On YooAsset update**: Verify that `PreprocessBuildCatalog.cs` still contains our `TryCreateCatalogFile` / `TryCreate` helper methods and the `using JEngine.Core.Encrypt` import. If the sample was overwritten by the update, reapply the decryption try-all logic.

## Update Checklist

When updating YooAsset or its Extension Samples:

- [ ] Diff the new Extension Sample files against our customized versions
- [ ] Reapply any overwritten JEngine customizations listed above
- [ ] Verify catalog generation works with both encrypted and unencrypted manifests
- [ ] Add any new upstream extension points to this file if customized
