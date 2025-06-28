请在Unity Asset Store或GitHub导入NativeGallery插件（推荐Asset Store一键导入）。

Asset Store链接：https://assetstore.unity.com/packages/tools/integration/native-gallery-for-android-ios-112630
GitHub链接：https://github.com/yasirkula/UnityNativeGallery

导入后，Android和iOS平台可用NativeGallery来选择图片。

使用方法示例：
NativeGallery.GetImageFromGallery((path) => {
    if(path != null) {
        // 处理图片
    }
}, "选择一张图片", "image/*");
