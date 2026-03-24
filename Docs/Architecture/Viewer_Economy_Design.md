# Viewer Economy And Balance Blueprint

## 1. Purpose

Tai lieu nay chot rule kinh te cho `Viewer` de tranh viec stream interaction pha vo combat loop cua `Host`.

## 2. Design Goals

- Viewer tac dong duoc vao tran dau nhanh va ro.
- Host van cam thay cong bang, khong bi pha game.
- Gia va cooldown de can bang muc do anh huong.
- He thong du nho de ship MVP.

## 3. Economy Model

### Currency

- Ten tam thoi: `Crowns`
- Viewer nhan `Crowns` qua:
  - login/join session
  - watch time
  - event milestones
  - nap tien that neu project can monetization

### Spending Categories

- hostile actions
  - spawn weak enemy
  - spawn elite enemy
- support actions
  - drop heal
  - drop random weapon

## 4. Starter Price Table

| Action | Price | Cooldown | Budget Cost | Notes |
| --- | ---: | ---: | ---: | --- |
| `spawn_weak_enemy` | 25 | 5s | 1 | action re, dung nhieu |
| `spawn_elite_enemy` | 120 | 20s | 4 | chi mo sau room N |
| `drop_heal` | 40 | 10s | 1 | support host |
| `drop_random_weapon` | 80 | 15s | 2 | tao variation |

## 5. Room Budget Rules

Moi room combat co `ViewerBudget`.

### Example

- combat room thuong: 5 points
- elite room: 4 points
- boss room: 0-2 points hoac khoa hoan toan

### Spend logic

- `spawn_weak_enemy` ton 1 point
- `spawn_elite_enemy` ton 4 points
- `drop_heal` ton 1 point
- `drop_random_weapon` ton 2 points

Khi het budget, host tu choi action moi cho den room ke tiep.

## 6. Safety Rules

- Moi 3-5 giay chi resolve mot so action gioi han.
- Action support va action hostile nen co cooldown rieng.
- Boss intro, reward phase, shop phase mac dinh khoa mua action.
- Elite spawn chi mo sau room thu 3 hoac muon hon.
- Khong cho stack heal lien tuc trong cung mot cua so thoi gian ngan.

## 7. Host Fairness Rules

- Viewer khong duoc:
  - one-shot host
  - spawn enemy ngay tren dau player
  - spam action khi host dang transition
- Spawn point viewer phai:
  - nam trong room
  - cach player mot khoang an toan toi thieu
  - thuoc tap spawn anchors da author truoc

## 8. Suggested Reward Curve For Viewer

- join session: +100 `Crowns`
- moi 60s xem: +10 `Crowns`
- host clear room: +15 `Crowns`
- boss clear: +50 `Crowns`

MVP co the fake cac gia tri nay bang local test values.

## 9. Anti-Spam Rules

- 1 viewer chi co 1 command active trong queue tai mot thoi diem.
- Queue global co gioi han.
- Khi queue full, host tra ve `queue_full`.
- Command duplicate trong window ngan bi bo.

## 10. Monetization Boundary

Neu viewer nap tien that:

- khong cho mua suc manh khong gioi han
- uu tien mua them `Crowns`, con action van bi rang buoc boi cooldown va room budget
- tranh pay-to-griefing qua muc

## 11. Config Assets

Nen author bang `ScriptableObject`:

- `ViewerActionDefinition`
  - action id
  - category
  - price
  - cooldown
  - budget cost
  - allowed phases
  - target rule
- `ViewerStoreCatalog`
  - visible actions
  - sort order
  - unlock conditions
- `ViewerEconomyConfig`
  - join reward
  - watch reward
  - queue size
  - default room budget

## 12. Tuning Priorities

- Dau tien tune:
  - `spawn_weak_enemy`
  - `drop_heal`
- Sau do moi tune:
  - `spawn_elite_enemy`
  - `drop_random_weapon`

Ly do:

- Hai action re va xuat hien nhieu se quyet dinh cam giac viewer interaction.
- Hai action dat tien de sau khi combat loop co du du lieu.

## 13. MVP Keep Simple

- Chua can dynamic pricing.
- Chua can viewer inventory.
- Chua can marketplace.
- Chua can rarity shop phuc tap.

## 14. Success Metrics

- viewer mua action trong duoi 3 click
- host thay action den trong duoi 2 giay o local test
- khong co room nao bi viewer pha vo hoan toan
- support va hostile actions deu duoc dung thuc te
