# Viewer Web Screen Flow

## 1. Purpose

Tai lieu nay mo ta flow man hinh cho `Viewer web`.

Muc tieu:

- viewer vao bang browser
- nhap `session code`
- join session cua host
- xem tran dau
- mua action
- chon vi tri tren scene neu action can target

Tai lieu nay chi mo ta UX flow, khong rang buoc implementation frontend framework.

## 2. UX Goals

- join session trong it thao tac
- trang thai ket noi ro rang
- viewer nhin phat hieu session dang song hay khong
- mua action nhanh
- target position de hieu
- feedback thanh cong/that bai ro rang

## 3. Screen Map

Viewer web MVP gom 4 man hinh/chinh the chinh:

1. `Join Screen`
2. `Connecting State`
3. `Live Session Screen`
4. `Disconnected / Session Closed State`

Neu can target action, `Live Session Screen` co them `Targeting Overlay`.

## 4. Join Screen

### Muc tieu

- cho viewer nhap `session code`
- join nhanh

### Thanh phan UI

- logo / title
- o nhap `session code`
- nut `Join`
- text status nho
- danh sach `recent sessions` neu co

### Hanh vi

- nut `Join` chi enable khi code hop le o muc format co ban
- bam `Join`:
  - hien state `Connecting`
  - khoa input trong thoi gian cho

### Loi can hien

- `invalid_session`
- `host_offline`
- `session_closed`
- `network_error`

## 5. Connecting State

### Muc tieu

- bao cho viewer biet he thong dang ket noi

### Thanh phan UI

- spinner/loading
- dong text:
  - `Joining session...`

### Hanh vi

- neu timeout:
  - quay ve `Join Screen`
  - giu lai code vua nhap

## 6. Live Session Screen

### Muc tieu

- la man hinh chinh cua viewer
- cho viewer xem tran dau va tac dong

### Bieu cuc de xuat

- `Top bar`
- `Main view`
- `Action panel`
- `Feedback bar`

## 7. Top Bar

### Noi dung

- `session code`
- `connection badge`
- `floor index`
- `host HP / shield`
- `crowns`
- `budget`

### Hanh vi

- connection badge phai co 3 state:
  - `Connected`
  - `Reconnecting`
  - `Disconnected`

## 8. Main View

Co 2 mode:

### Mode A. Spectator State View

- minimap / state board / markers
- target gate
- enemy count
- host position

### Mode B. Live Video View

- khung video stream
- viewer thay tran dau giong livestream

Khuyen nghi:

- thiet ke layout sao cho sau nay thay `Mode A` bang `Mode B` khong pha action panel.

## 9. Action Panel

### Muc tieu

- cho viewer mua action nhanh

### Moi action card phai co

- ten action
- icon
- mo ta ngan
- gia
- cooldown
- budget cost
- trang thai:
  - `available`
  - `cooldown`
  - `not enough crowns`
  - `no budget`

### Hanh vi

- action khong target:
  - bam la gui ngay
- action co target:
  - bam xong vao `Targeting Mode`

## 10. Targeting Overlay

### Muc tieu

- cho viewer chon diem tren scene

### Thanh phan UI

- overlay mo
- cursor/reticle
- nut `Cancel`
- dong hint:
  - `Click a location to place the action`

### Hanh vi

- viewer bam action co target -> bat overlay
- viewer click vao `Main View` -> gui `viewportX/Y`
- viewer bam `Cancel` -> thoat targeting

### Quy tac UX

- khi dang target, cac action khac tam khoa
- reticle phai ro va de thay
- neu click ra ngoai khung xem, khong gui command

## 11. Feedback Bar

### Noi dung

- action result moi nhat
- system notice moi nhat
- history ngan 3-5 dong

### Vi du

- `Drop Heal placed`
- `Not enough Crowns`
- `Target invalid`
- `Session reconnected`

## 12. Disconnected State

### Muc tieu

- thong bao mat ket noi ro rang

### Thanh phan UI

- message lon:
  - `Disconnected`
- nut:
  - `Reconnect`
  - `Back`

### Hanh vi

- `Reconnect` thu vao lai session cu
- `Back` quay ve `Join Screen`

## 13. Session Closed State

### Muc tieu

- thong bao host da dong session hoac session ket thuc

### Thanh phan UI

- message:
  - `Session ended`
- nut:
  - `Join another session`

## 14. Primary User Flow

### Flow A. Join and Watch

1. Viewer mo web.
2. Viewer thay `Join Screen`.
3. Viewer nhap `session code`.
4. Viewer bam `Join`.
5. He thong vao `Connecting State`.
6. Join thanh cong.
7. Viewer vao `Live Session Screen`.

### Flow B. Buy Instant Action

1. Viewer o `Live Session Screen`.
2. Viewer bam action khong target.
3. UI khoa nut tam thoi neu can.
4. Ket qua tra ve:
   - success hoac fail.
5. Update `crowns`, `budget`, `feedback bar`.

### Flow C. Buy Targeted Action

1. Viewer bam action co target.
2. UI vao `Targeting Overlay`.
3. Viewer click vao `Main View`.
4. Client gui `viewportX/Y`.
5. Host resolve.
6. Viewer nhan result.
7. UI thoat `Targeting Overlay`.

### Flow D. Reconnect

1. Ket noi bi mat.
2. UI chuyen sang `Disconnected`.
3. Viewer bam `Reconnect`.
4. He thong join lai session.
5. Neu thanh cong, nhan `snapshot` moi va quay lai `Live Session Screen`.

## 15. Screen Data Requirements

### Join Screen can:

- `recent sessions`
- `last code`

### Live Session Screen can:

- `session code`
- `connection state`
- `floor index`
- `host HP / shield`
- `viewer crowns`
- `viewer budget`
- `action catalog`
- `last action result`

### Targeting Overlay can:

- action dang duoc target
- target hint

## 16. UI State Rules

- Neu `Disconnected`:
  - khoa action panel
- Neu `Connecting`:
  - khoa form va action
- Neu `Targeting`:
  - chi mot action duoc active
- Neu `Cooldown`:
  - card action hien timer

## 17. MVP Screen Components

### Can co ngay

- `Join form`
- `Connection badge`
- `Main view`
- `Action cards`
- `Crowns / Budget bar`
- `Feedback bar`
- `Targeting overlay`
- `Reconnect state`

### De sau

- chat
- viewer profile
- animation nang
- rich notifications
- multi-tab moderation tools
- voice UI

## 18. Visual Direction Notes

Viewer web nen co cam giac:

- sach
- de doc
- uu tien live state
- khong qua game-like o layer shell

Action panel nen nhin giong:

- livestream extension
- tactical control deck

Khong nen nhin nhu mobile cash shop generic.

## 19. Acceptance Criteria

Viewer web MVP duoc xem la dat khi:

- nhap code join duoc session
- thay duoc session state sau join
- bam action mua duoc
- action co target click duoc tren scene
- nhan feedback dung
- reconnect van vao lai duoc

## 20. Suggested Future Upgrade

Sau khi MVP on dinh:

1. thay `spectator state view` bang `live video`
2. bo sung toast feedback dep hon
3. them multi-viewer session UX
4. them viewer account/profile neu can
