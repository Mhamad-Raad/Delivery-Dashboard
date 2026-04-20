# Category System Rework — Design

**Status:** Draft, not implemented
**Target release:** Phase 2 (parallel to/ahead of dispatch refactor)
**Author context:** triggered by business request — store and product categories must be manageable via UI, no code deploys to add new ones.

---

## 1. Goal

Give the business a category system where:

1. **Store categories** (what kind of shop a vendor is: Restaurant, Grocery, Pharmacy...) are **global** and managed by admins through the dashboard UI — no more code deploys to add a new shop type.
2. **Product categories** (menu sections within a vendor: Pizzas, Burgers, Drinks...) are **per-vendor** and managed by each vendor through their own dashboard. Data repetition across vendors ("Drinks" existing under 20 vendors) is expected and acceptable.

---

## 2. Critical finding — `Vendor.VendorType` enum

The current `Vendor` entity has a `Type` column backed by the `VendorType` enum:

```csharp
public enum VendorType {
    Restaurant = 1, Cafe = 2, Market = 3,
    Bakery = 4, Pharmacy = 5, Other = 6
}
```

This is the **current** store-category mechanism, hardcoded at compile time. The business's ask ("without backend intervention") means this enum must be **replaced** by the new `VendorCategory` entity. The enum will be deleted.

Migration implication: existing vendor rows have `Type` values that must map cleanly onto new `VendorCategory` seed rows before the column is dropped.

---

## 3. Current state

### Schema (relevant parts only)

```
Vendor
  Id, UserId, Name, Description, ProfileImageUrl,
  OpeningTime, CloseTime,
  Type : VendorType   ← to be replaced

Catagory  (typo, global, no vendor scope)
  Id, Name, Description
  ParentCategoryId : int?  ← self-ref, will be dropped (flat cats)
  → Products[]
  → SubCategories[]

Product
  Id, VendorId, CategoryId : int?, Name, Description, Price...
```

### Existing API

- `CategoryController` at `/DeliveryDashApi/Category` — full CRUD, paged list, top-level, subcategories. Writes locked to `SuperAdmin, Admin`. Reads `[AllowAnonymous]`.
- No `VendorCategoryController` (store categories not a first-class concept yet — lives as enum).

### Existing UI

- **Admin dashboard** (`delivery-dash - dashboard`): no Categories page. `data/Categories.ts` only fetches (read-only) for the product filter dropdown and a home-page analytics card.
- **Vendor dashboard** (`delivery - Vendor`): no Categories page. Product-create picks from the global category list.

---

## 4. Target model

### Entities

```
VendorCategory            (NEW — replaces VendorType enum)
  Id : int
  Name : string           (unique, non-empty, trimmed)
  Description : string?
  IsActive : bool         (soft-disable without deletion)
  CreatedAt, UpdatedAt

Vendor                    (CHANGED)
  - Type : VendorType     ← DROPPED
  + VendorCategoryId : int   (required FK, Restrict on delete)
  + VendorCategory : VendorCategory

Category                  (RENAMED from Catagory, flattened, scoped)
  Id : int
  + VendorId : int        (required FK, Cascade on Vendor delete)
  Name : string           (unique per vendor, non-empty, trimmed)
  Description : string?
  SortOrder : int?        (optional — lets vendors order menu sections)
  - ParentCategoryId      ← DROPPED (flat)
  - SubCategories[]       ← DROPPED

Product                   (UNCHANGED shape)
  CategoryId : int?       (still nullable — uncategorized allowed)
  + validator: Category.VendorId must equal Product.VendorId
```

### Indexes

- `VendorCategory.Name` — unique.
- `Category (VendorId, Name)` — unique composite.
- `Category.VendorId` — FK index (default from EF).

### Enums removed

- `VendorType` enum file deleted.

---

## 5. Authorization matrix

| Endpoint | SuperAdmin | Admin | Vendor (owner) | VendorStaff | Driver | Customer | Anonymous |
|---|---|---|---|---|---|---|---|
| **VendorCategory** |
| GET `/VendorCategory` (list) | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| GET `/VendorCategory/{id}` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| POST `/VendorCategory` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| PUT `/VendorCategory/{id}` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| DELETE `/VendorCategory/{id}` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Category (product categories)** |
| GET `/Category/mine` | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ |
| GET `/Category/{id}` | ❌ | ❌ | ✅ own | ✅ own | ❌ | ❌ | ❌ |
| GET `/Category/by-vendor/{vendorId}` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| GET `/Category/by-vendor/{vendorId}/public` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| POST `/Category` | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| PUT `/Category/{id}` | ❌ | ❌ | ✅ own | ❌ | ❌ | ❌ | ❌ |
| DELETE `/Category/{id}` | ❌ | ❌ | ✅ own | ❌ | ❌ | ❌ | ❌ |

**Key rules:**
- "✅ own" = service-layer check `Category.VendorId == currentUser.VendorId`. 403 otherwise.
- VendorStaff can read vendor's own categories (needed to pick category when creating products) but cannot modify.
- Admin read access on per-vendor categories is **read-only** — used by admin vendor-details page, not for editing vendor menus.
- Public GET of `/Category/by-vendor/{id}/public` exists so customer mobile app can render a vendor's menu sections while browsing the shop.

---

## 6. API surface

### New: `VendorCategoryController`

| Method | Path | Role | Body | Returns |
|---|---|---|---|---|
| GET | `/DeliveryDashApi/VendorCategory` | anon | — | `VendorCategoryResponse[]` |
| GET | `/DeliveryDashApi/VendorCategory/paged?page=&limit=&search=&activeOnly=` | anon | — | paged |
| GET | `/DeliveryDashApi/VendorCategory/{id}` | anon | — | `VendorCategoryResponse` |
| POST | `/DeliveryDashApi/VendorCategory` | Admin | `CreateVendorCategoryRequest { Name, Description, IsActive }` | 201 + resource |
| PUT | `/DeliveryDashApi/VendorCategory/{id}` | Admin | `UpdateVendorCategoryRequest { Name, Description, IsActive }` | 200 + resource |
| DELETE | `/DeliveryDashApi/VendorCategory/{id}` | Admin | — | 204 |

**Delete rule:** block if any `Vendor.VendorCategoryId == {id}` (409 `VendorCategoryInUseException`). Admin must reassign vendors first, or set `IsActive = false` to hide without deleting.

### Existing: `CategoryController` — refactored

Keep `/DeliveryDashApi/Category` path. Changes:

- Remove `GetAllCategories`, `GetTopLevelCategories`, `GetSubCategories` (flat model — no hierarchy, no global list).
- Add `/Category/mine` — returns the calling vendor/staff's own categories.
- Add `/Category/by-vendor/{vendorId}` — admin read of a vendor's categories (includes `ProductsCount` per category for the detail page).
- Add `/Category/by-vendor/{vendorId}/public` — public read for customer app, excludes counts.
- Writes (`POST / PUT / DELETE`) restricted to `Vendor` role, VendorId inferred from JWT, never from request body.
- Paged endpoint kept but filtered by caller's VendorId automatically (admins may pass `?vendorId=` query).

### Response shapes (sketch)

```csharp
class VendorCategoryResponse {
    int Id; string Name; string? Description; bool IsActive;
    int VendorsCount;  // for admin table
}

class CategoryResponse {
    int Id; int VendorId; string Name; string? Description; int? SortOrder;
}

class CategoryWithCountResponse : CategoryResponse {
    int ProductsCount;  // for admin vendor-detail view
}
```

---

## 7. Migration plan

### Seed data — `VendorCategory`

Seed at least these rows (aligned with current `VendorType` values so backfill is trivial):

| Id (suggested) | Name | Description |
|---|---|---|
| 1 | Restaurant | Full-service dining establishments. |
| 2 | Cafe | Coffee shops and casual cafés. |
| 3 | Market | Grocery and general goods stores. |
| 4 | Bakery | Bread, pastries, and baked goods. |
| 5 | Pharmacy | Prescription and OTC medications. |
| 6 | Other | Vendors that don't fit the categories above. |

These ids correspond to existing `VendorType` enum ints, making the backfill a direct copy.

### Migration steps (EF Core migration: `20260420xxxxxx_ReplaceCategorySystem`)

1. Create `VendorCategories` table + unique index on `Name`.
2. Insert the 6 seed rows above (explicit ids).
3. Add nullable `VendorCategoryId` column to `Vendors`.
4. Backfill: `UPDATE Vendors SET VendorCategoryId = CAST(Type AS INTEGER)`.
5. Make `Vendors.VendorCategoryId` NOT NULL; add FK → `VendorCategories.Id` (OnDelete Restrict).
6. Drop `Vendors.Type` column.
7. Rename table `Catagory` → `Category`.
8. Drop `Category.ParentCategoryId` column and its self-referencing FK.
9. Delete all existing `Category` rows **and** null out `Product.CategoryId` (dev DB only — approved, products temporarily uncategorized).
10. Add nullable `VendorId` column, make NOT NULL, FK → `Vendors.Id` (OnDelete Cascade).
11. Add unique index on `Category (VendorId, Name)`.

### Code-side cleanup alongside migration

- Delete `DeliveryDash.Domain/Enums/VendorType.cs`.
- Remove `Type` references in `Vendor.cs`, DTOs, seeders, validators, filters, and any controllers referencing `VendorType`.
- Rename all `Catagory`/`Catagories` identifiers → `Category`/`Categories` (class names, nav props, repo, service, DbSet, exception folder).
- Delete `CategoryHasSubCategoriesException` (no subcategories anymore).
- Update `CategoryService` to enforce vendor scoping + owner-only write rules.

---

## 8. Dashboard changes

### Admin dashboard (`delivery-dash - dashboard`)

**New page:** `pages/vendor-categories/` with:
- List table (Name, Description, IsActive, VendorsCount, actions).
- Create modal.
- Edit modal.
- Delete confirmation (blocked if in use — show vendor list).

**Updated page:** `pages/vendors/VendorDetail.tsx`:
- Replace `VendorType` badge with `VendorCategory.Name` (read from new FK).
- New "Product Categories" section: read-only list of that vendor's categories + product count per category. Uses `GET /Category/by-vendor/{vendorId}`.

**Updated page:** `pages/vendors/` create/edit:
- Replace hardcoded `VendorType` dropdown with a dropdown fed from `GET /VendorCategory?activeOnly=true`.

**Updated page:** `pages/products/` filter:
- Current global category filter no longer applies (categories are vendor-scoped now). Either remove the filter or scope it to a selected vendor.

### Vendor dashboard (`delivery - Vendor`)

**New page:** `pages/categories/` with:
- List of vendor's own categories + product count.
- Create / edit / delete modals.
- Drag-to-reorder (uses `SortOrder`) — stretch goal, can ship v1 without.

**Updated page:** `pages/products/CreateProduct.tsx`, `ProductDetail.tsx`:
- Category picker fetches from `GET /Category/mine` instead of a hardcoded list.
- VendorStaff sees the picker (read-only list comes through).

### Customer mobile app (`mall_dash_mobile`)

- When viewing a vendor's menu, fetch `/Category/by-vendor/{vendorId}/public` to render menu sections.
- Storefront browse ("all pharmacies") uses `/VendorCategory` + vendor-list query filtered by `VendorCategoryId`. (Backend vendor-list endpoint needs a `?vendorCategoryId=` query param — small addition.)

---

## 9. Rollout

| Phase | Surface | Scope |
|---|---|---|
| 2a | Backend | Schema migration, entities, services, controllers, validators, seed, delete enum. Ship & apply to Neon dev. |
| 2b | Admin dashboard | VendorCategory CRUD page + vendor-detail page update + vendor create/edit dropdown swap. |
| 2c | Vendor dashboard | Own-category CRUD page + product-create picker swap. |
| 2d | Mobile app | Menu-section rendering + store browse filter. Can be deferred. |

Each phase shippable independently after 2a.

---

## 10. Out of scope / deferred

- **Nested store categories.** Flat only. If the list grows past ~25 entries and customer-app browse gets unwieldy, revisit.
- **Many-to-many Vendor ↔ VendorCategory.** One per vendor for now. Additive migration later if a hypermarket-style vendor appears.
- **Per-staff-member product-category permissions.** Owner-only for writes. If staff delegation is ever needed, add a `ManageCategories` flag on `VendorStaff`.
- **Category images / icons.** Plain text names only. Add `IconUrl`/`ImageUrl` if the customer app needs visual browse chips later.
- **Category-level analytics.** Not adding fields now. Analytics can be computed via `GROUP BY CategoryId` on products/orders when needed.
- **Sort order on store categories.** Admin order is list order (by name or id). Skip `SortOrder` on `VendorCategory` until customer-app browse UI asks for it.

---

## 11. Resolved rules

- **Deleting a `VendorCategory` with active vendors** → **block** (409 `VendorCategoryInUseException`). Admin must reassign vendors, or set `IsActive = false` to hide without deleting. Safer than silent reassignment to "Other".
- **Deleting a product `Category` with products** → **null out `Product.CategoryId`** (products become uncategorized, not deleted). Vendor can recategorize later.
- **Per-vendor category cap** → **50**. Enforced at service layer with `CategoryLimitReachedException` (409). Prevents UI abuse and keeps menus scannable.

---

## 12. Diagrams

To be rendered as standalone HTML under `diagrams/` (same style as the address rework):

- `05-category-class-diagram.html` — entity classes + relations.
- `06-category-auth-matrix.html` — role × endpoint visual table.
- `07-category-migration-flow.html` — schema before → after, with the `VendorType` → `VendorCategory` backfill path called out.
