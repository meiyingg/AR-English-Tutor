# AR English Tutor

一个基于Unity的AR英语学习应用，集成了OpenAI API进行智能对话和语言学习。

## 功能特性

- AR增强现实体验
- AI智能对话系统
- 英语学习辅助
- 实时语音交互

## 设置说明

### 1. 克隆项目
```bash
git clone https://github.com/meiyingg/AR-English-Tutor.git
cd AR-English-Tutor
```

### 2. Unity设置
- 使用Unity 2022.3 LTS或更高版本打开项目
- 确保已安装AR Foundation和XR插件

### 3. API密钥配置
**重要：请不要将API密钥直接提交到代码仓库！**

1. 获取你的OpenAI API密钥：[OpenAI API Keys](https://platform.openai.com/api-keys)
2. 在Unity编辑器中：
   - 找到 `Assets/Scripts/AI/APIConfig.asset` 文件
   - 在Inspector面板中将 `YOUR_OPENAI_API_KEY_HERE` 替换为你的实际API密钥
   - **注意：这个文件已添加到.gitignore中，不会被提交**

### 4. 环境变量设置（推荐）
为了更安全地管理API密钥，建议使用环境变量：

Windows:
```cmd
setx OPENAI_API_KEY "your_actual_api_key_here"
```

macOS/Linux:
```bash
export OPENAI_API_KEY="your_actual_api_key_here"
```

## 项目结构

```
Assets/
├── Scripts/
│   ├── AI/          # AI相关脚本
│   ├── AR/          # AR功能脚本
│   ├── UI/          # 用户界面脚本
│   └── Utils/       # 工具类脚本
├── Prefabs/         # 预制体
├── Scenes/          # 场景文件
└── Materials/       # 材质资源
```

## 开发环境

- Unity 2022.3 LTS+
- AR Foundation 5.0+
- XR Plugin Management
- OpenAI API

## 许可证

本项目遵循 MIT 许可证。

## 贡献

欢迎提交问题和功能请求！

## 安全提醒

?? **重要安全提醒**：
- 绝不要将API密钥提交到代码仓库
- 使用环境变量或配置文件管理敏感信息
- 定期轮换API密钥
- 监控API使用情况
