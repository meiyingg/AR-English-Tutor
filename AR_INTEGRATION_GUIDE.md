# Unity AR 功能集成指南 (Android)

本文档旨在为你提供一个清晰、分步的指南，用于在你的 "AR English Tutor" 项目中集成核心 AR 功能。

## 阶段一：实现 AR 平面检测 (AR Plane Detection)

这是我们集成 AR 的第一步，目标是让应用能够识别并高亮显示真实环境中的平面（如地面、桌面）。

**实现步骤:**

1.  **创建 AR 场景**:
    *   新建一个场景 (例如 `ARScene`)。
    *   删除场景中默认的 `Main Camera`。

2.  **添加 AR 核心组件**:
    *   在场景中添加 `AR Session` GameObject。这个对象负责管理整个 AR 会话的生命周期。 (`GameObject -> XR -> AR Session`)
    *   添加 `XR Origin (Mobile AR)` GameObject。在较新版本的 Unity 中，它取代了旧的 `AR Session Origin`。这是 AR 世界的中心点，所有 AR 内容都将相对于它来定位。 (`GameObject -> XR -> XR Origin (Mobile AR)`)
        *   **快捷方式**: 你也可以直接在 `Hierarchy` 窗口右键点击默认的 `Main Camera`，然后选择 `XR -> Convert Main Camera To XR Rig`，这会一步到位地完成替换。
        *   在 `XR Origin` 下，会自动包含一个 `AR Camera`，它将取代我们删除的 `Main Camera`，负责渲染 AR 视图。

3.  **配置平面检测**:
    *   在 `XR Origin` 上，添加 `AR Plane Manager` 组件。
    *   这个组件负责检测真实世界中的平面。你需要为它指定一个 **Plane Prefab**。当检测到新的平面时，Unity 会使用这个 Prefab 来实例化一个对象，从而在视觉上把平面表示出来。

4.  **创建平面可视化 Prefab**:
    *   创建一个新的 3D 对象 (例如一个简单的 `Plane`) 作为可视化的基础。
    *   给它添加 `AR Plane Mesh Visualizer` 组件，这个组件会自动根据检测到的平面形状更新 Mesh。
    *   为了美观，可以给它一个半透明的材质，并用 `Line Renderer` 来绘制平面的边界。
    *   将这个配置好的对象拖拽到 `Assets/Prefabs` 文件夹，创建成一个 Prefab。
    *   最后，将这个 Prefab 赋值给 `AR Plane Manager` 的 `Plane Prefab` 字段。

5.  **项目设置检查**:
    *   前往 `Project Settings > XR Plug-in Management`。
    *   确保在 Android 标签页下，`ARCore` 插件已经被勾选。

完成以上步骤后，将应用部署到 Android 设备上，你就能看到摄像头捕捉的真实环境中，被识别出的平面会被半透明的网格覆盖。

---

## 阶段二：聊天界面 (Chat UI) 悬浮方案

目标是将聊天气泡或整个聊天窗口，固定在 AR Tutor 虚拟形象的头顶。

**实现思路:**

1.  **使用世界空间画布 (World Space Canvas)**:
    *   创建一个 `Canvas` GameObject。
    *   将其 `Render Mode` 设置为 `World Space`。这样，UI 元素就会像 3D 世界中的普通物体一样，而不是覆盖在屏幕上。

2.  **UI 成为子对象**:
    *   将这个 `World Space Canvas` 拖拽到你的 AR Tutor 模型**头部骨骼 (Head Bone) 的 Transform**下，成为它的子对象。
    *   调整 Canvas 的位置和缩放，使其正好位于头顶的合适位置。

3.  **朝向摄像机**:
    *   为了确保无论用户从哪个角度看，UI 都是正对着用户的，可以给 Canvas 添加一个简单的脚本，让它的 `transform.rotation` 在每一帧 (`Update`) 都朝向 `AR Camera` 的位置。

通过这种方式，当 AR Tutor 移动、旋转时，头顶的 UI 也会随之移动，看起来就像是“长”在它头上一样。
