# Audit of the RatBite rebase onto modern Goobstation

## Scope
Goobstation version merged was `ab045fe6d9`.
RatBite version merged was `d5547b1c83`.
Common ancestor was `da34e93f8d`.
Original direct merge clashes: 500 paths.
Paths changed by both branches since the common ancestor: 1,158.
Current overlap classification: 220 RB exact, 168 GS exact, 770 combined.
RatBite first-parent changes to review: 254 total; 152 touch shared behavior, 50 authoritative resources, 45 only namespaced content, and 7 database surfaces.
Exact original clash paths and kinds are in `REBASE_CLASHES.tsv`. Post-edits are below in the compatibility fix changelog table.
Namespaced code moved fine. `_BRatbite` and `_BRatBites` paths. It is unknown if the 's' is intentional but I treated it as such, to be 'sorted out' later.
Map and map pool conflicts: 22 original map conflicts plus map prototype and pool conflicts. Keep RatBite map files; update moved prototype IDs and modern map serialization.
42 texture conflicts, including 29 binary image conflicts; intercom, BSO mask, modsuit, hologram, CE/CMO ensemble rename/delete cases. Keep intentional RatBite replacements as complete RSI units; verify every metadata state exists.
RatBite removed Goob rule pages; Space Law, species, first-character, and BSO SOP content conflicts. Preserve RatBite policy documents and repair guidebook references.
61 `Content.Shared`, 40 `Content.Server`, 22 `Content.Client` original clashes. Compare each combined hunk with RatBite behavior and modern API ownership.
Goobstation modules has 17 server, 13 shared, 6 client, and 5 common original clashes. Distinguish RatBite reworks from old copies of systems now native upstream.
Localization has 18 FTL direct conflicts plus automatically merged locale changes. Verify retained RatBite names and removed-feature strings after prototype validation.

## My Preservation Efforts Have Not Been In Vain!!

All 439 prototype IDs under `Resources/Prototypes/_BRatbite` on RatBite `master` are present in the rebase.

Key to status codes:
`audit`: behavior still needs comparison against RatBite history.
`resolved`: a reviewed migration decision has been implemented.
`partial`: a migration decision has been partially implemented; further review is required that I can't be arsed to do.

## High-risk behavior queues

First-pass review of RatBite changes that touch shared or server behavior. These are the most likely to require further review and validation.

| Status | RatBite change | Main systems |
| --- | --- | --- |
| audit | PR #369 cuffs | Restraints, interactions, prototypes, localization |
| audit | PR #378 combat-trained | Combat traits and shared combat behavior |
| audit | PR #354 heretic fixes | Heretic shared/server behavior and prototypes |
| audit | PR #458 security coin | Security economy/items across code and prototypes |
| audit | PR #516 contraband tweaks | Contraband classifications and affected items/loadouts |
| audit | PR #301 orbit rework port | Ghost UI/system, warp points, species/player prototypes |
| audit | PR #403 lathe wires | Lathe machine behavior, wires, UI, prototypes |
| audit | PR #367 paper drawing | Paper UI/network state, fax persistence, drawing limits and sanitization |
| audit | PR #364 trait adjustments | Trait systems and prototype values |
| audit | PR #413 shove immunity | Shove/standing/combat interactions |
| audit | PR #568 uranium ammo rework | Cartridge/projectile damage and reagent behavior across all ammunition families |
| audit | PR #544 reagent implanter port | Implanter shared/server code, actions, catalog, uplink, and prototypes |
| audit | PR #419 changeling nerf | Changeling abilities, components, costs, and prototypes |
| audit | PR #418 synthflesh buff | Reagent effects and reaction values |
| audit | PR #433 vending-machine tipping nerf | Tipping behavior and component/prototype parameters |
| audit | PR #462 pacification tweaks | Pacification interactions and traits |
| audit | PR #529 salvage nerf | Salvage rewards, equipment, expeditions, and related values |
| audit | PR #534 trap-avoider nerf | Trait behavior and trap interactions |
| audit | PR #535 atheist/believer tweak | Religion traits and chaplain interactions |
| audit | PR #483 kleptomania nerf | Trait behavior and selection values |
| audit | PR #507 lectrazine buff | Medicine reagent effects and values |
| audit | PR #453 shotgun accuracy fix | Pellet/spread or gun accuracy values |
| audit | PR #407 modsuit bug fix | Modsuit state and equipment behavior |
| audit | PR #579 departmental pings | Radio/department alert behavior |

## Ports now present in modern Goob

### Verified native-upstream files/assets

These were independently added on both sides after the fork and are byte-identical in modern Goob.

| Status | Former RatBite port | Evidence |
| --- | --- | --- |
| native-upstream | Energy reagent dispenser card XAML | Same path and identical content |
| native-upstream | Resomi marking prototype and Resomi chest/head/tail RSI assets | Same paths and identical content |
| native-upstream | Station AI appearance prototype, locale, and RSI states | Same paths and identical content |
| native-upstream | Vox sprites for bone helmet, bone armor, atmos fire suit, and BSO SWAT mask | Same paths and identical content |
| native-upstream | Single clap emote sound | Same path and identical content |

### Native-upstream candidates requiring behavior comparison

| Status | Candidate | Why it is a candidate | Review needed |
| --- | --- | --- | --- |
| audit | Energy reagent dispenser system | Client, server, shared, XAML, and prototype paths were independently added on both sides; implementations diverged | Compare UI messages, inventory/card logic, access, energy use, dispense limits, and prototype values before dropping RatBite edits |
| audit | Orbit rework | RatBite explicitly ported it in PR #301; modern Goob now owns the same ghost UI/system and warp-point surfaces | Compare target selection, orbit controls, warp behavior, and RatBite species changes |
| audit | Reinforcement store/ghost-role port | RatBite PR #405 touches modern store and ghost-role systems plus Trauma content | Determine which framework changes are native and retain RatBite catalog, currency, roles, and balance |
| audit | Changeling | RatBite-added changeling symbols now exist in modern Goob | Treat modern implementation as base; replay RatBite nerfs and local behavior changes explicitly |
| audit | Game director | RatBite-added `GameDirectorSystem`, `PlayerCount`, and `RankedEvent` symbols exist in modern Goob | Compare event ranking, population scaling, timing, and RatBite EORG/performance changes |
| audit | Pointing scale extension | RatBite-added partial `PointingSystem` surface now exists in modern Goob | Verify scale behavior and remove duplicate partial logic if native behavior matches |
| audit | Hemophilia | RatBite/Mono trait system symbol exists in modern tree | Compare bleeding multipliers and trait lifecycle |
| audit | Trigger trait/system | Symbol overlap exists but path ownership differs | Confirm this is equivalent behavior rather than a coincidental generic type name |

### Ports intentionally not retained

| Status | Port | RatBite history decision |
| --- | --- | --- |
| resolved | Thunderdome port | Added by PR #440 and reverted by PR #449; modern Goob may contain it, but RatBite's final-state intent is removal unless a new decision is made |

## PostgreSQL and persistence

| Status | Surface | Current state | Required review |
| --- | --- | --- | --- |
| audit | Player model | RatBite fields `BrigSentence`, `BrigTime`, `Inpatient`, and `PPpoints` remain in merged `Model.cs` | Verify nullability/defaults and all service/API consumers |
| audit | PostgreSQL migrations | RatBite migrations for permanent-brig sentences, brig time, and inpatient state are present | Rebase migration snapshots onto modern model; test upgrade from a real RatBite schema, not only a fresh database |
| audit | SQLite migrations | Matching RatBite migrations are present | Keep parity with PostgreSQL and test local development upgrade |
| audit | Context snapshots | Both PostgreSQL and SQLite snapshots are combined files | Regenerate/verify snapshots after the final model is settled |
| audit | Database manager/base | `ServerDbBase.cs` and `ServerDbManager.cs` are combined overlap files | Audit read/write paths for permanent-brig and inpatient behavior |
| remove-before-merge | Local database artifacts | `data/preferences.db`, `data/preferences.db-shm`, and `data/preferences.db-wal` arrived from RatBite history | Confirm they are not intentional fixtures, then remove from version control and protect with ignore rules |

## Decisions already made

| Status | Decision | Reason |
| --- | --- | --- |
| resolved | Work occurs in separate `rebase-modern-goob` worktree | Preserve the dirty `update-robust-toolbox` worktree and its 27 edits |
| resolved | Use modern Goob as source-code dependency baseline | RatBite source deletions removed types still required by modern consumers |
| resolved | Preserve RatBite maps, server policy content, and complete texture replacements | These are server identity/content, not generic upstream implementation |
| resolved | Restore `DropHeldItemsBehavior` compatibility enum | Retained stun APIs and RatBite callers still require the serialized contract |
| resolved | Remove two stale Lavaland files containing embedded historical conflict markers | Modern Goob already contains the valid upgrade component/system |
| resolved | Repair lock/access reader merge damage | Mindshield enforcement remains in `AccessReaderSystem`; the orphaned duplicate lock hunk was removed and `NeedsMindshield` was restored to the reader component |
| resolved | Remove changeling dependency from shared brain handling | `Content.Shared` cannot reference the downstream Goobstation changeling assembly; changeling systems retain ownership of that behavior |

## Compatibility fix changelog

This is the running ledger of foundation work completed during the modern Goob rebase. RatBite-owned values and content remain authoritative unless explicitly noted.

| Status | Surface | Fix and intent | Validation |
| --- | --- | --- | --- |
| resolved | Lock/access reader for stuff that needs mindshield | Removed an orphaned mindshield block that referenced nonexistent variables; restored `AccessReaderComponent.NeedsMindshield` and retained enforcement in `AccessReaderSystem`. | `Content.Shared` builds |
| resolved | Shared brain/changeling | Removed the old changeling guard from `Content.Shared` because the modern changeling component is downstream; changeling systems retain ownership of identity behavior. | `Content.Shared` builds |
| resolved | Emote API | RatBite has a specified voluntary emoty thingy on `TryEmoteWithChat`. This is restored. | `Content.Shared` builds |
| resolved | Footstep audio | Volume calc in movement sound path was changed, kept the RB walk/sprint modifiers for footstep audio when performing the said actions. | `Content.Shared` builds |
| resolved | Pacifist stamina | Renamed the stale `applyResistances` argument to modern `ignoreResist: false` to preserve functionality. | `Content.Shared` builds |
| resolved | Item-slot delays | Restored nullable serialized `InsertDelay` and `EjectDelay` fields and adapted `TryEjectToHands` to the modern four-argument API. | `Content.Shared` builds |
| resolved | Lavaland attachments | Replaced deleted `AttachmentBayonet` and `AttachmentFlashlight` dependencies with modern `GunUpgradeBayonet` and `GunUpgradeFlashlight` components; updated the seclite prototype marker. | `Content.Shared` builds |
| resolved | Mindshield alert | Passed the owning entity to the modern `AlertsSystem`, preserving the RatBite alert prototype and lifecycle. | `Content.Shared` builds |
| resolved | IV beam visuals | Kept IV visual targets as local `EntityUid` values for the modern `JointVisualsComponent` contract. | `Content.Shared` builds |
| resolved | Item-upgrade action relay | Adapted relay events to modern equip-only `GetItemActionsEvent` semantics and removed obsolete action persistence calls. | `Content.Shared` builds |
| resolved | Nudist stamina | Added a shared stamina-damage multiplier component driven by `BeforeStaminaDamageEvent`; preserved RatBite's existing clothing and base multipliers exactly. | `Content.Shared` builds |
| resolved | Goob changeling n shiz | Fixed Hypospray namespace, changeling random-seed collection, modern knockdown wrapper, void-adaptation lifecycle subscription, and generated component state. | `Content.Server` dependency builds |
| resolved | Goob mimery wizard shit that nobody will use (but I will >:D) | Restored inheritance for the active finger-guns projectile event and exposed the existing projectile/spawn helpers used by Mimery. | `Content.Server` dependency builds |
| resolved | Server chat contracts | Matched server chat overrides to the modern forced-message and forced-emote parameters. | `Content.Server` dependency builds |
| resolved | Server mousetrap stuff | Moved the Giant Mousetrap system to the shared mousetrap system/event namespace and updated the trigger event name. I'd kindly request that Perstronzio changes `_BRatbites` folder that Giant Moustrap stuff is in to `_BRatbite`... | `Content.Server` dependency builds |
| resolved | Server uplink/store | Restored the Goob common uplink dependency, manifest listing event import, and store timing dependency; removed duplicate merged StoreSystem fields. | `Content.Server` dependency build |
| resolved | Server ghost/body-cam/trait | Restored RatBite ghost population manager and shared surveillance/scale/brain component imports. | `Content.Server` dependency builds |
| resolved | Dehusk entity effect | Migrated RatBite Dehusk to the modern `EntityEffectBase`/`EntityEffectSystem` pattern without changing its dehusking behavior. | `Content.Server` dependency builds |
| resolved | Vending knockdown | Ported RatBite's two-second vending-machine impact stun to modern forced knockdown semantics. | `Content.Server` dependency builds |
| resolved | Lathe access logging | Restored the modern access-reader dependency and preserved RatBite quantity-aware queue logging. | `Content.Server` dependency builds |
| resolved | Lavaland server attachments | Restored the shared `SharpComponent` namespace for bayonet attachment behavior. | `Content.Server` dependency builds |
| resolved | Giant mousetrap arming | Replaced the removed mousetrap toggle helper with modern `ItemToggleSystem.Toggle`. | `Content.Server` dependency builds |
| resolved | EMP wearable duration | Converted RatBite EMP duration values to the modern `TimeSpan` contract without changing configured seconds. | `Content.Server` dependency builds |
| resolved | Battery input validation logging | Repaired the merged logger field reference while retaining RatBite NaN rejection. | `Content.Server` dependency builds. |
| resolved | Server compiler closure | Resolved remaining server compile contracts across role bans, ghost warp filters, PermaBrig chemistry/logging, status-effect drunkenness, gun upgrades, Nuke tracking, trait handlers, Felinid actions, storage, and modern database ban lookups. | `Content.Server` Release builds |
| resolved | PaperWindow XAML hierarchy | Repaired the mismatched drawing/control tags so typed XAML references generate correctly. | `Content.Client` Release builds |
| resolved | Paper drawing/save API | Restored drawing toggle, undo, clear, state refresh, and stroke-aware save callbacks across all paper save paths. | `Content.Client` Release builds |
| resolved | Accessibility UI controls | Restored RatBite pointer scale and pointer outline controls required by the accessibility tab. | `Content.Client` Release build |
| resolved | Ghost UI typed references | Restored alt-server/button/container names and the Trauma `OnGuiLoaded` extension hook. | `Content.Client` Release build |
| resolved | RatBite server IoC | Registered `PermaBrigManager` and `AltServerPopCountManager`, which were injected but absent from `ServerContentIoC`. | RatBite `Content.Server` startup smoke |
| resolved | Duplicate changelings | Removed the obsolete Common `ChangelingComponent` marker and consolidated `ChangelingActionComponent` onto the active Shared implementation. | RatBite `Content.Server` startup smoke |
| resolved | Startup prototype | Updated stale `Food`, `UpgradeableGun`, timer-trigger, battery-ammo, and sound-trigger prototype component names to their modern registered types. | RatBite `Content.Server` startup smoke |
| resolved | Duplicate GameDirector | Removed the stale duplicate GameDirector system and retained the partial modern implementation. | RatBite `Content.Server` startup smoke |
| resolved | Startup registration/prototype closure | Removed duplicate changeling registrations, migrated stale trigger/food/gun prototype names, and confirmed startup reaches round restart. | RatBite `Content.Server` startup smoke: `ticker: Restarting round!` |
| resolved | Atmos firesuit RSI metadata | Removed the second `equipped-OUTERCLOTHING-vox` state declaration from `atmos_firesuit.rsi`; the single existing Vox sprite remains unchanged. | Structural RSI scan; RatBite startup reaches round restart |
| resolved | RSI JSON/state metadata closure | Removed leftover merge markers from formal Blueshield metadata and removed its undeclared folded state; corrected shaft-miner metadata to match its available folded monkey sprite. | JSON scan; RatBite startup reaches round restart |
| resolved | RatBite texture path/metadata audit | Corrected the stale boombox `moblilespeaker.rsi` typo and verified every `_BRatbite` prototype RSI reference resolves into `_BRatBites`; all RatBite RSI metadata has valid unique states with matching sprites. | Focused path/state scan; client reaches main menu |
| resolved | PermaBrig join callback | Initialized `PermaBrigManager`'s sawmill at construction; IoC services do not invoke its unused manual `Initialize`, which caused a join-time NullReferenceException in `RemoveBrigTime`. | `Content.Server` and `Content.Goobstation.Server` Release builds |
| resolved | Reagents fucked | `MoveSpeedModifier` for a lot of reagents was changed in Goob to `MovementSpeedModifier` but was not updated for RatBite's reagents. This is fixed now. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | Thunderdome weapons locked | Added a component to the Thunderdome dudes that bypasses weapon locks `FiringPinExemptComponent`. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| partial | Guidebook fucked up priorities | Not exactly 'resolved' yet. I made priorities of RB's rules 0 so newbies can see the rules and oldies can cope (they probably know where to look as its mostly the same structure just some things are fucked) | `Content.Server` and `Content.Client` builds successfully, and works(?) in-game |
| resolved | Server info & emotes tab | Readded this cool menu, I dunno why Goob removed it. Probably because of emote spam. But they're chuds that don't like fun so w/e. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | CrateCombatTrainingBook was missing | Readded this crate that had an invalid EntProtoId error. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | Plushies attack rate was slowed down because of spam | I put it back to normal because the only people who complain about shit like this are whiny chudbabies. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | theres an evil fuckin `../../../Scrubs/blue.rsi` thats givin me some problems cuz its turning all my textures into stupid little rocks, so im climbing up its callbacks while its up there in its throw-backs and im punching evil blue.rsi in its tiny little cock | RSI wasnt loading. Thanks, lifegrips, very cool. Also fixed some other rsi errors. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | Default preset was Secret Classic | Switched default to SusRat. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | Default map was Saltern, which fucking sucks | Switched default to Opticon if no map selected. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | I lied and the above did not work. Turns out some shits missing. | Fixed missing prototype `SyringeCognizine`. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | Stale device network configurators | Removed invalid `configurators` entries from device network components in various maps. Turns out Opticon had 22 such non-entries. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | CE Loadout brokey | Readded missing CE loadout, as well as two scarfs that were causing errors. | `Content.Server` and `Content.Client` builds successfully, and works in-game |
| resolved | Ghost UI wouldnt show shit | Fixed the Ghost UI so it displays correctly by changing `Populate()` to clear the grids instead of destroying `ButtonContainer`. | `Content.Server` and `Content.Client` builds successfully, and works in-game |

## Stuff to fix

- modern ghost-role requirements
- lathe access-reader merge damage
- database role-ban API changes
- uplink command naming
- felinid/nuke/tracking dependencies
- Lavaland Sharp and gun-upgrade component migrations
- material-storage verbs
- heretic polymorph dependency
- perma-brig, EMP, trait, storage, chemistry API ports
- Slasher machete hit sfx no work
- Xenobio console UI dont work
- Some music displays as "Unknown title by Unknown artist"
