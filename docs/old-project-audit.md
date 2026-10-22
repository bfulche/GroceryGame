# Grocery Game: Old Project Audit

*Audit date: 2026-10-22 · Branch `audit` · Unity 6000.0.30f1 · Read-only review (nothing in `Assets/`, `ProjectSettings/` or `Packages/` was changed; no scenes were saved)*

**How this was checked:** I read every script, then inspected the scenes, prefabs, data assets and settings in the open Editor through the MCP connection. I ran a clean recompile, then walked the game in Play Mode (menu → recipe → store → both store scenes). In the store I called the same functions a mouse click triggers, and you did a hand-played pass to confirm how it feels.

---

## 1. Summary

The game opens on a main menu (a full-screen image with a "Play Game!" button). Next comes a recipe carousel showing BLT and Grilled Cheese as notecard-style cards, then a store carousel with two cards (Corner Mart and Bodega Store). Picking **Corner Mart** loads a grey-box store, `Mock Up Store`. There you walk with WASD and mouse-look, and you can click grocery items to pick them up, right-click to examine them, and click again to drop them. A primitive shopping cart can be pushed (it follows to your right), and Tab opens a shopping list with a budget and running cart total. Picking **Bodega Store** loads `Bodega TEST`, a scene with no working player. It's a dead end.

**That's as far as the game goes.** There is no checkout, cashier, register, conveyor, wallet, money pickups, debrief, stars or pantry list. The `RecipeSuccess` and `RecipeFailed` screens exist as code only and are never reached. Inside the store, the cart is mostly broken (see Section 4), and every grocery item is set up as "Bread, $1.99", so the shopping list can't track ingredients correctly.

---

## 2. Project setup

| Setting | Value |
|---|---|
| Unity version | **6000.0.30f1** |
| Render pipeline | **URP 17.0.3**, Forward+ renderer (`PC_Renderer`) |
| Active quality level | **PC** (index 1), which uses `Assets/Settings/PC_RPAsset.asset`. There is also a "Mobile" level with `Mobile_RPAsset` |
| URP asset (PC) | Render scale **1.0** (full resolution), upscaling filter: Automatic, MSAA **off (1×)**, HDR **on**, depth texture **on**, opaque texture **on**, main-light shadows **on** (2048 shadowmap, 50 m distance, soft shadows), additional lights per-pixel |
| Renderer features | **Screen Space Ambient Occlusion** only (the stock template feature) |
| Volume profile | `DefaultVolumeProfile.asset` (stock URP template) and `SampleSceneProfile.asset` (Bloom, Vignette, Tonemapping; Motion Blur off). Nothing PS1-related |
| Input System | **Both.** *Active Input Handling* = "Both" (`activeInputHandler: 2`). **All game code uses the old Input Manager** (`Input.GetAxis`, `Input.GetKeyDown`). The new Input System is used only by the UI's EventSystem (`InputSystemUIInputModule`). `Assets/InputSystem_Actions.inputactions` is the unused default template |
| Default resolution | 1024×768 (Player Settings) |
| Target frame rate | 60, set in code (`GameManager.cs:49`) |
| Physics | Gravity −9.81; **"Queries Hit Triggers" = ON**, so raycasts *do* hit trigger colliders; collision matrix: everything collides with everything (no custom rules); Auto Sync Transforms off |
| Custom layer | **Layer 7 "Interactable"**. The rest are Unity defaults |
| Custom tag | `CinemachineTarget` (leftover from the deleted Starter Assets) |
| Canvas reference resolution | Managers scene: **Scale With Screen Size, 1920×1080, match width (0)**. Mock Up Store's own canvas: Constant Pixel Size (it's disabled anyway). Canvases built by code at runtime have a default CanvasScaler (Constant Pixel Size) |

### Installed packages (`Packages/manifest.json`)

Project-relevant: URP 17.0.3 (+ core, config, Shader Graph), Input System 1.11.2, uGUI 2.0.0 (+ TextMesh Pro, which lives in `Assets/TextMesh Pro`), **Cinemachine 2.10.3 (installed but unused)**, AI Navigation 2.0.5 (unused), Timeline 1.8.7 (unused), Visual Scripting 1.9.5 (unused), Burst/Collections/Mathematics (dependencies), Test Framework.
Tooling: Rider, Visual Studio, Collab Proxy, Multiplayer Center, Package Validation Suite (preview), **MCP for Unity** (`com.coplaydev.unity-mcp`, added for this audit; it's the only uncommitted change on the branch).
Third-party in `Assets/`: **TextMesh Pro** essentials (fonts, shaders, sprites). Not reviewed. `Assets/Plugins` is empty.

### Scenes in Build Settings (load order)

| # | Scene | Notes |
|---|---|---|
| 0 | `Assets/Project/Scenes/Managers.unity` | Menus plus the manager objects. This is the start scene |
| 1 | `Assets/Project/Scenes/Bodega TEST.unity` | Loaded by the "Bodega Store" card. **Broken** (missing player prefabs) |
| 2 | `Assets/Project/Scenes/Mock Up Store.unity` | Loaded by the "Corner Mart" card. The only playable store |

Scenes load **by name** (`SceneManager.LoadScene(store.sceneName)`), so renaming a scene silently breaks a store card.

---

## 3. Scripts

All 23 scripts live in `Assets/Project/Scripts/`. "Status" describes how the script behaves in the project as it is now.

| Path | Namespace | Purpose (one line) | Status |
|---|---|---|---|
| `Core/AudioManager.cs` | GroceryGame.Core | Global sound player. Currently only *logs* sound names; plays nothing | Half-built (and **not placed in any scene**, so it never runs) |
| `Core/FirstPersonController.cs` | GroceryGame.Core | WASD movement, mouse look, left/right-click and Tab/Esc input; adds the InteractionSystem at runtime | Works |
| `Core/GameManager.cs` | GroceryGame.Core | Survives scene loads; holds the game state (menu → recipe → store → shopping), the selected recipe and store; loads the store scene | Works (only up to Shopping) |
| `Core/IInteractable.cs` | GroceryGame.Core | Shared "contract" for clickable things (hover, interact, prompt text) | Works |
| `Core/InteractionSystem.cs` | GroceryGame.Core | Raycasts from the camera every frame, shows the "Click to…" prompt, tracks the held item, starts and stops examine mode | Half-built (works for items; the cart breaks it, see Section 4) |
| `Core/RecipeManager.cs` | GroceryGame.Core | Holds the list of available Recipe assets and passes the player's pick to GameManager | Works |
| `Core/StoreManager.cs` | GroceryGame.Core | Holds the StoreLocation assets and filters to those flagged "starting store" | Works |
| `Core/UIManager.cs` | GroceryGame.UI *(file sits in Core/)* | Shows and hides menu panels; button handlers for Play/Back | Works (two panel slots empty; one Back button is wired wrong) |
| `Data/GameState.cs` | GroceryGame.Core *(file sits in Data/)* | List of game states: MainMenu, RecipeSelection, StoreSelection, Shopping, Checkout, RecipeSuccess, RecipeFailed | Works (Checkout, Success and Failed are never used) |
| `Data/IngredientQuality.cs` | GroceryGame.Data | Generic/Standard/Premium list | **Unused** (a duplicate `ItemQuality` list inside GroceryItem.cs is used instead) |
| `Data/IngredientType.cs` | GroceryGame.Data | Fixed list of 20 ingredient names (Bread, Cheese, Lettuce…) | Works |
| `Data/Recipe.cs` | GroceryGame.Data | Recipe data asset: name, image, description, ingredient list, budget, difficulty | Works |
| `Data/StoreLocation.cs` | GroceryGame.Data | Store data asset: name, description, image, scene name, available ingredients, price modifier, starting-store flag | Works (the "available ingredients" list is never read) |
| `Shopping/GroceryItem.cs` | GroceryGame.Shopping | A pick-up-able product: type, name, price, quality; pick up, drop, place in cart; price math | Half-built (logic runs, but cart placement is wrong and the scene data is all placeholders) |
| `Shopping/ShoppingCart.cs` | GroceryGame.Shopping | Pushable cart: follows the player, keeps a list of items, total cost | **Broken** (throws errors on hover and click; items land outside the basket) |
| `UI/IngredientItemUI.cs` | GroceryGame.UI | Formats one ingredient line with a bullet / checkmark | **Unused** |
| `UI/ItemExaminationUI.cs` | GroceryGame.UI | Close-up "inspect" view: enlarges the held item, dark overlay, info panel (name, description, quality, price) | Works (builds its own UI in code) |
| `UI/Outline.cs` | GroceryGame.UI | Hover highlight. Despite the name it isn't an outline: it makes the material glow (emission) | Works on items; **crashes on the cart** |
| `UI/RecipeCardUI.cs` | GroceryGame.UI | Fills a recipe card (title, image, description, ingredient lines) | Works |
| `UI/RecipeCarouselUI.cs` | GroceryGame.UI | Left/right arrows and Select for recipes; also A/D, arrow keys, Enter and Space | Works |
| `UI/ShoppingListUI.cs` (+ helper class `IngredientListItem`) | GroceryGame.UI | In-store list built in code: recipe name, ingredient rows with have/need counts, budget vs cart total | Half-built (ingredient rows end up outside the panel, see Section 4) |
| `UI/StoreCardUI.cs` | GroceryGame.UI | Fills a store card (name, description, image, "Standard/Budget/Premium prices") | Works |
| `UI/StoreCarouselUI.cs` | GroceryGame.UI | Same carousel pattern as the recipes, plus lots of debug logging | Works (initializes twice; full of debug code) |

### How the scripts connect

```
Managers scene (start)
 ├─ GameManager  (DontDestroyOnLoad = survives scene changes, "singleton" = one global copy)
 │    ├─ refs → UIManager, RecipeManager, StoreManager (same scene)
 │    ├─ holds: CurrentState, SelectedRecipe, SelectedStore
 │    └─ StartShopping(): SceneManager.LoadScene(store.sceneName)
 │                        then after 0.1 s, finds or creates a ShoppingListUI
 ├─ UIManager ── shows/hides panels ── RecipeCarouselUI ─creates→ RecipeCardUI (prefab)
 │                                  └─ StoreCarouselUI  ─creates→ StoreCardUI (prefab)
 ├─ RecipeCarouselUI --Select--> RecipeManager.SelectRecipe --> GameManager.SelectRecipe
 └─ StoreCarouselUI  --Select--> StoreManager.SelectStore  --> GameManager.SelectStore --> load store scene

Store scene (Mock Up Store)
 Player: FirstPersonController ──adds at runtime──> InteractionSystem
   InteractionSystem ─raycast─> anything with IInteractable (GroceryItem, ShoppingCart)
        ├─ GroceryItem: pick up → parent to camera; drop/place → looks for any ShoppingCart within 2 m
        ├─ ShoppingCart: toggles "being pushed", follows the player in FixedUpdate
        └─ ItemExaminationUI (found or created at runtime)
   FirstPersonController (Tab) ──FindObjectOfType──> ShoppingListUI ──reads──> GameManager.SelectedRecipe
                                                                     └──reads──> ShoppingCart.GetItems()
   GroceryItem.Price ──reads──> GameManager.SelectedStore.priceModifier
```

**How state flows:** Everything that has to survive the jump into the store lives on the `GameManager` object (the selected recipe and store). The menu managers and UIManager stay behind in the Managers scene and are destroyed when the store loads. Nothing goes back the other way: no "return to menu", no money, no save data.

**Things that find each other at runtime:** About 13 spots use `FindObjectOfType` ("search the whole scene for one of these"). It's slow, it's deprecated in Unity 6, and it **ignores disabled objects**. That's why the store's pre-built shopping list (on a disabled Canvas) is never found, and code builds a second one instead.

---

## 4. Errors and warnings

### Compile warnings (14, no compile errors)

| # | Message | Cause in plain words | Location | Matters for rebuild? |
|---|---|---|---|---|
| 1–13 | `warning CS0618: 'Object.FindObjectOfType<T>()' is obsolete … Use Object.FindFirstObjectByType…` | Unity 6 renamed this "search the scene" function. The old one still works, but it's on the way out | `ShoppingListUI.cs` 53, 300, 366, 379 · `StoreCarouselUI.cs` 79, 127, 270 · `GameManager.cs` 180 · `InteractionSystem.cs` 163, 198 · `FirstPersonController.cs` 142, 153 · `RecipeCarouselUI.cs` 222 | **Yes, as a pattern.** In the new project, link objects directly in the Inspector or through a manager instead of searching the scene |
| 14 | `warning CS0414: The field 'InteractionSystem.isHoldingInteraction' is assigned but its value is never used` | Leftover variable | `InteractionSystem.cs:23` | No |

### Play Mode errors (console, full messages)

**E1. Bodega TEST scene: missing prefabs** (appears when the Bodega Store card is picked)
```
Problem detected while opening the Scene file: 'Assets/Project/Scenes/Bodega TEST.unity'.
Prefab instance problem. Missing Prefab Asset: 'MainCamera (Missing Prefab with guid: 2d3a85ecde41a8246a79669975912b74)'
Prefab instance problem. Missing Prefab Asset: 'PlayerFollowCamera (Missing Prefab with guid: 9c489a8547f4d9e4387da15a4ff82c81)'
Prefab instance problem. Missing Prefab Asset: 'PlayerCapsule (Missing Prefab with guid: c5efc39a8aaf6e64ea40e9ad573e9b47)'
Warning: Creating missing Camera component for UniversalAdditionalCameraData in MainCamera (Missing Prefab …)
```
*Cause:* This scene was built with Unity's **Starter Assets** first-person controller, and that package was deleted (commit `54546d1 "Removed Unity Character Controller & Package"`). The player and camera are now empty shells: no movement, no `MainCamera` tag, and none of the game's own scripts.
*File:* `Bodega TEST.unity` (scene data, not a script). *Matters:* **No**, the rebuild won't use this scene. It does explain why the second store card leads nowhere.

**E2. Second cart object crashes on load** (appears every time Mock Up Store loads)
```
Script 'ShoppingCart' on GameObject Shopping Cart Spawn has possibly missing Required Components!
MissingComponentException: There is no 'Rigidbody' attached to the "Shopping Cart Spawn" game object, but a script is trying to access it.
GroceryGame.Shopping.ShoppingCart.Awake () (at Assets/Project/Scripts/Shopping/ShoppingCart.cs:41)
```
*Cause:* There's an empty object called **"Shopping Cart Spawn"** with a `ShoppingCart` script but no body and no physics. It looks like someone meant to mark a spawn position and attached the cart script by mistake. When the script starts, it tries to change the physics weight of a Rigidbody that isn't there. A side effect: the scene now has **two carts**, and the shopping list and pick-up code each grab "whichever cart they find first".
*File:* `ShoppingCart.cs:41` + `Mock Up Store.unity` › `Shopping Cart Spawn`. *Matters:* **Yes, as a lesson.** Use a plain empty object (or a gizmo) for spawn points.

**E3. Looking at the cart throws an error, repeatedly** (confirmed in your hand test: 18 times in one session)
```
NullReferenceException: Object reference not set to an instance of an object
GroceryGame.Shopping.ShoppingCart.OnStartHover () (at Assets/Project/Scripts/Shopping/ShoppingCart.cs:135)
GroceryGame.Core.InteractionSystem.CheckForInteractable () (at Assets/Project/Scripts/Core/InteractionSystem.cs:82)
GroceryGame.Core.InteractionSystem.Update () (at Assets/Project/Scripts/Core/InteractionSystem.cs:54)
```
*Cause:* To highlight the cart, the code adds an `Outline` component to it. `Outline` refuses to be added to an object without a visible mesh (`[RequireComponent(typeof(Renderer))]`). The cart's root object is an empty parent; the meshes are on its children. So Unity says no, the code receives "nothing", and it crashes on the next line. Because the crash happens *before* the "Click to push cart" prompt appears, **you never see any prompt or highlight on the cart**. That's a big part of why you "could not interact with the cart."
*File:* `ShoppingCart.cs:130–136`. *Matters:* **Yes.** Highlight code has to handle objects whose meshes are on child objects.

**E4. Clicking the cart throws the same kind of error** (reproduced by calling the click code directly)
```
NullReferenceException: Object reference not set to an instance of an object
  at GroceryGame.Shopping.ShoppingCart.StartPushing (GameObject pusher) in …/Shopping/ShoppingCart.cs:177
  at GroceryGame.Shopping.ShoppingCart.OnStartInteract (GameObject interactor) in …/Shopping/ShoppingCart.cs:116
```
*Cause:* Same as E3, in the "start pushing" code. Pushing does start, because the flag is set before the crash, but the green highlight fails. This didn't show up in your hand test, which means **your clicks never reached the cart at all** (see "cart can't be clicked" below).
*File:* `ShoppingCart.cs:177`. *Matters:* Yes, same lesson as E3.

### Bugs found without a console error (all confirmed in Play Mode)

| Bug | What happens | Cause | Where |
|---|---|---|---|
| **Items drop outside the cart, behind it near the handle** (your observation) | Placed items fall onto the floor by the handle | The cart object's **pivot point is at the handle end, not the basket centre**. The script drops items at the pivot (+0.8 m up). I measured it: the basket floor is centred about 0.8 m forward and 0.4 m sideways from the drop point. There's also an unused, hand-made `ItemContainer` child, and the script creates a second one | `ShoppingCart.cs:47–58`, `AddItem` 221–241; cart in `Mock Up Store` |
| **Items near the cart count as "in the cart"** (your observation) | The shopping list ticks items that are lying on the floor | "Place in cart" doesn't check whether you're aiming at the cart. It looks for *any* cart within **2 m** of the item and adds it to the cart's list, wherever it physically lands. The list is a bookkeeping list, not a physical check | `GroceryItem.cs:171–193, 276–288`; `InteractionSystem.cs:113–126` (both branches do the same thing) |
| **Cart can't be clicked from most angles** (your observation) | No prompt, no push | Besides E3, the raycast often hits one of the cart's **child** colliders (side walls, wheels, handle). Those have no cart script, so they count as "nothing interactable". The root box collider is narrower than the basket on the handle side | `InteractionSystem.cs:63–66` (uses `GetComponent`, not `GetComponentInParent`) |
| Cart follows to the **side**, not in front | Cart trails 1.2 m to your right, 0.8 m ahead | `followOffset = (1.2, 0, 0.8)` | `ShoppingCart.cs:17` |
| **Every grocery item is "Bread", named "Grocery Item", $1.99** | Picking up a tomato ticks "Bread" on the list; the examine panel says "Grocery Item / A fresh grocery item." | The item prefabs and all 16 scene items still have the default values | `Prefabs/Items/*.prefab`, `Mock Up Store` items |
| Shopping list rows appear **outside the list panel** | The ingredient rows are created at the top level of the scene, not inside the panel. Text is correct ("Bread (1/1)", "Lettuce (0/1)…"), but it won't display properly | The code saves a reference to the container's transform, then adds a `RectTransform` (a UI transform), which *replaces* the original. The saved reference now points at a deleted object, so the rows get no parent. It also leaves a stray "IngredientItem" template object in the scene | `ShoppingListUI.cs:117–125`, `255–294` |
| Two shopping lists / two canvases | A disabled in-scene `Canvas` with ShoppingListUI and ItemExaminationUI is ignored; code builds a new "GameCanvas" | `FindObjectOfType` skips disabled objects | `GameManager.cs:175–200`, `InteractionSystem.cs:227–239` |
| Store screen's **Back button goes to the Main Menu**, not Recipe Selection | Wrong function wired in the Inspector | Store panel's Back button → `UIManager.OnRecipeSelectionBackClicked` | `Managers` › Store Selection Panel › Back Button |
| Store carousel builds its card **twice** on open | The card is destroyed and recreated once (invisible but wasteful) | Both `UIManager.ShowStoreSelection` and the carousel's own `OnEnable` initialize it | `StoreCarouselUI.cs:60–69` |
| Player spawns at the world origin | "Player Spawn Point" object is ignored | No code reads it | `Mock Up Store` |
| Camera sits at 2.0 m on a 2 m-tall capsule | Eye level is at the very top of the head (feels tall) | Camera local Y = 2 | `Mock Up Store` › Player › MainCamera |
| Examine hint says "Release Right Click to exit" | Actually a toggle: right-click again | Text doesn't match the code | `ItemExaminationUI.cs:224` |
| Escape does nothing | Only logs "Pause menu requested" | Not built | `FirstPersonController.cs:150–159` |
| No crosshair | Hard to tell where you're aiming | `Crosshair-PNG-Image.png` is imported but not used anywhere | — |
| AudioManager never exists | Sound calls silently do nothing | No GameObject in any scene has it | — |
| Recipe "Tacos" is named "Instant Noodles" | Copy-paste slip | Data asset | `ScriptableObjects/Recipes/Tacos.asset` |
| Fragile "•" bullet character | Shows fine now, but `IngredientItemUI.cs` and `RecipeCardUI.cs` are saved in an old text encoding (Windows-1252), not UTF-8 | Editors may turn it into "�" | `RecipeCardUI.cs:109`, `IngredientItemUI.cs:26` |

**Not observed:** no double-firing buttons (each click moved the carousel exactly one step). Walking, mouse look, Tab and Escape produced no errors.

---

## 5. Hard numbers worth keeping

| Value | Number | Where |
|---|---|---|
| Move speed | **5 m/s** (no sprint, no acceleration) | `FirstPersonController.cs:10`; Inspector on Mock Up Store › Player (same) |
| Gravity | −9.81 | `FirstPersonController.cs:11` |
| Mouse sensitivity | **2** (multiplies raw mouse axis, old Input Manager) | `FirstPersonController.cs:15`; Inspector (same) |
| Vertical look limit | ±90° | `FirstPersonController.cs:16` |
| Character Controller | height 2, radius 0.5, centre (0,1,0), slope 45°, step 0.3 | Mock Up Store › Player |
| Camera | FOV 60, near 0.3, far 1000, local position (0, **2.0**, 0) | Mock Up Store › Player › MainCamera |
| Interaction range | **3 m** | `FirstPersonController.cs:19` → passed to InteractionSystem |
| Interaction raycast layers | **All layers (−1)**, and triggers are hit too (Physics setting) | `FirstPersonController.cs:20`; Inspector |
| Items' layer | "Interactable" (layer 7). The cart forces itself onto it in code | Item prefabs; `ShoppingCart.cs:70` |
| Held item position | (0, −0.2, **1.5**) in front of camera, no rotation | `GroceryItem.cs:157–158` |
| Item physics | mass **0.5**, linear damping 2, angular damping 5 (set in code; prefabs say mass 1) | `GroceryItem.cs:61–63` |
| Place-in-cart search radius | **2 m** around the held item | `GroceryItem.cs:278` |
| Drop: ground search | raycast down 5 m, fallback 1.5 m in front of player, place 0.1 m above hit | `GroceryItem.cs:239–252` |
| Cart follow offset | **(1.2 right, 0 up, 0.8 forward)** from the player | `ShoppingCart.cs:17`; Inspector (same) |
| Cart follow speed | **3** in the scene (script default 5); rotation speed 5 | Inspector on Shopping Cart; `ShoppingCart.cs:18–19` |
| Cart physics | mass 10, damping 5/5, X and Z rotation frozen | `ShoppingCart.cs:41–44` |
| Cart capacity | 20 items | `ShoppingCart.cs:14` |
| Cart item container | local (0, 0.8, 0), trigger box 0.8 × 1 × 1.2; items stacked 0.1 m apart with random tilt ±10°, any spin | `ShoppingCart.cs:49–57, 231–240` |
| Examine | item scale × **2**, moved to (0, 0, 2.5) in front of camera; rotation speed **50°/s** (WASD), mouse drag × 2 | `ItemExaminationUI.cs:20–21, 75–76, 129–153` |
| Examine info panel | 300 × 200 px, top-right, 20 px margin, black 90% | `ItemExaminationUI.cs:192–200` |
| Interaction prompt | 300 × 50 px at 40% screen height, black 70%, 20 pt text | `InteractionSystem.cs:246–262` |
| Shopping list panel | 300 px wide, full height minus 100, 50 px from left, black 80%; row 280 × 30; refreshes every 0.5 s | `ShoppingListUI.cs:24, 77–82, 260` |
| Budget colour rule | green ≤ 80% of budget, yellow 80–100%, red over | `ShoppingListUI.cs:330–341` |
| Price rule | Generic = base × **0.7**, Premium = base × **1.8**, then × store price modifier, rounded to cents | `GroceryItem.cs:290–312` |
| UI reference resolution | 1920 × 1080 | Managers › Canvas |
| Recipe card | **1600 × 800** (fills Display Area); title 1400 × 60 @ 52 pt; image 760 × 440; description 640 × 130 @ 36 pt; ingredient list 640 × 480, font **48**, spacing **22** | Recipe Card prefab + Inspector (`RecipeCardUI`) |
| Store card | forced to **480 × 240**, centred (prefab is 1600 × 800) | `StoreCarouselUI.cs:229` |
| Carousel layout | Display Area 1600 × 800; arrow buttons **80 × 80** at x = ±880; Select button **280 × 60** @ 36 pt; Back 140 × 60; header 800 × 100, title 52 pt | Managers › Recipe/Store Selection Panel |
| Main menu Play button | 280 × 60 @ 36 pt, at (5.6, −398.8) | Managers › Main Menu Panel |
| Recipe budgets | BLT **$12**, Grilled Cheese **$10** | Recipe assets |
| Item scale | 0.2 on every item prefab (source models are modelled ~2–5 units) | Item prefabs |

---

## 6. Data assets

### How ingredients are identified
By a fixed **enum** (a hard-coded list in code), `IngredientType`, stored as a number in the assets. Adding an ingredient means editing code. The full list in order: 0 Bread, 1 Cheese, 2 Lettuce, 3 Tomato, 4 Milk, 5 Eggs, 6 Pasta, 7 Rice, 8 Butter, 9 Bacon, 10 InstantNoodles, 11 TacoShells, 12 HotSauce, 13 Mayo, 14 Ketchup, 15 Mustard, 16 Chicken, 17 Turkey, 18 Ham, 19 GroundBeef.
Quality is a second enum on each item (`ItemQuality`: Generic / Standard / Premium). There's a duplicate unused `IngredientQuality` enum. Items themselves are **not** data assets; each item's name, price and type is typed directly on the prefab or scene object.

### Recipe assets (`Assets/Project/ScriptableObjects/Recipes/`)

| Asset | recipeName | Image | Description | Ingredients (all qty 1, substitution allowed) | Budget | Difficulty | In menu? |
|---|---|---|---|---|---|---|---|
| BLT.asset | BLT | `Art/UI/BLT.png` | "A traditional bacon, lettuce, and tomato sandwich. Don't forget the mayo!" | Bread, Lettuce, Tomato, Bacon, Mayo | 12 | 1 | Yes |
| GrilledCheese.asset | Grilled Cheese | `Art/UI/Grilled Cheese.png` | "A simple grilled cheese sandwich. Reminds me of home..." | Bread, Butter, Cheese | 10 | 1 | Yes |
| InstantNoodles.asset | Instant Noodles | none | "An easy and cheap meal." | InstantNoodles | 3 | 1 | No |
| MacNCheese.asset | Mac N Cheese | none | "Mmmmm, so cheesy." | Pasta, Cheese, Butter, Milk | 6 | 1 | No |
| Tacos.asset | **Instant Noodles** (copy-paste error) | none | "An easy and cheap meal." | TacoShells, GroundBeef, Cheese, Lettuce, HotSauce, Tomato | 15 | 1 | No |

Unused fields: `quantity` is always 1, `allowsQualitySubstitution` is never read, and `difficulty` is labelled "not used in POC".

### StoreLocation assets (`Assets/Project/ScriptableObjects/Stores/`)

| Asset | storeName | Description | Image | sceneName | availableIngredients | priceModifier | isStartingStore |
|---|---|---|---|---|---|---|---|
| CornerMart.asset | Corner Mart | "A small store on the corner. They don't have the basics, but not much." *(garbled sentence)* | `Art/UI/Corner Store.png` | **Mock Up Store** | Bread, Cheese, Eggs, Milk, InstantNoodles, Bacon | 1.0 | true |
| Bodega.asset | Bodega Store | "A small conveinent store down the block. They don't have much, but they have the basics." *(typo: "conveinent")* | `Art/UI/bodega thumb AI.jpg` | **Bodega TEST** | *(identical list)* | 1.0 | true |

Notes: both stores are identical apart from text, image and scene. The "available ingredients" lists don't include Lettuce, Tomato, Mayo or Butter, so neither BLT nor Grilled Cheese could be finished if the list were enforced. It never is. The StoreManager lists them in the order Corner Mart, then Bodega.

---

## 7. Scenes

### `Managers.unity`: menus and global managers (start scene)
```
GameManager        [GameManager]   → refs UIManager, RecipeManager, StoreManager
RecipeManager      [RecipeManager] → BLT, GrilledCheese
StoreManager       [StoreManager]  → CornerMart, Bodega
UIManager          [UIManager]     → Main Menu / Recipe Selection / Store Selection panels
                                     (Shopping List, Success, Failed slots are EMPTY)
Canvas (1920×1080 scale-with-screen)
 ├─ Main Menu Panel (starts OFF) ─ Main Menu Image ("grocery game menu time.png"), Play button
 ├─ Recipe Selection Panel (starts OFF) [RecipeCarouselUI]
 │    ├─ Header › Title Text "Select a recipe"
 │    └─ Navigation Container ─ Carousel Row (◀ Left Arrow · Recipe Display Area · Right Arrow ▶)
 │                            └─ Button Row (Select Recipe · Back)
 └─ Store Selection Panel (starts ON) [StoreCarouselUI]   ← same layout, "Select a store"
EventSystem [InputSystemUIInputModule]
Camera
Recipe Card  ← stray copy of the prefab sitting at the top level, outside the Canvas (leftover)
```
The arrow buttons show placeholder TMP text "Button" over the arrow images. Counter text slots are empty, so "Recipe 1 of 2" never shows.

### `Mock Up Store.unity`: the playable store (Corner Mart)
```
+++++STATIC ENVIRONMENT+++++
 ├─ Directional Light, Global Volume, Plane (floor)
 ├─ Primitive Shelf ×2, Primitive Fridge ×7 (built from Unity cubes)
 └─ Wall ×5
+++++INTERACTABLES+++++
 ├─ 16 grocery items [GroceryItem, Rigidbody, BoxCollider] on "Interactable" layer:
 │    Eggs ×3, Ketchup ×2, Tomato ×3, Wheat Bread ×3, White Bread ×5   (all set to Bread / $1.99)
 └─ Shopping Cart [ShoppingCart, Rigidbody, BoxCollider]
      └─ bottom, 4 side cubes, 4 wheel cylinders, Cart Handle + 2 cylinders, ItemContainer (unused)
Player [CharacterController, FirstPersonController]   ← at world origin
 └─ MainCamera (y = 2.0)
Canvas (OFF, constant pixel size) ─ Shopping List UI [ShoppingListUI], Item Examination UI [ItemExaminationUI]  ← never used
EventSystem
Player Spawn Point          ← unused
Shopping Cart Spawn [ShoppingCart only]  ← causes error E2
```
At runtime, code adds: GameCanvas (InteractionPrompt, ShoppingListUI, ShoppingListPanel, ItemExaminationUI), a second ItemContainer on the cart, and stray IngredientItem rows at the top level.
No checkout counter, register, signage, money or exit exist.

### `Bodega TEST.unity`: broken second store
Directional Light, Global Volume, floor Plane, 3 shelves, 7 fridges and 5 walls (the same primitive layout as Mock Up Store, not grouped), plus three **missing** Starter-Assets prefabs (MainCamera, PlayerFollowCamera, PlayerCapsule). There are no grocery items, no cart and no game scripts. Loading it shows a static view.

---

## 8. Art and assets already imported

### Grocery item models (`Assets/Project/Art/3D Models/`)

| Model | Triangles | Texture (size, filter, compression) | Material (shader) | Prefab? | In a scene? | PS1 fit |
|---|---|---|---|---|---|---|
| White Bread.fbx | **36** | White Bread.png 256², **Point**, DXT1, mipmaps ON | Bread (URP/Lit) | Yes | Yes (×5) | Texture ✓, filter ✓; very low-poly (a box) |
| Wheat Bread.fbx | **36** | Wheat Bread.png 256², Point, DXT1, mips ON | Bread02 (URP/Lit) | Yes | Yes (×3) | Same as above |
| Eggs.fbx | 164 | Eggs.png 256², Point, DXT1, mips ON | Eggs (URP/Lit) | Yes | Yes (×3) | ✓ (under the 300 target, fine) |
| Ketchup.fbx | 112 | Ketchup.png 256², Point, DXT1, mips ON | Ketchup (URP/Lit) | Yes | Yes (×2) | ✓ |
| Tomato.fbx | 200 | Tomato.png 256², Point, DXT1, mips ON | Tomato (URP/Lit) | Yes | Yes (×3) | ✓ |
| Lettuce.fbx | 88 | Lettuce.png 256², Point, DXT1, mips ON | Lettuce (URP/Lit) | **No** | **No** | ✓ (needs a prefab) |
| Milk.fbx | 80 | Milk.png 256², Point, DXT1, mips ON | Milk Carton (URP/Lit) | **No** | **No** | ✓ (needs a prefab) |

**Overall:** these all fit the PS1 look well: point filtering, 256×256 textures (well under the 512 cap), and low triangle counts (36–200, below your 300–800 range; the breads could take more detail). To match fully: turn **mipmaps off** (they blur point-filtered textures at a distance), and swap URP/Lit for an unlit or vertex-lit PS1 shader. Every model is modelled at roughly 2–5 units and shrunk to 0.2 on the prefab. **Fix the scale in Blender or the import settings** so prefabs sit at scale 1.
**Variants:** only one version of each product exists. White vs Wheat bread is the only "variant-like" pair. **There are no Generic / Standard / Premium versions** of anything.

### Missing for the two POC recipes

| Ingredient | Needed by | Status |
|---|---|---|
| Bread | both | ✓ White + Wheat (could serve as Standard / Generic) |
| Tomato | BLT | ✓ model + prefab |
| Lettuce | BLT | ⚠ model + texture, **no prefab** |
| **Bacon** | BLT | ✗ **missing** |
| **Mayo** | BLT | ✗ **missing** |
| **Cheese** | Grilled Cheese | ✗ **missing** |
| **Butter** | Grilled Cheese | ✗ **missing** |
| Generic/Standard variants | all | ✗ none exist |
| Eggs, Ketchup, Milk | not needed | exist (could be "extra items" for the pantry list) |

### Store fixtures, cart, people
Everything is **Unity primitive cubes and cylinders**: shelves, fridges, walls, floor and cart. There are no modelled fixtures, no checkout counter, conveyor, register, returns tray, cashier, money or signage.

### Prefabs (`Assets/Project/Prefabs/`)
- **Items:** Eggs, Ketchup, Tomato, Wheat Bread, White Bread. Each is mesh + GroceryItem + Rigidbody + BoxCollider on the Interactable layer, all with placeholder data (Bread, "Grocery Item", $1.99, Standard).
- **UI:** `Recipe Card` (1600×800, notecard background, TMP text), `Store Card` (same structure), `Ingredient Item` (one TMP line, 640×60). All use the default TMP font *LiberationSans SDF*.

### 2D / UI art (`Assets/Project/Art/UI/`), all Sprites, Bilinear filter

| File | Size | Used by | Note |
|---|---|---|---|
| BLT.png | 400×225 | BLT recipe | Dish image, reusable for the debrief |
| Grilled Cheese.png | 400×225 | Grilled Cheese recipe | Dish image, reusable for the debrief |
| Corner Store.png | 400×225 | Corner Mart card | |
| bodega thumb AI.jpg | 640×640 | Bodega card | Filename says AI-generated, so check it's OK for your course's policies |
| grocery game menu time.png | 500×300 | Main menu background | Stretched to 1920×1080, so it will look soft |
| notecard.jpg | 1024×614 | Recipe Card background | |
| left arrow.png / right arrow.png | 906×905 | Carousel arrows | Oversized for an 80 px button |
| Crosshair-PNG-Image.png | 2048×2048 | **Nothing** | Way oversized; unused |

For a PS1 feel, the UI art would want Point filtering too. It's currently Bilinear.

---

## 9. PS1 look

**Nothing exists yet.** There are no custom shaders (no `.shader` or `.shadergraph` files in the project), no renderer features beyond the template's SSAO, no vertex wobble, low-resolution render target, dithering, colour-depth reduction, affine texture mapping or fog. Render scale is 1.0 and MSAA is off. The volume profiles are the stock URP template (Bloom, Tonemapping, Vignette), which pull *away* from a PS1 look.
The only PS1-friendly choices already made are the **point-filtered 256×256 textures** and the **low-poly item models**.
Shader Graph is installed, so a PS1 shader could be built with it in the new project.

---

## 10. Lessons and patterns

How the five known lessons hold up in this project, plus new ones:

1. **Double-firing buttons:** *not reproduced here.* One click moved the carousel one step. The **risk pattern is present**, though: both carousels add their button listeners in code (`Start`), while the Back and Play buttons are wired in the Inspector. Wiring the same button both ways makes it fire twice. Also, the carousels listen for **Space/Enter** in `Update` *and* the EventSystem treats Space/Enter as "submit" on the selected button, so a keyboard press can trigger Select twice. **Rule:** wire each button exactly one way.
2. **Panels that start inactive don't run `Start()`:** **confirmed.** The Recipe Selection panel starts disabled, so `RecipeCarouselUI.Start()` (which hooks up the arrow and Select buttons) only runs one frame *after* the panel is first shown. Clicking during that frame does nothing (I reproduced this). The Store panel was left *enabled* in the scene, probably as a workaround, which is why it has the extra `OnEnable`/`DelayedInitialize` code and initializes twice. Related: `FindObjectOfType` can't see disabled objects, which is why the store scene's disabled Canvas is ignored and duplicated. **Rule:** set up button wiring in `Awake`/`OnEnable` or in the Inspector, and pass references in rather than searching for them.
3. **Store data flags:** **confirmed.** `isStartingStore` decides which stores appear; if none are flagged, the first one is shown with a warning. Both are flagged. `availableIngredients` is set up but never enforced, and it contradicts the recipes. `priceModifier` is 1.0 on both, so "Standard Prices" always shows. Scenes are linked by **typed name** (`sceneName`), so a rename breaks the link silently.
4. **Held item's collider blocking the cart raycast:** *handled in this version.* The held item's collider is switched off while held (`GroceryItem.cs:149`). But a **new version of the same problem** appeared: (a) the ray hits the cart's *child* colliders, which have no cart script; (b) "Queries Hit Triggers" is on, so the cart's trigger box catches rays too; (c) "place in cart" ignores the raycast entirely and uses a 2 m proximity check. **Rule:** use a dedicated drop-zone trigger on the cart and check `GetComponentInParent`. Exclude the held item and triggers with a layer mask.
5. **Physics cart pushing is hard to control:** **confirmed and worse than expected.** The cart is a non-kinematic physics body pulled toward a spot beside the player with `MovePosition`. Its pivot is at the handle, it sits off to the right (1.2 m), and its items are separate physics bodies parented to it, so they can slide, fall through or get left behind. Your plan (parent the cart to the player, items jostle a little) is the right call.

**New lessons**
6. **Pivot placement matters for gameplay.** Off-centre pivots (cart at the handle, items modelled huge and scaled to 0.2) cause placement bugs. Set pivots and scale in the modelling tool before import.
7. **Spawn markers shouldn't carry gameplay scripts.** The "Shopping Cart Spawn" empty with a ShoppingCart script crashed and created a phantom second cart.
8. **Placeholder data hides bugs.** All items are "Bread", so the list *looked* like it worked (tomato ticks Bread). Give every item real data from the start, ideally a ScriptableObject per product.
9. **Building UI in code made it fragile.** Three scripts each have their own "find or create GameCanvas" copy, and one of them has the container bug. Build UI as prefabs in the editor.
10. **Highlight with a component that tolerates child meshes.** `Outline` requires a Renderer on the same object, and it isn't really an outline (it's emission glow, which may not read well in a PS1 look).
11. **Deleting a package left a scene full of missing prefabs** (Bodega TEST). Check scenes after removing packages.
12. **The Editor needs focus to run Play Mode frames.** This matters if you or students drive tests through MCP.

---

## 11. Reuse recommendations

| System / asset | Verdict | Reason |
|---|---|---|
| Recipe ScriptableObject (`Recipe.cs`) | **Reuse with changes** | Good shape; add per-trip allowance and dish image/stars for the debrief; drop `difficulty`/`allowsQualitySubstitution`; point ingredients at ingredient assets instead of the enum |
| `IngredientType` enum | **Rebuild** | Swap for an Ingredient ScriptableObject (name, $–$$ hint, Generic and Standard product variants) so new items don't need code |
| Grocery item data (on prefabs) | **Rebuild** | Use a Product ScriptableObject (ingredient, variant, price, model) referenced by the prefab |
| StoreLocation ScriptableObject | **Reuse with changes** | Keep name/description/image/scene; drop `isStartingStore` and the unenforced ingredient list; use a scene reference instead of a typed name |
| GameManager state machine | **Reuse with changes** | The pattern is fine; add Checkout/Debrief states, the wallet, the pantry list and a return to the menu; give it direct links instead of `FindObjectOfType` |
| UIManager panel switching | **Reuse with changes** | Simple and works; fix the Back wiring and the empty slots |
| Recipe / Store carousels | **Reuse with changes** | Layout and flow match your POC; merge into one carousel script, strip debug logging, wire buttons once, initialize once |
| RecipeCardUI / StoreCardUI + card prefabs | **Reuse as-is** (minor tidy) | Work well; the notecard look is nice; fix the counter text and the store card size |
| FirstPersonController | **Reuse with changes** | Numbers are a good start; move to the new Input System, lower the eye height, use the spawn point |
| InteractionSystem | **Rebuild** | Right idea, but raycast-to-parent, hover-crash handling, layer masks and cart drop zones all need redoing; it also builds UI in code |
| GroceryItem pick-up / drop / examine | **Reuse with changes** | The hold position and examine feel are worth keeping; replace the "2 m from any cart" rule with a real drop zone |
| ItemExaminationUI | **Reuse with changes** | Behaviour is good; turn it into an editor-built prefab and fix the instructions text |
| ShoppingCart | **Rebuild** | Broken (E3/E4, wrong pivot, physics follow, phantom second cart); you planned a parented cart anyway |
| ShoppingListUI | **Rebuild** | Container bug, built in code, uses the old UI Text; rebuild as a prefab with $–$$ hints and the wallet fade-in |
| Outline highlight | **Rebuild** | Crashes on parent objects; pick a highlight that suits the PS1 look |
| AudioManager | **Drop** (rebuild later) | Placeholder only; not in any scene |
| IngredientItemUI, IngredientQuality | **Drop** | Unused |
| Managers scene layout | **Reuse with changes** | Menu flow works; remove the stray Recipe Card and set panels' starting states on purpose |
| Mock Up Store scene | **Drop** (keep as layout reference) | Grey-box only; the rebuild needs checkout, signage and real fixtures |
| Bodega TEST scene | **Drop** | Missing prefabs, nothing playable |
| Item models + textures (bread ×2, eggs, ketchup, tomato, lettuce, milk) | **Reuse with changes** | PS1-friendly; fix scale/pivots, turn mipmaps off, add Generic/Standard variants, make Lettuce/Milk prefabs |
| Recipe/store card images, notecard, arrows | **Reuse with changes** | Usable; resize the arrows, switch to Point filtering, check the AI-generated Bodega image |
| Main menu image | **Reuse with changes** | Low-res (500×300) for full-screen; redo at a deliberate low resolution if the pixelation is intentional |
| Crosshair PNG | **Drop / replace** | 2048² for a tiny reticle; make a small one |
| URP settings / volume profiles | **Rebuild** | Template defaults; set up a PS1 pipeline on purpose (low render scale or render texture, point sampling, fog, no SSAO/bloom) |
| Cinemachine, AI Navigation, Visual Scripting, Timeline packages | **Drop** | Not used |

---

## 12. Open questions for you

1. **Which store scene is "the" store?** Corner Mart → `Mock Up Store` (playable), Bodega → `Bodega TEST` (broken). Your POC loads one scene from both cards. Should the new store be based on Mock Up Store's layout, or start fresh?
2. **Corner Mart description** reads "They don't have the basics, but not much." Was this meant to be the opposite of the Bodega's line, or is it placeholder text?
3. **Quality tiers:** the old code has Generic/Standard/**Premium** (×0.7 / ×1.0 / ×1.8). Your POC lists only Generic and Standard. Drop Premium entirely, or keep the multipliers as a starting point?
4. **Recipe budgets ($12 BLT, $10 Grilled Cheese):** should these become the "fixed allowance each recipe adds", or are they a separate guideline?
5. **Extra recipes** (Instant Noodles, Mac N Cheese, Tacos) exist as data but aren't in the menu. Keep them for later, or drop them?
6. **Bread variants:** are White and Wheat bread meant to be the Generic/Standard pair, or two different products?
7. **Item scale:** models are built at roughly 2–5 units and shrunk to 0.2. Was that a deliberate export choice by the interns, or should the new models be exported at real-world size?
8. **"bodega thumb AI.jpg":** is the AI-generated store image OK to keep under your course's art policy?
9. **Main menu art** is 500×300 stretched to full HD. Intentional low-res look, or a placeholder?
10. **Eye height:** the camera sits at 2.0 m (very tall). Intentional (seeing over shelves), or should it be about 1.6 m?
11. **Cinemachine** was installed alongside the removed Starter Assets. Do you want Cinemachine in the new project (e.g., for checkout or debrief camera moves), or a plain camera?
12. **The `audit` branch** still has the MCP package changes in `Packages/manifest.json` and `packages-lock.json`, uncommitted. I left them as they were. Discard them when you're done with this branch.
