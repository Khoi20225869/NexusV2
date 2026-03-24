# Host Viewer Tuning Notes

## 1. Goal

Tai lieu nay la diem bat dau playtest balance cho prototype `host + viewer`.

## 2. First Pass Tuning Order

1. `spawn_weak_enemy`
2. `drop_heal`
3. `spawn_elite_enemy`
4. `drop_random_weapon`

Ly do:

- Hai action dau xuat hien nhieu va anh huong ro nhat den cam giac viewer.
- Hai action sau nen de sau khi room loop va combat loop da on.

## 3. Signals To Watch

### Host side

- room co bi viewer pha vo qua de khong
- co room nao bi viewer spam den khong the clear khong
- player co du co hoi recover khong

### Viewer side

- viewer co thay feedback ngay sau khi mua khong
- viewer co hay bi `Need Crowns` hoac `No Budget` qua nhieu khong
- button cooldown co ro rang khong

## 4. Starter Ranges

- weak enemy:
  - price `20-30`
  - cooldown `4-6s`
  - budget `1`
- elite enemy:
  - price `100-140`
  - cooldown `15-25s`
  - budget `3-4`
- heal:
  - price `35-50`
  - cooldown `8-12s`
  - budget `1`
- random weapon:
  - price `70-90`
  - cooldown `12-18s`
  - budget `2`

## 5. What To Nerf First

- neu viewer qua manh:
  - tang cooldown
  - tang budget cost
  - giam join reward

- neu viewer qua yeu:
  - tang join reward
  - giam weak enemy cost
  - giam heal cooldown

## 6. Playtest Loop

1. reset scene
2. viewer duoc 100 `Crowns`
3. clear 1 run 3 room
4. note:
   - room nao kho nhat
   - action nao viewer mua nhieu nhat
   - action nao khong ai muon bam
5. sua 1 bien so moi lan, khong sua 4-5 bien cung luc

## 7. Red Flags

- viewer chi bam 1 action duy nhat
- host thua vi spam elite lien tuc
- viewer khong bao gio du tien de mua action dat
- room budget het qua nhanh lam viewer bi vo hieu

## 8. Good Prototype Outcome

- viewer thay moi room deu co it nhat 1-2 quyet dinh bam nut
- host cam thay viewer dang “chen vao run”, khong phai pha game vo ly
- sau 1 run, ban co du du lieu de chot gia va cooldown vong 2
