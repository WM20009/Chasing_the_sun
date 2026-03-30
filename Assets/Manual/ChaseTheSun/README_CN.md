# Chase the Sun Level 0 手工资产目录

这个目录现在是教学关的正式手工资产目录。

目录规则：

- `Assets/Scenes/Sprite` 只保留原始美术，不直接拖进最终场景
- `Assets/Generated/ChaseTheSun` 只保留自动生成的参考资产，不作为最终交付来源
- `Assets/Manual/ChaseTheSun` 下的 prefab、anim、controller 才是最终手工调好的正式资产

推荐结构：

- `Animations`
- `Controllers`
- `Prefabs/Background`
- `Prefabs/Foreground`
- `Prefabs/Gameplay`
- `Prefabs/Player`

统一标准：

- `1 格 = 1 Unity unit`
- 地图摆放可以继续使用 `0.25` 子网格微调
- 所有玩法 collider 以整数格尺寸为准，不按图片边框自动包

角色与箱体标准：

- 角色玩法高度：`1.75 格`
- 角色 `CapsuleCollider2D.size = (0.75, 1.75)`
- 角色 `CapsuleCollider2D.offset = (0, 0.875)`
- 箱子玩法尺寸：`2 x 2 格`
- 箱子 `BoxCollider2D.size = (2, 2)`
- 箱子 `BoxCollider2D.offset = (0, 1)`

动画标准：

- `Idle` 必须是单帧 `push1_0`
- `Run` 使用 `running`
- `PushRight` 使用 `push`
- `PushLeft` 使用 `push_left`
- `Climb` 使用 `climb1`
- 跳跃阶段保持现有状态名，不改代码枚举

详细操作流程见：

- `Assets/Manual/ChaseTheSun/ManualBuildGuide_CN.md`
