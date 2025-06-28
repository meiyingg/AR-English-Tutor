# AR English Tutor 成就系统介绍

## 系统概述
成就系统是一个激励用户学习英语的游戏化功能，通过完成各种学习活动来解锁成就并获得经验值奖励。

## 成就分类（共15个成就）

### 1. 对话类成就 (Conversation)
- **Ice Breaker** - 发送第一条消息 (+10 EXP)
- **Chat Master** - 与AI导师进行10次对话 (+25 EXP)  
- **Conversation King** - 总共发送50条消息 (+50 EXP)

### 2. 学习类成就 (Learning)
- **First Steps** - 完成第一次学习会话 (+15 EXP)
- **Dedicated Learner** - 完成10次学习会话 (+40 EXP)
- **Scholar** - 完成50次学习会话 (+100 EXP)

### 3. 经验类成就 (Experience)
- **EXP Collector** - 获得100总经验值 (+20 EXP)
- **EXP Master** - 获得500总经验值 (+75 EXP)
- **EXP Legend** - 获得1000总经验值 (+150 EXP)

### 4. 等级类成就 (Level)
- **Rising Star** - 达到等级3 (+30 EXP)
- **Intermediate** - 达到等级5 (+60 EXP)
- **Advanced Learner** - 达到等级10 (+120 EXP)

### 5. 特殊类成就 (Special)
- **Early Bird** - 上午9点前完成学习 (+25 EXP)
- **Night Owl** - 晚上9点后完成学习 (+25 EXP)
- **Weekend Warrior** - 周末学习 (+20 EXP)

## 显示界面

### 成就状态标识
- `[DONE]` - 已解锁的成就
- `[....]` - 进行中的成就（有部分进度）
- `[LOCK]` - 未开始的成就

### 界面布局
```
*** ACHIEVEMENTS ***

UNLOCKED:
[DONE] Ice Breaker
       Send your first message to start learning!
       Unlocked: 2025-06-28 10:30

IN PROGRESS:
[....] Chat Master
       Have 10 conversations with your AI tutor
       Progress: 3/10

UPCOMING GOALS:
[LOCK] EXP Collector
       Earn 100 total experience points
       Goal: 100 | Reward: +20 EXP

PROGRESS: 1/15 achievements unlocked
```

## 交互功能

### 成就按钮
- **点击一次** - 显示成就面板
- **再点击一次** - 关闭成就面板
- **自动关闭** - 升级/成就解锁通知会自动关闭

### 解锁通知
当解锁成就时会弹出通知：
```
*** ACHIEVEMENT UNLOCKED! ***

Ice Breaker

Send your first message to start learning!

Reward: +10 EXP
```

## 技术特点

### 自动触发
- **发送消息时** - 自动检查对话类成就
- **获得经验时** - 自动检查经验类成就
- **升级时** - 自动检查等级类成就
- **完成会话时** - 自动检查学习类成就

### 数据持久化
- 使用PlayerPrefs保存成就进度
- 游戏重启后保持解锁状态
- 自动合并新增成就

### 奖励机制
- 解锁成就时获得额外EXP奖励
- 奖励范围：10-150 EXP
- 奖励会加入到用户总经验值中

## 设计理念
1. **简单易懂** - 不使用复杂的emoji，支持所有字体
2. **即时反馈** - 用户行为立即触发成就检查
3. **渐进激励** - 从简单到复杂，逐步引导用户深入学习
4. **可视化进度** - 清晰显示进度状态，激发用户完成欲望

这个系统设计为无干扰、自然融入学习过程，让用户在不知不觉中被激励继续学习！
