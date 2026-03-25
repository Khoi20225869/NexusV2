# Host-Viewer Web MVP Specification

## 1. Scope

Tai lieu nay mo ta dich den MVP cho he thong `Host app + Viewer web + Backend`.

Muc tieu cua MVP:

- `Host` choi game that trong app Unity desktop.
- `Viewer` join phong bang web qua `session code`.
- `Backend` quan ly room/session va relay du lieu giua host va viewer.
- Viewer xem duoc tran dau dang dien ra.
- Viewer chon action va, voi action co target, chon vi tri tren man hinh de tac dong vao game.
- Host la authority cuoi cung cho moi thay doi gameplay.

Tai lieu nay khong rang buoc vao implementation TCP desktop prototype hien tai. No dinh huong cho phase ke tiep khi chuyen sang `viewer tren web`.

## 2. System Roles

### Host App

- Chay gameplay that.
- Giu state runtime cua tran dau.
- Gui session state len backend.
- Nhan viewer command tu backend.
- Validate va resolve command.
- Gui ket qua va state moi len backend.

### Viewer Web

- Join phong bang `session code`.
- Xem tran dau cua host.
- Hien store/action UI.
- Gui action request len backend.
- Nhan ket qua thanh cong/that bai va state moi.

### Backend

- Tao va quan ly room/session.
- Mapping `session code -> host`.
- Mapping `session -> viewers`.
- Relay state tu host toi viewers.
- Relay command tu viewer toi host.
- Co the giu mot phan hoac toan bo authority cho economy sau nay.

## 3. Functional Requirements

### FR-01. Host Session Lifecycle

- Host phai dang ky duoc voi backend.
- Backend phai tao `session`.
- Backend phai sinh `session code` ngan, de nhap tay duoc.
- Host phai nhan duoc `session code` de show/copy cho nguoi dung.

### FR-02. Viewer Join

- Viewer phai co man hinh nhap `session code`.
- Viewer phai join duoc room neu code hop le.
- Viewer phai nhan duoc thong bao neu:
  - code sai
  - room da dong
  - host offline

### FR-03. Match Viewing

- Sau khi join thanh cong, viewer phai thay duoc tran dau dang dien ra.
- MVP cho phep 2 cap do:
  - `State spectator`: viewer xem state/spectator UI.
  - `Live video`: viewer xem video stream giong livestream.
- Dinh huong dich den cua san pham la `live video`, nhung phase MVP co the di qua `state spectator` truoc neu can de giam rui ro.

### FR-04. Viewer Action Purchase

- Viewer phai thay duoc action catalog.
- Moi action phai hien toi thieu:
  - ten
  - mo ta ngan
  - gia
  - cooldown
  - budget cost
- Viewer phai co the chon action va gui request.

### FR-05. Targeted Action

- Action co target position phai dua viewer vao `targeting mode`.
- Viewer phai click duoc vao khung xem tran dau de chon diem.
- He thong phai gui vi tri click len host theo mot dinh dang chuan hoa.
- Host phai quy doi duoc vi tri nay sang world position hop le trong game.

### FR-06. Host Authority

- Viewer khong duoc spawn, heal, drop, hay thay doi gameplay truc tiep.
- Host phai validate:
  - session hop le
  - viewer hop le
  - viewer du crowns
  - viewer con budget
  - action hop le
  - target hop le
- Chi sau khi pass validate moi duoc thuc thi gameplay effect.

### FR-07. Economy and Budget

- Moi viewer phai co `Crowns` rieng.
- Moi floor/combat phase phai co `Budget` rieng.
- Moi action phai co:
  - `Cost`
  - `Cooldown`
  - `BudgetCost`
- Sau khi host resolve action, viewer phai nhan duoc so du moi.

### FR-08. Action Result Feedback

- Viewer phai nhan duoc ket qua cho moi command:
  - `success`
  - `fail`
  - `reason`
- Viewer phai thay duoc:
  - so du sau mua
  - budget con lai
  - action log ngan

### FR-09. Reconnect

- Viewer mat ket noi phai co the reconnect vao dung session neu room con mo.
- Sau reconnect, viewer phai nhan `snapshot` moi nhat.
- Host khong duoc bi anh huong gameplay khi viewer reconnect.

### FR-10. Extensibility

- Giai phap MVP phai mo rong duoc sang:
  - nhieu viewer cung luc
  - web viewer full
  - live stream that
  - action target phong phu hon

## 4. Non-functional Requirements

### NFR-01. Authority Safety

- Host phai la authority gameplay duy nhat trong MVP.
- Viewer khong duoc phep gui world state truyen thang.
- Viewer chi duoc gui `intent`.

### NFR-02. Latency

- Command viewer -> host -> result viewer trong local/LAN nen duoi `500ms`.
- Neu co live video, muc tieu UX la do tre duoi `1.5s` trong local/LAN demo.

### NFR-03. Resolution Independence

- Target click khong duoc phu thuoc vao pixel resolution.
- Dinh dang target position phai dung `normalized viewport coordinates`.

### NFR-04. Recoverability

- Viewer reconnect phai nhan duoc `snapshot` day du.
- System khong duoc phu thuoc vao delta-only sync.

### NFR-05. Simplicity

- MVP khong them rollback, replay, prediction, hay anti-cheat nang.
- Uu tien implementation it moving parts, de demo duoc nhanh.

### NFR-06. Web Alignment

- Kien truc cuoi phai phu hop voi `viewer web`.
- Transport dich den nen uu tien:
  - `WebSocket` cho command/state
  - `WebRTC` cho video neu lam livestream that

### NFR-07. Observability

- Host, backend, va viewer phai co log ro:
  - session created
  - join accepted/rejected
  - command received
  - command rejected
  - command resolved

## 5. Authority Model

- Host la authority duy nhat cho:
  - player HP
  - player inventory
  - enemy spawn
  - item drop
  - floor progression
  - reward resolution
- Backend la authority session-level cho:
  - room/session mapping
  - join/leave
  - connection lifecycle
- Viewer chi la authority cho:
  - action selection intent
  - target selection intent

Command flow bat buoc:

1. Viewer gui request.
2. Backend receive va relay.
3. Host validate.
4. Host resolve hoac reject.
5. Host gui result/state moi.
6. Backend broadcast toi viewer.

## 6. Message Protocol MVP

### 6.1 Transport

- `State and command channel`: `WebSocket`
- `Video channel`: `WebRTC` o phase livestream

Luu y:

- Trong phase MVP som, co the chua co `WebRTC`.
- Khi do viewer se dung `state spectator`.

### 6.2 Message Directions

#### Backend -> Host

- `SessionCreated`
- `ViewerJoined`
- `ViewerLeft`
- `ViewerActionRequest`
- `SessionClosed`

#### Host -> Backend

- `HostHello`
- `HostSnapshot`
- `HostDelta`
- `ActionResolved`
- `SessionHeartbeat`

#### Backend -> Viewer

- `JoinAccepted`
- `JoinRejected`
- `SessionSnapshot`
- `SessionDelta`
- `ActionCatalog`
- `ActionResolved`
- `SystemNotice`

#### Viewer -> Backend

- `JoinSessionRequest`
- `TargetedActionRequest`
- `DisconnectNotice`
- `ViewerHeartbeat`

### 6.3 Core Data Contracts

#### JoinSessionRequest

```json
{
  "type": "JoinSessionRequest",
  "sessionCode": "AB12CD",
  "viewerId": "viewer_7829"
}
```

#### JoinAccepted

```json
{
  "type": "JoinAccepted",
  "sessionCode": "AB12CD",
  "viewerId": "viewer_7829",
  "viewerBalance": 120,
  "roomBudget": 6
}
```

#### SessionSnapshot

```json
{
  "type": "SessionSnapshot",
  "sessionCode": "AB12CD",
  "hostState": {
    "floorIndex": 1,
    "phase": "Combat",
    "playerHp": 9,
    "playerShield": 2,
    "aliveEnemyCount": 4,
    "targetVisible": true
  },
  "viewerState": {
    "viewerId": "viewer_7829",
    "crowns": 120,
    "budget": 6
  }
}
```

#### TargetedActionRequest

```json
{
  "type": "TargetedActionRequest",
  "sessionCode": "AB12CD",
  "viewerId": "viewer_7829",
  "commandId": "cmd_001",
  "actionId": "drop_random_weapon",
  "target": {
    "viewportX": 0.62,
    "viewportY": 0.41
  }
}
```

#### ActionResolved

```json
{
  "type": "ActionResolved",
  "sessionCode": "AB12CD",
  "viewerId": "viewer_7829",
  "commandId": "cmd_001",
  "actionId": "drop_random_weapon",
  "success": true,
  "reason": "ok",
  "viewerBalanceAfter": 65,
  "roomBudgetAfter": 4
}
```

### 6.4 Target Coordinate Contract

Viewer khong gui `world position`.

Viewer gui:

- `viewportX` trong khoang `0..1`
- `viewportY` trong khoang `0..1`

Y nghia:

- `(0, 0)` la goc duoi trai khung xem
- `(1, 1)` la goc tren phai khung xem

Host se:

1. lay camera hien tai
2. doi `viewport -> screen -> world`
3. validate world point
4. neu hop le moi resolve action

### 6.5 MVP Action IDs

- `drop_heal`
- `drop_random_weapon`
- `spawn_weak_enemy`
- `spawn_elite_enemy`

### 6.6 Validation Rules

Host phai check toi thieu:

- `invalid_session`
- `invalid_viewer`
- `invalid_phase`
- `insufficient_currency`
- `budget_exceeded`
- `cooldown_active`
- `invalid_action`
- `invalid_target`
- `queue_full`
- `host_not_ready`

### 6.7 Snapshot and Delta Strategy

`Snapshot` gui luc:

- viewer join
- viewer reconnect
- floor change
- phase change
- desync recovery

`Delta` gui khi:

- HP/shield doi
- enemy count doi
- crowns doi
- budget doi
- action resolve

## 7. MVP Viewing Modes

### Mode A. State Spectator

- Viewer thay spectator UI/state, khong phai video stream.
- De lam nhat.
- Dung de hoan thanh action loop som.

### Mode B. Live Stream Viewer

- Viewer thay man hinh host nhu xem livestream.
- Can them `WebRTC`.
- Dung voi muc tieu san pham cuoi.

Khuyen nghi:

- Hoan thanh authority + target action tren `Mode A` truoc.
- Sau do thay lop viewing bang `Mode B`.

## 8. Milestone Implementation Plan

### Milestone 1. Session and Backend Bridge

Muc tieu:

- Host app ket noi backend.
- Backend tao session code.
- Viewer web join bang code.

Done khi:

- host tao duoc session
- viewer join duoc session
- backend log dung host/viewer mapping

### Milestone 2. State Spectator MVP

Muc tieu:

- Viewer web thay duoc state tran dau.
- Chua can video stream.

Done khi:

- viewer thay floor, HP, crowns, budget, action catalog
- reconnect nhan duoc snapshot dung

### Milestone 3. Viewer Action Loop

Muc tieu:

- Viewer bam action.
- Host nhan command qua backend.
- Host reject/resolve dung luat.

Done khi:

- viewer mua duoc 4 action MVP
- crowns/budget cap nhat dung
- host log command ro rang

### Milestone 4. Targeted Actions

Muc tieu:

- Viewer click len khung xem.
- He thong gui `viewport target`.
- Host quy doi va spawn/heal/drop tai diem hop le.

Done khi:

- `drop_heal` va `drop_random_weapon` co target duoc
- `spawn_weak_enemy` target duoc
- host tu choi dung reason neu diem khong hop le

### Milestone 5. Reconnect and Stability

Muc tieu:

- Viewer mat ket noi van vao lai duoc.
- State khong ve 0 sai lech.

Done khi:

- reconnect van nhan snapshot dung
- economy/budget khong bi reset sai

### Milestone 6. Live Video Stream

Muc tieu:

- Viewer thay man hinh host nhu livestream.

Done khi:

- host stream duoc video sang web viewer
- viewer click tren khung stream van map dung target

### Milestone 7. Multi-viewer and Scale

Muc tieu:

- Nhieu viewer cung join mot host.

Done khi:

- economy rieng theo viewer
- command resolve dung viewer id
- room/session van on dinh khi co nhieu viewer

## 9. Do Now / Skip Now

### Do Now

- Chot message contracts.
- Chot authority model.
- Chot target coordinate contract.
- Chot backend role.
- Chot implementation order.

### Skip Now

- replay log full fidelity
- voice/chat
- anti-cheat nang
- adaptive bitrate
- spectator prediction
- rollback sync

## 10. Summary

Kien truc dich den cho bai toan nay la:

- `Host app Unity` chay gameplay that
- `Viewer web` join session bang code
- `Backend` giu room/session va relay
- `Host` giu authority gameplay
- `Viewer` chi gui `intent`
- target position phai gui bang `normalized viewport coordinates`
- `WebSocket` la kenh MVP cho state/command
- `WebRTC` la lop them vao khi muon viewer nhin nhu livestream that
