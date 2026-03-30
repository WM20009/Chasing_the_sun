# Generated 目录说明

`Assets/Generated/ChaseTheSun` 现在只保留自动生成的参考资产。

请不要再把这里的 prefab、anim、controller 当作最终交付资产。

正式手工调好的最终资产请放到：

- `Assets/Manual/ChaseTheSun/Animations`
- `Assets/Manual/ChaseTheSun/Controllers`
- `Assets/Manual/ChaseTheSun/Prefabs/Background`
- `Assets/Manual/ChaseTheSun/Prefabs/Foreground`
- `Assets/Manual/ChaseTheSun/Prefabs/Gameplay`
- `Assets/Manual/ChaseTheSun/Prefabs/Player`

当前自动生成器已经做了两项参考修正：

- 参考 `Idle.anim` 改为单帧 `push1_0`
- 参考玩家 prefab 的胶囊体默认值改为 `size = (0.75, 1.75)`、`offset = (0, 0.875)`

但平台、箱子、梯子、尖刺、营火等 collider 仍然建议手工制作，不要继续依赖自动包边。
