# GDD: Project Baseline + Soul Knight-like Interactive Stream Blueprint

## 1. Project Scout Summary

### Technical baseline

- Unity version: `6000.2.12f1`
- Render pipeline: Built-in Render Pipeline (`m_CustomRenderPipeline: {fileID: 0}`)
- Packages dang chu y:
  - `com.unity.ugui`
  - `com.unity.timeline`
  - `com.unity.ai.navigation`
  - `com.unity.test-framework`
  - `com.besty.unity-skills`
- Scene hien co:
  - `Assets/SPUM/Scene/SPUM_Scene.unity`
  - `Assets/SPUM/Sample/Scene/Sample.unity`
- Script runtime/custom trong `Assets/` rat it, chu yeu la:
  - `SPUM_Manager`
  - `SPUM_Prefabs`
  - `SPUM_SpriteList`
  - sample scripts `PlayerManager`, `PlayerObj`

### Existing architectural signals

- Project hien tai la sample/content project cua `SPUM`, chua phai gameplay project hoan chinh.
- Chua co `asmdef` rieng trong `Assets/` cho code game.
- Chua thay gameplay systems dang combat loop, room flow, inventory, progression.
- Co dung:
  - `Resources.Load` de nap prefab/sprite
  - `MonoBehaviour` truc tiep cho flow runtime
  - custom editor cho phan SPUM authoring
- Chua thay test rieng cua game.

### Existing conventions worth preserving

- Giu Unity workflow don gian, tranh dua framework lon vao tu dau.
- Tan dung `SPUM` de author prefab nhan vat, enemy, NPC.
- Uu tien top-down 2D dung sprite/prefab thay vi mo rong sang 3D systems.

### Risks / constraints

- Repo gan nhu chua co nen gameplay, nen moi de xuat nen la vertical slice nho.
- `Resources` dang duoc dung; neu muon ship nhanh co the chap nhan o phase dau, chua can Addressables.
- `PlayerObj` sample dang thien ve point-and-click movement; voi Soul Knight-like can doi sang twin-stick hoac keyboard/mouse top-down shooter.

### Unknowns

- Chua ro target platform chinh: PC truoc hay mobile truoc.
- Chua ro art scope ngoai asset SPUM.
- Chua ro user muon roguelite nhe hay clone loop sat Soul Knight.

## 2. Game Vision

### Working title

`Soul Forge Protocol`

### High concept

Mot game top-down action roguelite 2D lay cam hung tu Soul Knight, nhung co them che do interactive stream: 1 host choi binh thuong, viewer chi xem va tac dong vao run bang cach mua item, buff, debuff hoac spawn quai.

### Design goals

- Combat nhanh, doc tinh huong ro, vao game trong duoi 10 giay.
- Moi run ngan: 12-20 phut.
- Quyet dinh build de hieu: vu khi, passive relic, skill nhan vat.
- Viewer phai tac dong duoc vao tran dau ma khong pha vo authority cua host.
- Scope vua voi repo hien tai: 1 host player, 1 viewer flow co ban, 3-4 enemy types, 1 boss, 1 biome dau tien.

### Pillars

1. Di chuyen va ban phai da tay ngay ca khi content con it.
2. Moi phong la mot bai kiem tra vi tri, ne dan va uu tien muc tieu.
3. Phan thuong sau phong phai du ro de tao build variation.
4. Viewer interaction phai de hieu, co gia tien va co phan hoi nhanh.
5. Kien truc phai du nho de lam vertical slice nhanh.

## 3. Core Loop

1. Vao hub nho.
2. Chon hero/loadout khoi dau.
3. Enter run.
4. Don tung combat room.
5. Nhan reward: weapon, relic, heal hoac currency.
6. Mo sang room tiep theo.
7. Danh boss cuoi tang.
8. Nhan meta currency va quay lai hub.

### Moment-to-moment loop

1. Move.
2. Aim.
3. Shoot / use active skill / dodge.
4. Tranh projectile va giu vi tri.
5. Nhat ammo, heal, buff, weapon drop.

### Host-viewer loop

1. Host tao session va vao run.
2. Viewer vao bang app viewer hoac web viewer.
3. Viewer xem state toi thieu cua tran dau.
4. Viewer dung currency de mua action.
5. Host validate action, dua vao queue va resolve trong combat.
6. Ket qua duoc broadcast lai cho host va viewer.

## 4. Scope For First Vertical Slice

### In scope

- 1 playable hero
- 1 host player session
- 1 viewer session flow
- 2 weapon archetypes:
  - pistol ban tu dong
  - shotgun tam ngan
- 1 active skill:
  - dash i-frame ngan
- 3 enemy archetypes:
  - chaser melee
  - ranged shooter
  - tank cham
- 1 mini boss hoac 1 boss don gian
- 6-8 rooms
- 1 tileset dungeon don gian
- UI toi thieu:
  - HP
  - shield/armor
  - ammo hoac heat
  - minimap don gian
  - reward panel
- viewer actions dau tien:
  - spawn weak enemy
  - spawn elite enemy
  - drop heal
  - drop random weapon

### Out of scope for slice 1

- online co-op giua nhieu host player
- procedural generation phuc tap
- class roster lon
- crafting sau
- weapon rarity system qua nhieu tang
- cinematic/timeline-heavy flow
- auction, vote wars, streamer economy phuc tap

## 5. Player Design

### Base stats

- HP: 6
- Shield/Armor: 4
- Move Speed: trung binh-cao
- Dash cooldown: 4 giay
- Default weapon: pistol vo han ammo hoac ammo regen nhe

### Controls

#### PC-first recommendation

- `WASD`: move
- mouse: aim
- left click: fire
- `Space`: dash
- `E`: interact / pick weapon
- `Q`: drop/swap weapon

### Combat feel targets

- Player phai doi huong aim doc lap voi huong di chuyen.
- Dash dung de pha tinh huong bi kep, khong phai spam mobility.
- Bullet pattern phai doc duoc bang mat, khong qua noisy.

## 6. Enemy Design

### Enemy archetypes

#### 1. Slime Raider

- lao vao can chien
- tao ap luc vi tri
- HP thap, spawn dong

#### 2. Bone Gunner

- dung giu khoang cach
- ban burst 3 vien
- buoc player uu tien xu ly ranged threat

#### 3. Iron Brute

- di chuyen cham
- mau cao
- ban projectile to hoac shockwave ngan

### Boss concept

`Forge Sentinel`

- phase 1: ban quat dan + summon adds
- phase 2: charge theo truc + vong dan
- reward: chest lon, meta currency, unlock weapon pool

## 7. Weapon and Build Design

### Weapon slots

- 2 weapon slots
- 1 active skill co dinh theo hero

### Weapon archetypes for early production

- Pistol: on dinh, DPS vua
- Shotgun: burst cao, range ngan
- SMG: spray control thap, clear room tot
- Rail shot prototype: charge ngan, xuyen muc tieu

### Relic categories

- offensive:
  - tang crit
  - tang projectile count
  - burn / poison / chain hit
- defensive:
  - tang shield
  - khien hoi sau X giay khong nhan damage
  - dash de lai decoy
- utility:
  - hut currency
  - tang move speed
  - mo chest re hon

### Build rule

- Reward nen co synergy ro trong 1 cau.
- Moi relic nen thay doi quyet dinh choi, khong chi +5% stat vo nghia.

## 8. Room and Run Structure

### Room types

- combat room
- elite room
- treasure room
- shop room
- rest room
- boss room

### Flow for slice 1

- layout ban thu cong
- graph don gian:
  - start
  - 4 combat rooms
  - 1 treasure/shop branch
  - 1 elite
  - 1 boss

### Encounter rules

- Chi khoa cua khi con enemy song.
- Reward spawn sau khi clear room.
- Room nen co cover don gian: pillar, crate, wall corner.

## 9. Meta Progression

### Run rewards

`Soul Cores` dung unlock hero passive, weapon unlock, hub upgrades.

### Early meta layer

- unlock weapon moi
- +1 lua chon reward sau elite room
- unlock hero thu hai

### Keep simple now

- Khong can talent tree lon.
- Chi can 8-12 unlocks dau game la du cho vertical slice mo rong.

## 10. UX / UI

### Screens

- Boot
- Main Menu
- Hub
- Run HUD
- Viewer Lobby
- Viewer Store
- Reward Select
- Pause
- Result Screen

### HUD responsibilities

- HP / Shield
- current weapon + ammo
- skill cooldown
- minimap don gian
- currency run hien tai

### Viewer UI responsibilities

- current room
- host HP / shield
- combat phase status
- item store / spawn store
- cooldown va gia cua moi action
- action history ngan

### UX principles

- thong tin chien dau phai o goc nhin trung tam hoac goc duoi de doc
- reward panel hien thi ngan, mot dong effect chinh
- weapon compare phai co delta ro

## 11. Audio / Visual Direction

### Art direction

- pixel-ish 2D top-down, giu tuong thich voi SPUM sprites
- moi truong dungeon fantasy-tech nhe
- effect dan sang, silhouette ro

### VFX priorities

- muzzle flash
- hit flash
- enemy death burst
- dash trail
- room clear pulse

## 12. Recommended Project Tier

`small-game vertical slice`

Khong nen xay framework tong quat hoa som. Muc tieu dung la dung gameplay foundation du sach de mo rong sau khi combat loop da chung minh duoc fun, dong thoi bo sung 1 lop viewer interaction nho va authority ro rang.

## 13. Recommended Modules

### 1. Bootstrap

- vao scene, tao run session, wire references, load config

### 2. Player

- input, movement, aim, shoot, dash, hurt, weapon swap

### 3. Combat

- damage, hitbox/hurtbox, projectile, status effect toi thieu

### 4. Enemies

- enemy brain don gian, spawn rules, death reward

### 5. Rooms / Run Flow

- room state, door lock/unlock, encounter clear, reward progression

### 6. Viewer Interaction

- nhan lenh viewer, validate, queue, execute, broadcast ket qua

### 7. Economy / Commerce

- viewer currency, gia item, cooldown, room budget, purchase rules

### 8. Data / Config

- `ScriptableObject` cho weapon, enemy, room, reward tables

### 9. UI

- HUD host, reward panel, viewer store, result flow

## 14. Scene / Bootstrap Plan

### Recommended scenes

- `Bootstrap`
  - scene cuc mong de vao menu hoac hub
- `Hub`
  - hero select, unlocks, start run
- `Run_Prototype`
  - dungeon vertical slice
- `Viewer_Client`
  - app viewer hoac web-viewer shell de xem va mua action

### Composition rule

- Scene chi chua:
  - camera
  - map root
  - spawn points
  - UI root
  - bootstrap references
- Gameplay state khong nen rai lung tung trong scene object khong ro ownership.

## 15. Data Ownership

### Scene objects

- map geometry
- enemy spawn markers
- door references
- camera bounds
- viewer interaction anchors neu can show marker trong room

### ScriptableObjects

- `HeroDefinition`
- `WeaponDefinition`
- `EnemyDefinition`
- `RoomEncounterDefinition`
- `RewardDefinition`
- `RunBalanceConfig`
- `ViewerActionDefinition`
- `ViewerStoreCatalog`

### Pure C# classes

- damage payload
- reward roll logic
- run progression state
- stat modifier aggregation
- viewer command payload
- purchase validation result
- room action budget tracker

### MonoBehaviours

- bridge giua scene/prefab va pure logic
- movement, collision callbacks, animation playback, VFX hooks

## 16. Communication Rules

- Direct references cho object trong cung prefab hoac cung room.
- C# events hoac UnityAction cho:
  - room cleared
  - player damaged
  - weapon changed
  - reward selected
- Viewer command khong duoc chay truc tiep vao gameplay object.
- Moi viewer action phai di qua:
  - validate
  - queue
  - execute
  - broadcast
- Host la authority duy nhat cho:
  - HP
  - enemy spawn
  - item drop
  - reward resolution
- Khong nen dung singleton toan cuc tran lan.
- Neu can 1 service nho cho slice 1, chi giu:
  - `RunContext`
  - `AudioRouter`
  - `ViewerSessionService`

## 17. Initial Script Inventory

### Bootstrap / flow

- `GameBootstrap`
- `HubController`
- `RunController`
- `RoomController`
- `DoorController`
- `ViewerClientBootstrap`

### Player

- `PlayerController`
- `PlayerAim`
- `PlayerHealth`
- `PlayerDash`
- `PlayerWeaponController`

### Combat

- `Projectile`
- `HitReceiver`
- `DamageDealer`
- `WeaponRuntime`

### Enemies

- `EnemyController`
- `EnemyMovement`
- `EnemyAttack`
- `EnemyHealth`
- `EnemySpawner`

### Viewer / economy

- `ViewerSessionService`
- `ViewerCommandReceiver`
- `ViewerActionValidator`
- `ViewerActionQueue`
- `ViewerActionExecutor`
- `ViewerEconomyService`
- `StateBroadcaster`
- `ViewerStorePresenter`

### UI

- `HudPresenter`
- `RewardPanelPresenter`
- `ResultScreenPresenter`
- `ViewerHudPresenter`

### Data

- `HeroDefinition`
- `WeaponDefinition`
- `EnemyDefinition`
- `EncounterDefinition`
- `RewardDefinition`
- `ViewerActionDefinition`
- `ViewerStoreCatalog`

## 18. Folder Structure Recommendation

```text
Assets/
  Game/
    Art/
    Audio/
    Prefabs/
      Characters/
      Enemies/
      Weapons/
      UI/
    Scenes/
      Bootstrap.unity
      Hub.unity
      Run_Prototype.unity
      Viewer_Client.unity
    Scripts/
      Bootstrap/
      Combat/
      Data/
      Economy/
      Enemies/
      Player/
      Rooms/
      UI/
      Viewer/
    ScriptableObjects/
      Heroes/
      Weapons/
      Enemies/
      Encounters/
      Rewards/
      Viewer/
```

## 19. Performance Risks

- Projectile count tang nhanh neu khong pool object.
- `Update()` cho moi enemy/projectile co the on o slice dau, nhung nen pool projectile som.
- Tranh `Resources.Load` trong combat runtime; chi load khi boot hoac prewarm.
- UI reward/minimap khong can toi uu som, nhung projectile va enemy spawn thi co.
- Viewer spam command se lam host lag neu khong co queue va budget.
- Broadcast qua nhieu state thua se tang chi phi network va latency.

## 20. Host-Viewer Rules

### Authority rules

- Host la nguon state duy nhat.
- Viewer khong dieu khien player, khong so huu enemy, khong trigger gameplay truc tiep.
- Viewer chi gui command co gia tien va host moi co quyen thuc thi.

### Allowed viewer actions for slice 1

- spawn weak enemy
- spawn elite enemy
- drop heal
- drop random weapon

### Safety rules

- moi 3-5 giay chi resolve mot so action gioi han
- moi room co spawn budget toi da
- khong spawn them khi dang boss intro hoac reward phase
- elite spawn chi mo sau room thu N
- item ho tro va item pha host phai nam o 2 pool gia khac nhau

## 21. Do Now / Skip Now

### Do now

- build `Run_Prototype` scene
- lam player movement + aim + shoot + dash
- lam 3 enemy types
- lam 1 room clear flow + reward select
- author 2-4 SPUM-based hero/enemy prefabs
- dung `ScriptableObject` cho weapon/enemy config ngay tu dau
- them `ViewerActionDefinition` va store catalog
- them host authority flow cho viewer command

### Skip now

- ECS
- event bus tong quat
- save system phuc tap
- procedural dungeon hoan toan random
- network/co-op
- inventory grid lon
- viewer chat integration phuc tap
- realtime auction, prediction, replay system

## 22. Milestone Plan

### Milestone 1: Combat box

- player move/aim/shoot
- 1 enemy ranged
- projectile collision
- HP / death / restart

### Milestone 2: Room loop

- room lock/unlock
- 3 enemy archetypes
- clear reward
- weapon swap

### Milestone 3: Viewer interaction MVP

- viewer join session
- 4 viewer actions dau tien
- command validation + queue
- state broadcast toi thieu

### Milestone 4: Full slice

- 6-8 rooms
- 1 boss
- hub don gian
- meta currency

## 23. Conclusion

Project nay hien phu hop nhat de phat trien mot `small-game vertical slice` kieu Soul Knight co them che do `host + viewer`. Huong di dung la giu host lam authority tuyet doi, viewer chi gui lenh mua action, va moi viewer action deu di qua validate, queue, execute, broadcast. Neu combat loop, room loop va viewer loop cung chay on trong MVP, luc do moi dang mo rong them economy sau, audience scale va content scale.
