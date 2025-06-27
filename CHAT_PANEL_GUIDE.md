# Chat Panel 隐藏/显示功能说明

## 功能概述
为ChatTestUI添加了Chat面板的隐藏和显示功能，支持简单的切换操作。

## 新增字段

在Unity Inspector中需要设置的字段：

### Chat Panel Control
- **Chat Panel**: 拖拽Chat面板的根GameObject（通常是Canvas下的ChatPanel）
- **Toggle Chat Button**: 拖拽用于切换显示/隐藏的按钮

## 使用方法

### 1. 创建Toggle Button（超简单）

1. **在Unity中创建按钮**：
   - 右键点击Hierarchy中的`Canvas`
   - 选择 `UI` → `Button - TextMeshPro`
   - 重命名为 `ChatToggleButton`

2. **设置按钮**：
   - 拖拽按钮到合适位置（比如屏幕角落）
   - 调整按钮大小（建议50x50像素）
   - 修改按钮文字为 "?" 或 "聊天"

### 2. 连接到ChatTestUI

1. 选择包含`ChatTestUI`组件的GameObject
2. 在Inspector的"Chat Panel Control"部分：
   - **Chat Panel**: 拖拽包含聊天界面的GameObject（通常是ChatPanel）
   - **Toggle Chat Button**: 拖拽刚创建的ChatToggleButton

### 3. 完成！
运行游戏，点击按钮就能隐藏/显示Chat面板了。

### 2. 运行时控制

#### 通过按钮切换
用户点击Toggle Chat Button即可切换Chat面板的显示/隐藏状态。

#### 通过代码控制
```csharp
// 获取ChatTestUI组件
ChatTestUI chatUI = FindObjectOfType<ChatTestUI>();

// 显示Chat面板
chatUI.ShowChatPanel();

// 隐藏Chat面板
chatUI.HideChatPanel();

// 切换Chat面板状态
chatUI.ToggleChatPanel();

// 检查Chat面板是否可见
bool isVisible = chatUI.IsChatPanelVisible();
```

## 特性

- ? 简单的显示/隐藏切换
- ? 状态记录（isChatPanelVisible）
- ? Debug日志输出
- ? 公共API供外部调用
- ? 初始状态设置（默认显示）
- ? 安全检查（null检查）

## 扩展建议

如果需要更复杂的功能，可以考虑：

1. **动画效果**: 使用DOTween或Unity Animation添加淡入淡出效果
2. **按钮文本更新**: 根据状态显示"Show Chat"/"Hide Chat"
3. **快捷键支持**: 添加键盘快捷键（如Escape键）
4. **自动隐藏**: 在特定条件下自动隐藏（如AR扫描时）
5. **状态保存**: 保存用户的偏好设置

## 注意事项

- 确保在Inspector中正确分配所有字段
- Chat Panel应该是包含整个聊天界面的根GameObject
- Toggle Button可以放置在任何位置，建议在UI的显眼处
