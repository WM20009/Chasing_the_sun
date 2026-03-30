# Chase the Sun Level 0 兜底方案与交接说明

## 当前建议

不要继续依赖 `Tuanjie.exe -batchmode` 做这套资产生成和场景初始化。

目前更稳的做法是：

1. 在编辑器里正常打开 `Chase_the_sun_level_0` 项目，等脚本编译完成。
2. 按小步骤执行菜单，而不是一次跑完整批处理。
3. 生成完正式资源后，再手动拖拽物体搭关卡。

## 推荐执行顺序

在编辑器顶部菜单里依次执行：

1. `Tools > Chase the Sun > Generate > Player Animations`
2. `Tools > Chase the Sun > Generate > Backdrop Prefabs`
3. `Tools > Chase the Sun > Generate > Gameplay Prefabs`
4. `Tools > Chase the Sun > Generate > Player Prefab`
5. `Tools > Chase the Sun > Scene > Refresh Scene Support`

如果你想一次生成，也可以继续用：

- `Tools > Chase the Sun > Generate All Assets`
- `Tools > Chase the Sun > Setup Active Scene`

但在当前项目状态下，优先推荐分步执行，方便定位是哪一步慢或出错。

## 搭图辅助菜单

为了减少手动返工，现在可以配合使用：

- `Tools > Chase the Sun > Fix Selection Sorting`
  - 会同时修正选中物体的推荐 `Layer`、`Sorting Layer` 和常用 `sortingOrder`
- `Tools > Chase the Sun > Move Selection To Recommended Root`
  - 会把选中物体移动到推荐的场景根节点，如 `Background`、`Gameplay`、`Hazards`、`Checkpoints`、`Foreground`
- `Tools > Chase the Sun > Snap Selection To 0.25 Grid`
  - 会把选中物体吸附到 0.25 单位网格

## 已完成的底层内容

下面这些内容已经写进项目里，不依赖 batchmode 才算存在：

- 角色控制、跳跃、推箱、攀爬、死亡、复活、相机、黑幕等运行时代码已经新增到 `Assets/Scripts`
- 编辑器生成器已经新增到 `Assets/Editor/ChaseTheSun/SceneAssetBuilder.cs`
- 项目 Layer 和 Sorting Layer 已经扩充：
  - `Player`
  - `Ground`
  - `Pushable`
  - `Climbable`
  - `Hazard`
  - `Checkpoint`
  - `Decoration`
  - `Background`
  - `Gameplay`
  - `Player`
  - `Foreground`
  - `Overlay`

## 当前真正的未完成项

还没有稳定落地的部分是：

- `Assets/Generated/ChaseTheSun` 下的正式 prefab / anim / controller 资产生成
- `SampleScene.scene` 的推荐根节点模板刷新
- 基于编辑器的最终手测

也就是说，现在卡住的是“自动生成资产这一步”，不是“底层逻辑全部白写了”。

## 是否有后效副作用

目前最需要注意的全局副作用只有一类：

- `ProjectSettings/TagManager.asset`
  - 这是全项目级配置
  - 它已经占用了 Layer 8 到 14，并新增了上述 Sorting Layer
  - 如果同一项目里别的场景之前已经在用这些 Layer 编号，就需要在交接时明确说明

其余改动基本都是增量式的：

- `Assets/Scripts` 下新增脚本
  - 只有当场景里挂上这些组件时才会参与运行
- `Assets/Editor` 下新增工具
  - 只影响编辑器菜单，不会进运行时包
- `Assets/Generated/ChaseTheSun` 下新增说明文档
  - 不影响运行

## 交接时务必说明的三件事

1. 正式资源来源应以 `Assets/Generated/ChaseTheSun` 为准，而不是直接从原始 `Assets/Scenes/Sprite` 拖到最终场景。
2. 当前不推荐继续用 batchmode 跑生成，优先在编辑器里用拆开的菜单一步一步生成。
3. `Palettes` 和 `Wall` 仍然是 legacy 原始目录，本轮没有删除，也不应再作为正式引用源。

## 最稳的协作方式

如果需要交给其他同学继续做，建议按下面方式交接：

1. 先让对方打开项目并确认脚本没有编译错误。
2. 让对方只用编辑器菜单分步生成，不跑 headless 批处理。
3. 让对方把实际摆进场景的物体都从 `Assets/Generated/ChaseTheSun` 下拖拽。
4. 每搭完一批物体，执行一次 `Fix Selection Sorting` 和 `Snap Selection To 0.25 Grid`。
5. 最后再做角色移动、跳跃、推箱、梯子、尖刺、断桥、营火和复活的整体验证。
