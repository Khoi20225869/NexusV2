# Implementation Timeline

## 1. Goal

Timeline nay ket hop `unity-architecture` va `unity-blueprints` mindset: ship mot vertical slice `host + viewer` nho, co combat loop, room loop va interactive viewer loop chay duoc end-to-end.

## 2. Assumption

- 1 dev chinh
- scope MVP nho
- uu tien local prototype truoc, network/transport co the fake hoac local-loopback

## 3. Phase Overview

### Phase 0: Setup

Duration: 2-3 ngay

- tao folder structure `Assets/Game`
- khoa GDD, protocol doc, economy doc
- tao scene rong:
  - `Bootstrap`
  - `Hub`
  - `Run_Prototype`
  - `Viewer_Client`
- tao script placeholders cho module chinh

Deliverable:

- repo co skeleton ro rang
- docs du cho implementation

### Phase 1: Host Combat Core

Duration: 5-7 ngay

- `PlayerController`
- `PlayerAim`
- `PlayerHealth`
- `Projectile`
- `EnemyController`
- `EnemySpawner`
- 1 enemy ranged co ban
- HP, death, restart flow

Deliverable:

- host choi duoc 1 combat room co ban

### Phase 2: Room Loop

Duration: 4-6 ngay

- `RoomController`
- `DoorController`
- room lock/unlock
- 3 enemy archetypes
- reward flow sau clear room
- weapon swap co ban

Deliverable:

- 3-4 room lien tiep chay duoc

### Phase 3: Data-Driven Content

Duration: 3-4 ngay

- `HeroDefinition`
- `WeaponDefinition`
- `EnemyDefinition`
- `EncounterDefinition`
- `RewardDefinition`
- author 2-4 SPUM prefabs cho host va enemy

Deliverable:

- combat loop duoc config bang `ScriptableObject`

### Phase 4: Viewer Protocol MVP

Duration: 4-6 ngay

- `ViewerSessionService`
- `ViewerCommandReceiver`
- `StateBroadcaster`
- local session join flow
- `SessionSnapshot` va `SessionDelta`
- fake local viewer UI de test

Deliverable:

- viewer thay duoc room, HP, va action catalog

### Phase 5: Viewer Action Execution

Duration: 5-7 ngay

- `ViewerActionValidator`
- `ViewerActionQueue`
- `ViewerActionExecutor`
- 4 action dau tien:
  - spawn weak enemy
  - spawn elite enemy
  - drop heal
  - drop random weapon

Deliverable:

- viewer tac dong duoc vao host run qua command flow day du

### Phase 6: Economy And Balancing

Duration: 3-5 ngay

- `ViewerEconomyService`
- `ViewerActionDefinition`
- `ViewerStoreCatalog`
- cooldown
- room budget
- anti-spam rules

Deliverable:

- host-viewer loop can bang co ban, khong vo tran

### Phase 7: Full Vertical Slice

Duration: 5-7 ngay

- 6-8 rooms
- 1 boss
- simple hub flow
- result screen
- meta currency prototype

Deliverable:

- MVP vertical slice hoan chinh

## 4. Recommended Order Of Coding

1. Host combat core
2. Room loop
3. Data assets
4. Viewer session and state broadcast
5. Viewer command validation and execution
6. Economy tuning
7. Content expansion

## 5. Concrete Week Plan

### Week 1

- setup skeleton
- player move/aim/shoot
- 1 enemy ranged
- projectile + damage

### Week 2

- room flow
- 3 enemy archetypes
- reward flow
- weapon swap

### Week 3

- ScriptableObject configs
- SPUM host/enemy prefabs
- cleanup gameplay architecture

### Week 4

- viewer client shell
- session snapshot/delta
- join flow

### Week 5

- command validator
- queue
- execute 4 viewer actions

### Week 6

- economy tuning
- anti-spam
- room budget
- boss + polish

## 6. Exit Criteria Per Phase

### Combat core done when

- host choi duoc 1 room
- enemy co the damage player
- player co death/restart

### Room loop done when

- room khoa/mo dung
- clear room tao reward
- co it nhat 3 room lien tiep

### Viewer MVP done when

- viewer join session duoc
- thay state toi thieu
- mua duoc 4 action dau tien
- host resolve command dung luat

### Vertical slice done when

- 1 run tu start toi boss hoan thanh duoc
- viewer tac dong duoc xuyen suot run
- economy khong bi spam vo tran

## 7. Risks To Schedule

- neu network stack that duoc lam qua som, timeline se cham
- neu content art mo rong qua som, combat core se tre
- neu economy lam truoc authority flow, logic se roi

## 8. Practical Recommendation

- Prototype viewer transport truoc bang local fake client hoac in-editor mock.
- Khi command flow on dinh moi noi sang backend/web viewer that.
- Khong nen nhay vao payment that truoc khi action loop va anti-spam da on.
