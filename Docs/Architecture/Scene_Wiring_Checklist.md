# Scene Wiring Checklist

## 1. Goal

Tai lieu nay la checklist thao tac trong Unity Editor de lap duoc `Run_Prototype` va viewer shell local dua tren toan bo code da scaffold.

## 2. Create Prototype Assets

Trong Unity Editor:

1. mo menu `Tools/SoulForge/Create Prototype Assets`
2. verify cac asset duoc tao tai:
   - `Assets/Game/ScriptableObjects/Heroes`
   - `Assets/Game/ScriptableObjects/Weapons`
   - `Assets/Game/ScriptableObjects/Enemies`
   - `Assets/Game/ScriptableObjects/Encounters`
   - `Assets/Game/ScriptableObjects/Rewards`
   - `Assets/Game/ScriptableObjects/Viewer`

## 3. Run_Prototype Scene

### Root objects

- `GameBootstrap`
- `RunController`
- `RunResetController`
- `ViewerRuntime`
- `UIRoot`
- `Player`
- `Room_A`
- `Room_B`
- `Room_C`

### GameBootstrap

- add [GameBootstrap.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Bootstrap/GameBootstrap.cs)

### RunController

- add [RunController.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Bootstrap/RunController.cs)
- drag room sequence:
  - `Room_A`
  - `Room_B`
  - `Room_C`
- drag:
  - `StateBroadcaster`
  - `PlayerHealth`
  - `ViewerActionQueue`
  - `ViewerRoomBudgetService`

### RunResetController

- add [RunResetController.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Bootstrap/RunResetController.cs)
- drag:
  - `RunController`
  - `PlayerHealth`
  - `ResultScreenPresenter`

## 4. Player Setup

### Player object

- `SpriteRenderer` hoac SPUM prefab root
- `Collider2D`
- optionally `Rigidbody2D` set kinematic
- add:
  - [PlayerController.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Player/PlayerController.cs)
  - [PlayerAim.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Player/PlayerAim.cs)
  - [PlayerHealth.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Player/PlayerHealth.cs)
  - [PlayerWeaponController.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Player/PlayerWeaponController.cs)
  - [WeaponRuntime.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Combat/WeaponRuntime.cs)

### Assign assets

- `PlayerController.heroDefinition` -> `Hero_Default`
- `PlayerAim.weaponDefinition` -> `Weapon_Pistol`
- `PlayerAim.projectilePrefab` -> projectile prefab
- `PlayerWeaponController.weaponRuntime` -> same object `WeaponRuntime`

## 5. Projectile Prefab

### Create prefab

- name: `Projectile_Player`
- add:
  - `SpriteRenderer`
  - `CircleCollider2D` set `Is Trigger = true`
  - [Projectile.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Combat/Projectile.cs)

### Enemy projectile

- duoc dung chung neu muon, hoac clone prefab rieng

## 6. Enemy Prefabs

### Weak enemy prefab

- add:
  - `SpriteRenderer`
  - `Collider2D`
  - [EnemyController.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Enemies/EnemyController.cs)

- assign:
  - `definition` -> `Enemy_Weak`
  - `projectilePrefab` -> enemy projectile prefab

### Elite enemy prefab

- clone weak enemy prefab
- doi `definition` -> `Enemy_Elite`

## 7. Room Setup

Moi room can:

- 1 object root co [RoomController.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Rooms/RoomController.cs)
- 1 object con co [EnemySpawner.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Enemies/EnemySpawner.cs)
- 0..n `DoorController`
- 1 `RewardSpawner`

### RoomController

- set `roomId`
- set `connectedDoors`
- drag `RewardSpawner`

### EnemySpawner

- set `ownerRoom` = room root
- them spawn entries:
  - `SpawnId = weak_spawn`
  - `Prefab = weak enemy prefab`
  - `Definition = Enemy_Weak`
  - `SpawnPoint = transform spawn point`
- them entry elite:
  - `SpawnId = elite_spawn`
  - `Prefab = elite enemy prefab`
  - `Definition = Enemy_Elite`

### RewardSpawner

- add [RewardSpawner.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Rooms/RewardSpawner.cs)
- add possible reward:
  - `Reward_Default`
- drag `RewardPanelPresenter`

## 8. Room Transitions

### A -> B

- tao trigger object o cua ra room A
- add [RoomTransitionTrigger.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Rooms/RoomTransitionTrigger.cs)
- set:
  - `sourceRoom = Room_A`
  - `targetRoom = Room_B`
  - `requireSourceRoomCleared = true`

### B -> C

- lap lai voi `Room_B` -> `Room_C`

### Final gate

- tao trigger object o room C
- add [RunFinishGate.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Rooms/RunFinishGate.cs)
- set:
  - `ownerRoom = Room_C`

### Boss signal

- tren room C add [BossRoomSignal.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Rooms/BossRoomSignal.cs)
- set:
  - `ownerRoom = Room_C`
  - `bossName = Forge Sentinel`

## 9. Viewer Runtime Object

Tao object `ViewerRuntime` va add:

- [ViewerSessionService.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Viewer/ViewerSessionService.cs)
- [ViewerEconomyService.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Economy/ViewerEconomyService.cs)
- [ViewerRoomBudgetService.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Viewer/ViewerRoomBudgetService.cs)
- [ViewerActionValidator.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Viewer/ViewerActionValidator.cs)
- [ViewerActionQueue.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Viewer/ViewerActionQueue.cs)
- [ViewerActionExecutor.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Viewer/ViewerActionExecutor.cs)
- [StateBroadcaster.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Viewer/StateBroadcaster.cs)
- [LocalViewerCommandTester.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/Viewer/LocalViewerCommandTester.cs)

### Assign runtime assets

- `ViewerEconomyService.config` -> `ViewerEconomyConfig`
- `ViewerRoomBudgetService.config` -> `ViewerEconomyConfig`
- `ViewerActionValidator.storeCatalog` -> `ViewerStoreCatalog`
- `ViewerActionValidator.economyService` -> `ViewerEconomyService`
- `ViewerActionValidator.roomBudgetService` -> `ViewerRoomBudgetService`
- `ViewerActionQueue.config` -> `ViewerEconomyConfig`
- `ViewerActionExecutor.validator` -> `ViewerActionValidator`
- `ViewerActionExecutor.queue` -> `ViewerActionQueue`
- `ViewerActionExecutor.economyService` -> `ViewerEconomyService`
- `ViewerActionExecutor.broadcaster` -> `StateBroadcaster`
- `ViewerActionExecutor.enemySpawner` -> room spawner hien hanh hoac spawner tong
- `ViewerActionExecutor.playerHealth` -> player
- `ViewerActionExecutor.playerWeaponController` -> player
- `ViewerActionExecutor.fallbackViewerWeapon` -> `Weapon_ViewerDrop`
- `LocalViewerCommandTester.executor` -> `ViewerActionExecutor`
- `LocalViewerCommandTester.economyService` -> `ViewerEconomyService`

## 10. UI Root

Tao `Canvas` + `EventSystem`

### Host UI

- `HUDPanel`
  - add [HudPresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/HudPresenter.cs)
- `RewardPanel`
  - add [RewardPanelPresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/RewardPanelPresenter.cs)
- `RunProgress`
  - add [RunProgressPresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/RunProgressPresenter.cs)
  - drag TMP text vao `progressText`
- `ResultScreen`
  - add [ResultScreenPresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/ResultScreenPresenter.cs)
  - add [ResultScreenButtonsPresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/ResultScreenButtonsPresenter.cs)
  - create:
    - title TMP
    - summary TMP
    - restart button
    - hub button

### Viewer UI

- `ViewerShellUI`
  - add [ViewerHudPresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/ViewerHudPresenter.cs)
  - add [ViewerStateFeed.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/ViewerStateFeed.cs)
  - add [ViewerEconomyPresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/ViewerEconomyPresenter.cs)
  - add [ViewerControlsPresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/ViewerControlsPresenter.cs)
  - add [ViewerActionHistoryPresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/ViewerActionHistoryPresenter.cs)
  - add [ViewerStorePresenter.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/ViewerStorePresenter.cs)

### Viewer texts

Tao TMP text cho:

- snapshot
- delta
- action result
- raw json
- balance
- budget
- controls
- history
- store header

### Viewer store buttons

Tao 4 button.

Moi button can:

- `Button`
- `Image`
- [ViewerStoreButton.cs](/d:/SPUM-master/SPUM-master/Assets/Game/Scripts/UI/ViewerStoreButton.cs)
- 3 TMP text con:
  - label
  - price
  - state

Drag 4 button vao `ViewerStorePresenter.actionButtons`.

## 11. Local Test Controls

- `0` = cong currency
- `1` = spawn weak enemy
- `2` = spawn elite enemy
- `3` = drop heal
- `4` = drop random weapon

Hoac bam button trong viewer store UI.

## 12. Success Criteria

Prototype duoc coi la lap thanh cong khi:

- player di chuyen va ban duoc
- room A clear xong sang duoc room B
- viewer bam button duoc va state UI thay doi
- budget giam sau khi viewer mua action
- cooldown button hien dung
- room cuoi vao duoc finish gate
- result screen hien sau khi complete run hoac player death
