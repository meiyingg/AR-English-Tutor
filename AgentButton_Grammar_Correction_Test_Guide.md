# AgentButton与Grammar Correction联动测试指南

## 问题修复说明

### 原问题
在Normal模式下，使用AgentButton进行语音输入后，AI没有提供grammar correction和示范。

### 根本原因
1. **路径分支问题**: `OnTranscriptionReceived`方法中，AgentButton的语音输入会优先选择`SendSceneRecognitionRequest`而不是`SendMessage`
2. **缺失逻辑**: `SendSceneRecognitionRequest`方法没有调用grammar correction功能

### 修复方案
1. **修改ChatManager.cs**: 在`SendSceneRecognitionRequest`方法中，当处于Normal模式时，AI回复后自动调用`ProvideGrammarCorrection`
2. **优化OnTranscriptionReceived逻辑**: 根据当前学习模式智能判断是否使用场景上下文

## 测试步骤

### 测试1: Normal模式下AgentButton语音输入
1. **切换到Normal模式**:
   ```
   点击"Normal Mode"按钮或输入"/normal"
   ```

2. **使用AgentButton进行语音输入**:
   - 点击AgentButton（语音输入按钮）
   - 说一段包含语法错误的英语，如: "I are going to school yesterday"
   - 观察AI回复

3. **期望结果**:
   - AI首先提供对话回复
   - 然后自动提供语法纠错和示范
   - 格式类似：
     ```
     ? Grammar Analysis:
     Original: "I are going to school yesterday"
     Correction: "I went to school yesterday"
     
     ? Grammar Tips:
     - Use "went" (past tense) instead of "are going" when talking about yesterday
     - "Yesterday" indicates past time, so use past tense verbs
     
     ? Practice Examples:
     - "I went to the store yesterday"
     - "She visited her friend last week"
     ```

### 测试2: Scene模式下AgentButton行为
1. **切换到Scene模式**:
   ```
   点击"Scene Mode"按钮或输入"/scene"
   ```

2. **使用AgentButton**:
   - 确保有可见的AR场景对象
   - 点击AgentButton进行语音输入
   - 说: "What do you see here?"

3. **期望结果**:
   - AI分析当前AR场景
   - 提供场景相关的英语学习内容
   - **不**提供grammar correction（因为是Scene模式）

### 测试3: Normal模式下场景相关查询
1. **切换到Normal模式**
2. **使用AgentButton说场景相关内容**:
   - "What do you see in this picture?"
   - "Describe what's here"
   - "Tell me about this scene"

3. **期望结果**:
   - AI会分析场景（因为查询明显与场景相关）
   - 同时提供grammar correction（因为是Normal模式）

### 测试4: Normal模式下非场景查询
1. **切换到Normal模式**
2. **使用AgentButton说一般性内容**:
   - "How are you today?"
   - "I like pizza very much"
   - "The weather is nice"

3. **期望结果**:
   - AI进行一般对话（不分析场景）
   - 提供grammar correction

## 新增逻辑说明

### 智能场景上下文判断
在Normal和Word模式下，系统会智能判断用户语音是否与场景相关：
- **场景相关关键词**: "what", "see", "this", "here", "picture", "image"
- **Scene模式**: 总是尝试分析场景
- **Normal/Word模式**: 只有在检测到场景相关关键词时才分析场景

### Grammar Correction触发条件
- **SendMessage方法**: Normal模式下自动触发
- **SendSceneRecognitionRequest方法**: Normal模式下且有用户输入时自动触发

## 调试信息

### 检查日志
1. **模式切换日志**:
   ```
   [LearningModeManager] Mode changed to: Normal
   ```

2. **语音输入处理日志**:
   ```
   Transcription received: [用户语音内容]
   [ChatManager] Current mode: Normal, providing grammar correction
   ```

3. **Grammar Correction日志**:
   ```
   [ChatManager] Grammar correction requested for: [用户输入]
   [ChatManager] Grammar correction completed
   ```

## 故障排除

### 如果Grammar Correction仍然不工作：
1. **检查LearningModeManager**: 确认当前模式确实是Normal
2. **检查ChatManager日志**: 确认`ProvideGrammarCorrection`方法被调用
3. **检查OpenAI响应**: 确认AI正确返回了格式化的语法纠错内容

### 如果场景分析不工作：
1. **检查BackgroundSceneMonitor**: 确认场景图像被正确捕获
2. **检查captureSceneOnAgentRecord**: 确认此选项已启用
3. **检查AgentButton状态**: 确认按钮处于活跃状态

## 总结

修复后，AgentButton在Normal模式下的行为应该是：
1. **智能路径选择**: 根据用户语音内容决定是否分析场景
2. **双重功能**: 既提供对话回复，又提供语法纠错
3. **模式感知**: 在不同模式下表现出不同的行为重点

这样确保了用户在任何模式下使用AgentButton都能获得一致且完整的学习体验。
