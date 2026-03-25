# Backend API Contract

## 1. Purpose

Tai lieu nay dinh nghia backend contract cho mo hinh:

- `Host app Unity`
- `Backend`
- `Viewer web`

Muc tieu cua backend MVP:

- tao/join session
- relay state
- relay command
- quan ly viewer connection lifecycle

Tai lieu nay uu tien `WebSocket-centric` transport cho MVP web.

## 2. System Boundary

### Backend Responsibilities

- tao `session`
- sinh `session code`
- track `host <-> session`
- track `viewer <-> session`
- relay host state toi viewers
- relay viewer actions toi host
- phat system notice va disconnect reason

### Backend Non-responsibilities in MVP

- khong xu ly gameplay simulation
- khong spawn object
- khong tinh combat result
- khong resolve physics

Gameplay authority trong MVP van thuoc ve `Host app`.

## 3. Transport

### State / Command Channel

- `WebSocket`

### Video Channel

- de sau
- du kien `WebRTC`

## 4. Session Model

### Session Fields

- `sessionCode`
- `sessionId`
- `hostConnectionId`
- `status`
- `createdAt`
- `lastHeartbeatAt`
- `viewerIds[]`

### Session Status

- `Created`
- `Live`
- `Closing`
- `Closed`

## 5. Connection Roles

### Host

- tao session
- publish state
- nhan viewer action
- resolve action

### Viewer

- join session bang `session code`
- subscribe state
- gui action request

## 6. Message Envelope

Tat ca message nen dung cung mot envelope:

```json
{
  "type": "MessageType",
  "version": 1,
  "timestamp": 1710000000,
  "payload": {}
}
```

### Required Envelope Fields

- `type`
- `version`
- `timestamp`
- `payload`

### Optional Envelope Fields

- `requestId`
- `sessionCode`
- `viewerId`
- `hostId`

## 7. Host -> Backend Messages

### HostHello

Dung khi host ket noi backend.

```json
{
  "type": "HostHello",
  "version": 1,
  "payload": {
    "hostId": "host_01",
    "buildVersion": "0.1.0",
    "protocolVersion": 1
  }
}
```

### CreateSessionRequest

```json
{
  "type": "CreateSessionRequest",
  "version": 1,
  "payload": {
    "hostId": "host_01"
  }
}
```

### HostSnapshot

Snapshot day du sau join/reconnect/floor change.

```json
{
  "type": "HostSnapshot",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "hostState": {
      "floorIndex": 1,
      "phase": "Combat",
      "playerHp": 9,
      "playerShield": 2,
      "aliveEnemyCount": 4,
      "targetVisible": true
    },
    "viewerStates": [
      {
        "viewerId": "viewer_7829",
        "crowns": 120,
        "budget": 6
      }
    ]
  }
}
```

### HostDelta

```json
{
  "type": "HostDelta",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "changes": {
      "playerHp": 7,
      "aliveEnemyCount": 3
    }
  }
}
```

### ActionResolved

```json
{
  "type": "ActionResolved",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "viewerId": "viewer_7829",
    "commandId": "cmd_001",
    "actionId": "drop_heal",
    "success": true,
    "reason": "ok",
    "viewerBalanceAfter": 90,
    "roomBudgetAfter": 5
  }
}
```

### SessionHeartbeat

```json
{
  "type": "SessionHeartbeat",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "hostId": "host_01"
  }
}
```

## 8. Backend -> Host Messages

### SessionCreated

```json
{
  "type": "SessionCreated",
  "version": 1,
  "payload": {
    "sessionId": "sess_001",
    "sessionCode": "AB12CD"
  }
}
```

### ViewerJoined

```json
{
  "type": "ViewerJoined",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "viewerId": "viewer_7829"
  }
}
```

### ViewerLeft

```json
{
  "type": "ViewerLeft",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "viewerId": "viewer_7829"
  }
}
```

### ViewerActionRequest

```json
{
  "type": "ViewerActionRequest",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "viewerId": "viewer_7829",
    "commandId": "cmd_001",
    "actionId": "drop_random_weapon",
    "target": {
      "viewportX": 0.62,
      "viewportY": 0.41
    }
  }
}
```

### SessionClosed

```json
{
  "type": "SessionClosed",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "reason": "host_disconnected"
  }
}
```

## 9. Viewer -> Backend Messages

### JoinSessionRequest

```json
{
  "type": "JoinSessionRequest",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "viewerId": "viewer_7829"
  }
}
```

### TargetedActionRequest

```json
{
  "type": "TargetedActionRequest",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "viewerId": "viewer_7829",
    "commandId": "cmd_001",
    "actionId": "spawn_weak_enemy",
    "target": {
      "viewportX": 0.45,
      "viewportY": 0.36
    }
  }
}
```

### ViewerHeartbeat

```json
{
  "type": "ViewerHeartbeat",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "viewerId": "viewer_7829"
  }
}
```

## 10. Backend -> Viewer Messages

### JoinAccepted

```json
{
  "type": "JoinAccepted",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "viewerId": "viewer_7829",
    "viewerBalance": 120,
    "roomBudget": 6
  }
}
```

### JoinRejected

```json
{
  "type": "JoinRejected",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "reason": "invalid_session"
  }
}
```

### SessionSnapshot

- payload giong `HostSnapshot`
- gui khi:
  - viewer join
  - viewer reconnect
  - floor change
  - desync recovery

### SessionDelta

- gui cac thay doi nho
- khong thay snapshot

### SystemNotice

```json
{
  "type": "SystemNotice",
  "version": 1,
  "payload": {
    "sessionCode": "AB12CD",
    "notice": "host_not_ready"
  }
}
```

## 11. Target Coordinate Contract

Backend chi relay target.

Target phai theo `normalized viewport coordinates`:

- `viewportX` thuoc `[0..1]`
- `viewportY` thuoc `[0..1]`

Viewer khong duoc gui:

- `worldX`
- `worldY`

Host la ben duy nhat duoc quy doi:

- `viewport -> world`

## 12. Validation Ownership

### Backend Validate

- session code ton tai
- role hop le
- message schema hop le
- host session con song

### Host Validate

- viewer du crowns
- viewer con budget
- action hop le
- cooldown hop le
- target hop le
- phase hop le

## 13. Error Codes

Backend va host nen thong nhat cac ma loi sau:

- `invalid_session`
- `session_closed`
- `host_offline`
- `invalid_role`
- `invalid_payload`
- `invalid_phase`
- `invalid_viewer`
- `invalid_action`
- `invalid_target`
- `insufficient_currency`
- `budget_exceeded`
- `cooldown_active`
- `queue_full`
- `host_not_ready`

## 14. Reconnect Rules

- viewer reconnect vao session cu neu session con `Live`
- backend gui lai `SessionSnapshot`
- host khong reset gameplay state khi viewer reconnect

## 15. Security Notes For MVP

- MVP chua can auth nang
- `viewerId` co the la pseudo id
- `sessionCode` la secret join toi thieu

Nhung backend van nen:

- rate limit `JoinSessionRequest`
- rate limit `TargetedActionRequest`
- reject message schema sai

## 16. Do Now / Skip Now

### Do Now

- WebSocket session server
- session create/join
- snapshot/delta relay
- targeted action relay
- action result relay

### Skip Now

- REST API phong phu
- auth account that
- replay storage
- analytics pipeline
- video ingest gateway
