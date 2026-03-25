# Architecture Docs Index

## Purpose

Thu muc nay chua cac tai lieu kien truc cho prototype `Host app + Viewer`.

Bo docs hien tai da chia thanh 3 lop:

- `gameplay prototype`
- `host-viewer protocol`
- `web transition target`

## Recommended Reading Order

### 1. Product / Game Direction

- [GDD_SoulKnight_Like.md](d:/SPUM-master/SPUM-master/Docs/GDD_SoulKnight_Like.md)

Doc file nay truoc neu can hieu:

- game loop tong the
- host-viewer fantasy
- scope vertical slice

### 2. Core Host-Viewer Target

- [HostViewer_Protocol.md](d:/SPUM-master/SPUM-master/Docs/Architecture/HostViewer_Protocol.md)

Doc file nay de nam:

- scope MVP dich den
- functional requirements
- non-functional requirements
- protocol MVP
- milestone implementation plan

### 3. Backend Layer

- [Backend_API_Contract.md](d:/SPUM-master/SPUM-master/Docs/Architecture/Backend_API_Contract.md)

Doc file nay de nam:

- backend nhan gi
- backend gui gi
- envelope message
- session lifecycle
- command/state relay contract

### 4. Viewer Web UX Layer

- [Viewer_Web_Screen_Flow.md](d:/SPUM-master/SPUM-master/Docs/Architecture/Viewer_Web_Screen_Flow.md)

Doc file nay de nam:

- viewer web co nhung man hinh nao
- join/connect/live/targeting/disconnect flow
- acceptance criteria cho frontend viewer

### 5. Ownership Decision

- [State_Ownership_Decision.md](d:/SPUM-master/SPUM-master/Docs/Architecture/State_Ownership_Decision.md)

Doc file nay de chot:

- cai gi nam o host
- cai gi nam o backend
- cai gi viewer chi duoc xem/gui intent

### 6. Economy / Balance

- [Viewer_Economy_Design.md](d:/SPUM-master/SPUM-master/Docs/Architecture/Viewer_Economy_Design.md)
- [HostViewer_Tuning_Notes.md](d:/SPUM-master/SPUM-master/Docs/Architecture/HostViewer_Tuning_Notes.md)

Doc nhom nay khi bat dau:

- chot gia
- cooldown
- budget
- tuning sau playtest

### 7. Execution / Build-Up

- [Implementation_Timeline.md](d:/SPUM-master/SPUM-master/Docs/Architecture/Implementation_Timeline.md)
- [Scene_Wiring_Checklist.md](d:/SPUM-master/SPUM-master/Docs/Architecture/Scene_Wiring_Checklist.md)

Doc nhom nay khi:

- chuyen tu planning sang implementation
- can lap scene prototype local

## Current Recommended Direction

Huong dich den hien tai la:

- `Host` = Unity desktop app
- `Viewer` = web app
- `Backend` = room/session relay layer
- `Video` = de sau, du kien `WebRTC`
- `State/command` = `WebSocket`

## Do Not Treat As Final Product Architecture

He thong local `TCP viewer app` hien tai trong project chi la prototype de validate:

- session flow
- authority flow
- crowns / budget
- viewer actions

No khong nen duoc xem la kien truc dich den cho web viewer.
