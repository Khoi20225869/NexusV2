# Host-Viewer Protocol Blueprint

## 1. Purpose

Tai lieu nay dinh nghia luong message toi thieu giua `Host` va `Viewer` cho MVP. Muc tieu la giu `Host` lam authority duy nhat, con `Viewer` chi gui command hop le de tac dong vao tran dau.

## 2. Roles

### Host

- chay gameplay that
- giu game state, RNG, HP, enemy state, room state
- validate va resolve moi viewer action
- broadcast state toi thieu cho viewer

### Viewer

- chi xem state cua tran dau
- gui purchase/action request
- nhan ket qua thanh cong, that bai, cooldown, gia hien tai

## 3. Authority Model

- Host la authority duy nhat cho:
  - player HP
  - enemy spawn
  - item drop
  - room phase
  - reward resolution
- Viewer khong bao gio duoc spawn object truc tiep.
- Moi viewer command phai di qua:
  1. receive
  2. validate
  3. enqueue
  4. execute
  5. broadcast result

## 4. Session States

- `Lobby`
- `RunStarting`
- `Combat`
- `Reward`
- `Shop`
- `BossIntro`
- `BossCombat`
- `RunEnded`

Viewer chi duoc mua action trong mot so state hop le, mac dinh la `Combat`.

## 5. Core Message Types

### Host -> Viewer

- `SessionSnapshot`
  - session id
  - room index
  - room phase
  - host hp
  - host shield
  - alive enemy count
  - available viewer actions
- `SessionDelta`
  - state thay doi tu snapshot gan nhat
- `ActionCatalog`
  - danh sach action, cost, cooldown, room rules
- `ActionQueued`
  - command id
  - action id
  - eta hoac queue position
- `ActionResolved`
  - command id
  - success/fail
  - reason
  - gameplay effect summary
- `EconomyUpdate`
  - viewer currency
  - current discounts/surcharges neu co
- `SystemNotice`
  - room locked
  - phase changed
  - action temporarily disabled

### Viewer -> Host

- `JoinSessionRequest`
  - viewer id
  - session code
- `JoinSessionAck`
  - viewer id
  - auth token hoac pseudo token cho MVP
- `PurchaseActionRequest`
  - command id
  - viewer id
  - action id
  - optional target data
- `Heartbeat`
  - viewer id
  - timestamp

## 6. Purchase Flow

1. Viewer gui `PurchaseActionRequest`.
2. Host receive command.
3. `ViewerActionValidator` check:
   - session state
   - viewer currency
   - action cooldown
   - room budget
   - active cap
4. Neu fail:
   - gui `ActionResolved` voi `success = false`
   - tra ve reason
5. Neu pass:
   - tru currency
   - dua command vao queue
   - gui `ActionQueued`
6. `ViewerActionExecutor` resolve command theo tick hop le.
7. Host broadcast `ActionResolved` va `SessionDelta`.

## 7. Minimal Data Contracts

### PurchaseActionRequest

```json
{
  "type": "PurchaseActionRequest",
  "commandId": "cmd_001",
  "viewerId": "viewer_42",
  "actionId": "spawn_weak_enemy",
  "target": {
    "roomNode": "entry_east"
  }
}
```

### ActionResolved

```json
{
  "type": "ActionResolved",
  "commandId": "cmd_001",
  "success": true,
  "reason": "ok",
  "effect": {
    "spawnedEnemyId": "slime_raider",
    "roomIndex": 3
  }
}
```

## 8. MVP Action IDs

- `spawn_weak_enemy`
- `spawn_elite_enemy`
- `drop_heal`
- `drop_random_weapon`

## 9. Validation Rules

- room phase phai hop le
- command id phai unique trong time window ngan
- viewer phai du currency
- action khong dang cooldown
- room budget khong vuot tran
- target data phai nam trong whitelist hop le

## 10. Failure Reasons

- `invalid_phase`
- `insufficient_currency`
- `cooldown_active`
- `budget_exceeded`
- `invalid_target`
- `action_disabled`
- `queue_full`

## 11. Broadcast Strategy

- Dung `snapshot + delta`
- Snapshot gui luc:
  - viewer join
  - phase change
  - recover from desync
- Delta gui khi:
  - HP doi
  - enemy count doi
  - room phase doi
  - action resolve

## 12. Keep Simple For MVP

- Chua can rollback sync.
- Chua can spectator prediction.
- Chua can replay log full fidelity.
- Chua can chat-driven parser.

## 13. Recommended Runtime Owners

- `ViewerSessionService`
  - session lifecycle
- `StateBroadcaster`
  - snapshot/delta output
- `ViewerCommandReceiver`
  - parse incoming request
- `ViewerActionValidator`
  - rule checks
- `ViewerActionQueue`
  - queue pending actions
- `ViewerActionExecutor`
  - perform allowed actions in gameplay
