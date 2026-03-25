# State Ownership Decision

## 1. Purpose

Tai lieu nay chot `cai gi thuoc ve Host`, `cai gi thuoc ve Backend`, va `cai gi Viewer chi duoc xem hoac gui intent`.

Day la decision quan trong nhat khi chuyen tu prototype local sang mo hinh:

- `Host app Unity`
- `Viewer web`
- `Backend`

## 2. Design Goal

- giu gameplay authority ro rang
- tranh backend om qua nhieu gameplay som
- tranh viewer co quyen sua state game
- de prototype chuyen dan len web ma khong dap di qua nhieu

## 3. Recommended Ownership Model

### Host Owns

- player HP
- player shield
- player inventory
- player position
- enemy state
- enemy spawn
- item drop
- floor progression
- combat resolution
- target mapping `viewport -> world`
- action validation mang tinh gameplay

### Backend Owns

- session lifecycle
- room/session mapping
- host connection state
- viewer connection state
- session code
- message routing
- reconnect state

### Viewer Owns

- UI state cuc bo
- action selection intent
- target selection intent

Viewer khong own:

- world state
- inventory state
- enemy state
- combat result

## 4. Why This Model

### Vi sao khong de Viewer giu them quyen

- viewer de gian lan
- viewer co the gui toa do/command sai
- viewer khong duoc tin cho gameplay state

### Vi sao chua nen day gameplay authority sang Backend ngay

- backend gameplay authority lam scope no ra rat nhieu
- se can tach simulation khoi Unity som
- se lam cham MVP

### Vi sao Host nen giu gameplay authority o MVP

- host da co gameplay runtime that
- de noi tiep voi prototype hien tai
- it rui ro hon

## 5. Ownership by State Category

## Session State

### Owner

- `Backend`

### Includes

- session code
- host online/offline
- viewer join/leave
- room open/closed

## Gameplay State

### Owner

- `Host`

### Includes

- floor hien tai
- player HP/shield
- player transform
- enemy count
- spawn result
- loot result

## Economy State

### MVP Owner

- `Host`

### Future Option

- co the chuyen dan mot phan sang `Backend`

### Includes

- viewer crowns
- viewer budget
- action cooldown outcome

### Reason

Trong prototype hien tai, economy dang gan chat voi action validator va action executor o host. De MVP len nhanh, nen giu economy authority o host truoc.

## Action Catalog

### Authoring Owner

- `Host build / config data`

### Delivery Owner

- `Backend relay`

### Notes

Viewer chi nhan du lieu de hien thi. Viewer khong tu quyet dinh gia hay budget cost.

## Target Coordinates

### Viewer Sends

- `viewportX`
- `viewportY`

### Host Owns

- quy doi sang world
- validate diem
- reject hoac accept

### Backend Role

- chi relay

## 6. Ownership by Decision Type

### Backend Decides

- session code co hop le khong
- viewer co join dung room khong
- host con online khong
- message schema co hop le khong

### Host Decides

- viewer co du crowns khong
- viewer con budget khong
- action nay co duoc phep trong phase hien tai khong
- target co hop le khong
- action co thanh cong khong

### Viewer Decides

- muon chon action nao
- muon target o dau

## 7. Hard Rules

### Rule 1

Viewer khong duoc gui `world position`.

### Rule 2

Viewer khong duoc gui `state patch`.

### Rule 3

Backend khong resolve gameplay effect trong MVP.

### Rule 4

Host khong duoc phep bo qua validate chi vi viewer UI da khoa nut.

## 8. What Can Move To Backend Later

Sau MVP, co the day dan cac phan sau sang backend:

- viewer balance ledger
- budget ledger
- cooldown ledger
- anti-spam / anti-fraud rate limit nang hon
- action purchasing authority

Nhung ngay ca khi do, backend van khong nen tu resolve:

- physics
- combat
- item spawn position

neu chua co dedicated server simulation that.

## 9. Recommended Evolution Path

### Phase A. Current MVP

- `Host` own gameplay
- `Host` own economy
- `Backend` own session
- `Viewer` own UI intent

### Phase B. Network Hardening

- `Backend` them rate limit
- `Backend` them action audit log
- `Backend` them reconnect state tot hon

### Phase C. Economy Centralization

- `Backend` own viewer balance ledger
- `Host` van own gameplay validation cuoi cung

### Phase D. Full Service Upgrade

Chi xem xet neu project lon hon rat nhieu:

- tach gameplay simulation authority ra khoi host

Day khong nam trong MVP hien tai.

## 10. Final Decision

Quyet dinh hien tai cho project nay:

- `Host` la gameplay authority
- `Backend` la session authority
- `Viewer` chi la intent sender

Va doi voi target action:

- viewer gui `normalized viewport coordinates`
- backend relay
- host quy doi va validate

Day la mo hinh nho nhat van dung cho:

- host desktop app
- viewer web
- transition len livestream/viewer web sau nay
