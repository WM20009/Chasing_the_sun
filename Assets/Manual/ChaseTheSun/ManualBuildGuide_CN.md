# Chase the Sun Level 0 手工搭建指南

## 1. 先建正式手工目录

把最终资产放到下面这些目录，不要继续把 `Generated` 当正式来源：

- `Assets/Manual/ChaseTheSun/Animations`
- `Assets/Manual/ChaseTheSun/Controllers`
- `Assets/Manual/ChaseTheSun/Prefabs/Background`
- `Assets/Manual/ChaseTheSun/Prefabs/Foreground`
- `Assets/Manual/ChaseTheSun/Prefabs/Gameplay`
- `Assets/Manual/ChaseTheSun/Prefabs/Player`

## 2. 统一玩法尺寸

固定标准：

- `1 格 = 1 Unity unit`
- 所有可站立平台默认 `1 格高`
- 角色玩法高度 `1.75 格`
- 箱子玩法尺寸 `2 x 2 格`

推荐 root 规则：

- 每个 prefab 用一个 `Root` 作为玩法锚点
- `Root` 只负责 `Collider2D`、`Rigidbody2D`、脚本、Layer
- `Visual` 作为子物体，只负责 `SpriteRenderer`、`Animator`
- `Root.localScale = (1, 1, 1)`
- 只允许调 `Visual.localPosition` 和 `Visual.localScale`

## 3. 平台与场景物件

### Medium_Ground

- 玩法尺寸：`3 x 1`
- `BoxCollider2D.size = (3, 1)`
- `BoxCollider2D.offset = (0, 0.5)`
- `Layer = Ground`
- `Sorting Layer = Gameplay`

### Platform2_Ground / Platform3_Ground

- 玩法尺寸：`4 x 1`
- `BoxCollider2D.size = (4, 1)`
- `BoxCollider2D.offset = (0, 0.5)`
- `Layer = Ground`
- `Sorting Layer = Gameplay`

### Wall_Blocker / Gravestone_Blocker

- 不按整张图包边
- 用 `1~3` 个 `BoxCollider2D` 拼出真正阻挡角色的实体部分
- `Layer = Ground`

### Bridge_Ground

- 只做桥面的站立面
- 推荐 `BoxCollider2D.size = (4, 0.5)`
- 推荐 `BoxCollider2D.offset = (0, 0.25)`

### BrokenBridge_Hazard

- 左右各一段站立 `BoxCollider2D`
- 中间单独建子物体 `GapHazard`
- `GapHazard` 上挂 `HazardZone`
- `GapHazard` 的 `BoxCollider2D.isTrigger = true`

### Ladder_Climbable

- 只给中间可攀爬的区域做 trigger
- 不把顶部装饰和外边框算进可吸附区域
- 推荐从底到顶做一条窄竖条
- `Layer = Climbable`

### Spikes_Hazard

- 只给尖端一条薄 trigger
- 推荐高度 `0.25 ~ 0.35`
- 不拿整张图当危险区
- `Layer = Hazard`

### Campfire_Checkpoint

- 只给火堆底部接触区做 trigger
- 复活点靠 `spawnOffset` 单独调
- 不拿整张火焰图当触发区
- `Layer = Checkpoint`

### Box_Pushable / BrokenBox_Pushable

- 玩法尺寸：`2 x 2`
- `BoxCollider2D.size = (2, 2)`
- `BoxCollider2D.offset = (0, 1)`
- `Rigidbody2D.gravityScale = 3`
- `Freeze Rotation = true`
- `Interpolation = Interpolate`
- `Collision Detection = Continuous`
- `Layer = Pushable`

## 4. 玩家 prefab

新建 `CTS_Player`：

- 根物体放在脚底中心
- 加 `Rigidbody2D`
- 加 `CapsuleCollider2D`
- 加 `PlayerAnimationDriver`
- 加 `PlayerController2D`

角色碰撞标准：

- `CapsuleCollider2D.direction = Vertical`
- `CapsuleCollider2D.size = (0.75, 1.75)`
- `CapsuleCollider2D.offset = (0, 0.875)`

角色刚体建议：

- `Gravity Scale = 4`
- `Freeze Rotation = true`
- `Interpolation = Interpolate`
- `Collision Detection = Continuous`

玩家检测参数：

- `groundCheckSize = (0.68, 0.10)`
- `groundCheckDistance = 0.08`
- `pushCheckSize = (0.30, 1.00)`
- `pushCheckOffset = (0.55, 0.85)`

视觉子物体 `Visual`：

- 挂 `SpriteRenderer`
- 挂 `Animator`
- 把 `push1_0` 作为初始图片
- 把 `Visual` 往上移，直到脚掌刚好踩在 `y = 0`

## 5. 动画与 Animator

在 `Assets/Manual/ChaseTheSun/Animations` 手工建立：

- `Idle.anim`
- `Run.anim`
- `PushRight.anim`
- `PushLeft.anim`
- `Climb.anim`
- `JumpStart.anim`
- `JumpRise.anim`
- `JumpApex.anim`
- `JumpFall.anim`
- `Land.anim`
- `Dead.anim`

规则：

- `Idle` 只允许 1 帧：`push1_0`
- `Run` 用 `running`
- `PushRight` 用 `push`
- `PushLeft` 用 `push_left`
- `Climb` 用 `climb1`
- `JumpStart` 用 `jumping_0`、`jumping_1`
- `JumpRise` 用 `jumping_2`
- `JumpApex` 用 `jumping_3`
- `JumpFall` 用 `jumping_4`
- `Land` 用 `jumping_5`
- `Dead` 可以先临时复用 `jumping_4`

Animator Controller：

- 状态名必须保持和代码一致：
  - `Idle`
  - `Run`
  - `PushRight`
  - `PushLeft`
  - `JumpStart`
  - `JumpRise`
  - `JumpApex`
  - `JumpFall`
  - `Land`
  - `Climb`
  - `Dead`

## 6. 最小测试场景

场景里只摆：

- `1` 个 `Medium_Ground`
- `1` 个 `CTS_Player`
- `1` 个 `Box_Pushable`
- `1` 个 `Ladder_Climbable`
- `1` 个 `Spikes_Hazard`

测试顺序：

1. 角色站在平台上不下沉、不漂浮
2. 静止时保持单帧 `Idle`
3. `A / D` 左右移动不穿地
4. `Space` 只有落地时能稳定起跳
5. 推箱时进入推箱状态，松开后回 `Idle / Run`
6. 梯子中间区域可吸附，左右可以离开
7. 尖刺只在尖端致死

## 7. 工作纪律

- 一旦开始启用手工目录，就不要再把场景引用回 `Assets/Generated/ChaseTheSun`
- `Generate All` 之后可以把结果当参考，但不要把它当最终资产
- 所有最终交付 prefab / anim / controller 一律放回 `Assets/Manual/ChaseTheSun`
