# Chase the Sun Level 0 使用说明

## 一键生成
1. 打开 `Chase_the_sun_level_0` 工程，等待 Unity / 团结编辑器完成脚本编译与资源导入。
2. 在菜单栏执行 `Tools > Chase the Sun > Generate All Assets`。
3. 生成结果会放到 `Assets/Generated/ChaseTheSun`：
   - `Animations`：角色正式动画
   - `Controllers`：角色 Animator Controller
   - `Prefabs/Background`：背景 prefab
   - `Prefabs/Foreground`：前景 prefab
   - `Prefabs/Gameplay`：地形、机关、箱子、梯子、营火等 prefab
   - `Prefabs/Player`：正式玩家 prefab

## 场景初始化
1. 打开你们要搭建的场景。
2. 执行 `Tools > Chase the Sun > Setup Active Scene`。
3. 这会自动创建推荐根节点：
   - `Background`
   - `Gameplay`
   - `Checkpoints`
   - `Hazards`
   - `Foreground`
   - `Spawn`
   - `CameraBounds`
   - `UIOverlay`
4. 同时会补齐：
   - 黑屏淡入淡出 UI
   - `RespawnManager`
   - 默认出生点 `Spawn/PlayerSpawn`
   - 相机跟随组件
   - 相机边界框

## 推荐搭图方式
1. 把 `Night_Background.prefab` 拖到 `Background`。
2. 把所有可站立地形拖到 `Gameplay`：
   - `Medium_Ground`
   - `Platform2_Ground`
   - `Platform3_Ground`
   - `Wall_Blocker`
   - `Bridge_Ground`
   - `BrokenBridge_Hazard`
   - `Gravestone_Blocker`
3. 把机关和互动物拖到对应层：
   - `Spikes_Hazard` 放到 `Hazards`
   - `Campfire_Checkpoint` 放到 `Checkpoints`
   - `Ladder_Climbable` 放到 `Gameplay`
   - `Box_Pushable` / `BrokenBox_Pushable` 放到 `Gameplay`
4. 把 `TreeTrunk_Foreground.prefab` 拖到 `Foreground`。
5. 把 `CTS_Player.prefab` 拖到场景里，并把它放到 `Spawn/PlayerSpawn` 附近或者你们想要的起点。
6. 运行前，把 `Spawn/PlayerSpawn` 调到你们想要的默认出生位置。

## 角色与玩法默认规则
- `A / D`：左右移动
- `Space`：跳跃
- `W / S`：上/下爬梯子
- 接近箱子会自动切推箱动作，推箱速度更慢
- 接近梯子并重叠足够时会自动吸附
- 接触营火会自动更新复活点
- 接触尖刺或断桥中间掉落区会黑屏并复活
- 死亡后可推动箱子会重置到初始位置

## 可手动微调的重点参数
- 角色移动、跳跃、推箱、爬梯速度：
  `CTS_Player` 上的 `PlayerController2D`
- 黑屏速度：
  `RespawnManager`
- 相机平滑与边界：
  `Main Camera` 上的 `CameraFollow2D` 与 `CameraBounds`
- 营火复活位置：
  `Campfire_Checkpoint` 的 `spawnOffset`

## 辅助菜单
- `Tools > Chase the Sun > Fix Selection Sorting`
  用来把选中物体的 Sorting Layer 修正到推荐层。
- `Tools > Chase the Sun > Snap Selection To 0.25 Grid`
  用来把选中的物体吸附到 0.25 单位网格，减少手动摆放误差。

## 当前限制
- 旧目录 `Palettes` 和 `Wall` 作为 legacy 原始资源保留，不参与正式生成链。
- 正式 prefab 和动画以 `Assets/Generated/ChaseTheSun` 下的资源为准。
- 如果你们后续新增了素材，建议扩展 `SceneAssetBuilder.cs` 再重新执行生成菜单。
