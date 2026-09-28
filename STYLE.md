# STYLE.md — visual style guide

House visual language for the desktop UIs in this repo (quickshell bar & panels
in `hm-modules/quickshell/`, the standalone clipper in `scripts/clipper.nix`,
launchers, OSDs, etc.). **Read this before building or restyling any UI, and
append a dated note to the Changelog whenever a new visual decision is made** so
later UIs match from the start instead of drifting.

Theme lineage: `material-monokai` (see `hm-themes/`) — dark teal surfaces, a
single lime accent, mono nerd font.

## Font

- **`ComicShannsMono Nerd Font`** everywhere (`fFamily`). Nerd-font glyphs are
  used for icons; insert them via `\u` escapes and verify with hexdump — editors
  can silently strip them (see the quickshell nerd-glyph memory).
- **Prefer solid nerd-font icons** (filled shapes, e.g. desktop
  ``, video camera ``) over outline-style variants when both
  exist for a concept.
- Typical sizes: body 15–17px, small/labels 12–14px, hero/OSD 26–50px.

## Colour palette

Surfaces (opaque, from the clipper) / panels sometimes use an `b3`/`aa` alpha
prefix when floating over the desktop (e.g. bar `cBg = #b3192227`).

| Token                            | Hex                | Use                                             |
| -------------------------------- | ------------------ | ----------------------------------------------- |
| `cBg`                            | `#192227`          | window background                               |
| `cPanel`                         | `#263238`          | raised panel / button face                      |
| `cInset`                         | `#11181c`          | sunken areas (video area, slider track, inputs) |
| `cHover`                         | `#314048`          | hover fill on panels/buttons                    |
| `cBorder`                        | `#3a4a52`          | 1px borders                                     |
| `cText`                          | `#ffffff`          | primary text                                    |
| `cTextDim`                       | `rgba(1,1,1,0.55)` | secondary text, inactive tab labels             |
| `cAccent` / `cHighlight`         | `#9dff00`          | **the** accent — interactive/active/lime        |
| `cAccentDark` / `cHighlightDark` | `#8db946`          | pressed / hover-underline / slider fill         |
| `cWarning`                       | `#cc7a00`          | warnings                                        |
| `cCritical`                      | `#BF616A`          | errors/critical                                 |

Semantic pairs:

- **old / original = green `#9dff00`**, **new / modified = red `#ff6b6b`** (used
  in the clipper for "post-clip vs original" runtime & filesize readouts).
- There's exactly **one** accent (lime). Don't introduce a second accent hue for
  emphasis; use bold / dim / the accent underline instead. Multi-series charts
  may use `svPalette` (`["#9dff00","#00b4d8","#e040fb","#ff9100",…]`).

## Components

### Tabs — accent underline, full-width (NOT a pill/box)

The house tab style: no background pill, no border box. Just a **3px accent
underline** under the active tab. Active label = accent colour + bold; inactive
= `cTextDim`; hover = faint `cAccentDark` underline. **Tabs equally fill the full
horizontal width** — each tab is `Layout.fillWidth: true` with equal stretch (no
trailing spacer), the label centered, and the underline spans the whole tab.
Vertical padding stays snug (~1px above the label + the 3px underline at the
bottom); don't inflate it. Applies to every tabbed UI — the recording popup, the
clipper (`scripts/clipper.nix`), and the audio Output/Input strip
(`hm-modules/quickshell/audio.nix`) all use full-width tabs. Also serves as the
title of a titleless popup.

### Buttons (`CtlButton` in the clipper)

**Prefer an icon over a text label** whenever a glyph makes the action clear
(save, copy, close, play, mute, record, …); only fall back to a text label when
an icon would be ambiguous about what the button does. Don't show keyboard-hint
text on buttons — instead, give every icon button a **hover tooltip** naming the
action and its shortcut (e.g. "Copy (Ctrl+C)"). Tooltip = a small `cInset`
rounded rect + 1px `cBorder`, `cText` label, anchored above the button (`z` above
neighbours), shown while hovered (see the clipper's `CtlButton`).

Rounded rect (`radius 5`), `cPanel` face, 1px `cBorder`, fill → `cHover` on
hover with an 80ms `ColorAnimation`. Disabled state = `opacity 0.4` and the
hover/click disabled (don't hide the button). **Primary action buttons are
icon-only** — no text label, no keyboard-hint text (standard glyphs: save =
floppy `󰆓`, copy = two-page `󰆏`, close = X `󰅖`, play/pause `󰐊`/`󰏤`, mute
`󰝟`/`󰕾`). Keep control bars responsive: when the window gets small, drop the
least-essential readouts (e.g. the clipper hides its runtime + size labels below
~700px) so action buttons never get pushed off-screen; give a slider
`Layout.fillWidth` with a `minimumWidth` so it shrinks instead of overflowing.

### Sliders (`MiniSlider`)

Track = `cInset` with 1px `cBorder`, radius 3, height ~6px; filled portion =
`cAccentDark`; round handle = `cAccent` (brighter when pressed). No QtQuick
Controls — hand-rolled from Rectangles + a MouseArea.

### Gauges (round / speedometer volume dials)

Volume gauges are **speedometer semicircles** — an arc wrapping the outside of a
centred app/device icon, with the trimmed title beneath. Range is **0–200%, with
100% at the exact midpoint** of the arc (fill fraction = `value/200`); 0% = min
(one end), 200% = max (the other). Adjust by **scroll-wheel** and by **dragging
along the arc**; a click on the icon toggles mute.

**Velocity-sensitive snapping (applies to ALL gauges/sliders that drag):** the
increment granularity scales with pointer speed — move slowly for fine control
(1% steps), move fast to snap to coarse steps (5%, then 10%). Track the pointer
delta per move (px/ms); small delta → 1% quantum, larger → 5%, largest → 10%,
snapping the result to that multiple in the direction of travel. Keyboard/scroll
use a fixed 5% step. This is the house feel for every draggable dial or bar.

### Progress bar

Same as a slider track but the fill animates by fraction (120ms `NumberAnimation`
on width), accent fill, with a right-aligned `NN% · ~Xs` dim label.

### Spinner

Braille frames `["⠋","⠙","⠹","⠸","⠼","⠴","⠦","⠧","⠇","⠏"]` cycled on a ~90ms
`Timer` while busy (plain Unicode, renders reliably in the mono font).

## Conventions

- **Radius:** 5 for buttons/inputs, 6–8 for panels/large areas, 3 for slider
  tracks / thin bars.
- **Spacing:** ColumnLayout/RowLayout `spacing` ~8–12px; window margins ~12px.
- **Hover feedback** everywhere interactive: `cursorShape: PointingHandCursor`
  and an 80ms colour transition.
- **Behaviour on colour** (`ColorAnimation { duration: 80 }`) for state changes.

### Live/polled lists must be visually frozen while open (NO churn)

Any menu that renders a list backed by an **asynchronous or polled** data
source (audio devices, Bluetooth, tailnet/cast peers, notifications, …) must
never reorder, insert, or remove rows while it is open. Watching items blink
out and back — or the whole list rebuild on every poll — is the single worst
UX regression this repo has hit, repeatedly. The rules:

1. **Source of truth is a persistent store, not the view.** A durable
   dictionary keyed by a **stable per-device id** is the single source of
   truth. The view renders from that store; it never derives order or
   membership from a raw fetch.
2. **The store is maintained event-driven, out-of-band — never scanned on
   open.** Each device source has its own hook that subscribes to that
   source's change events (`pactl subscribe`, BlueZ DBus signals, zeroconf
   browse, the phone server's connect/disconnect, …) and updates the store as
   events fire. Sources with no push (e.g. tailnet mesh) get a **background**
   refresh — never a blocking one triggered by opening the menu. Opening must
   never kick off a scan; that is exactly the delay we are avoiding.
3. **Updates are atomic + idempotent.** A hook mutates the store in one swap
   (temp-file + rename, or a single guarded write) once it has a complete
   result — a half-finished update must never be observable.
4. **Absence is a state, not a deletion.** When a device goes away, mark it
   `visible=false` — do **not** remove it. An authoritative event (BlueZ
   disconnect) flips it immediately; a polled source flips it only after a
   small consecutive-miss grace so one flaky poll can't. Confirmed-gone
   entries are pruned from the _rendered_ list only on the next open.
5. **Frozen while open = zero movement.** On open, the rendered order and
   membership are snapshotted from the store. Until the menu closes, the
   _only_ permitted change to an existing row is **in-place restyling** — a
   device that flips to `visible=false` is greyed + struck-through + made
   unselectable, staying exactly where it was. No re-sort, no insert of
   newly-appeared devices, no removal.
6. **Open is instant — a pure read.** Rendering on open just reads the warm
   store. No scanning, no recompute, no async fetch blocking first paint.
   Reopening re-reads the store: `visible=false` entries drop out, new ones
   appear, order refreshes.
7. **Selection is off-model + optimistic.** "Current" is tracked separately
   from the list model (so marking a selection can't reassign/rebuild the
   array), and set optimistically on click so one tap switches once.
8. **An empty read never clears a non-empty store.** An enumeration that
   returns NOTHING while the store has entries is a failed/raced scan (pactl
   contention, killed helper) — not "everything vanished". Skip applying it
   and keep last-known data; the next good read reconciles. This applies to
   the BAR's widget-membership feeds too (mic/sink lists, auto-mic state):
   applying an empty read collapsed the per-mic triggers to the default one
   and reshuffled the whole bar (found 2026-08-29).

If a list is churning, the fix is almost always to move state _out_ of the QML
view into an event-maintained store and render from it — not to add more sort
tiebreakers or debounces to the view.

## Changelog

- **2026-09-23** — **Remote-desktop picker** (`hm-modules/quickshell/remote.nix`,
  Meta+D → `qs ipc call remote toggle`): a keyboard-summoned **centered
  wallpaper gallery** of every tailnet device offering a remote-desktop
  service. Summon plumbing reuses the soundboard picker verbatim (always-mapped
  PanelWindow, empty input mask while closed, exclusive keyboard focus, pill
  search box for focus + fuzzy filter) but the body is **centred on screen**
  (`anchors.centerIn`, dim `#66000000` backdrop, opaque `#f2192227` panel) — NOT
  anchored under the bar like the list pickers — and renders a **card grid**
  (up to 3 columns, `cPanel` cards radius 8). Each card = a **16:9 wallpaper
  thumbnail** of that machine (`cInset` frame; remote-desktop glyph `󰢹` fallback
  while absent) above the host name (bold) + dim lowercase **protocol badge**
  (`rdp` / `vnc` / `moonlight`). Selection = 2px accent border; arrow keys move
  in 2-D across the grid (Up/Down = ±columns), gone devices are skipped/greyed
  (`opacity 0.45`, strikethrough) in place. Backing store per the live-lists
  rules: `remote-scan` (scripts/remote-desktop.nix) probes peers for
  3389/5900/47989/48989, atomically merges into
  `$XDG_STATE_HOME/qs-remote/devices.json` (2-miss grace before `visible=false`,
  empty tailscale reads never clear it), and best-effort fetches each host's
  wallpaper over SSH into `…/qs-remote/thumbs/<name>.jpg` (resolved REMOTELY:
  `~/bg` on our Hyprland hosts per swww, else the Plasma `Image=` path on
  SteamOS/Plasma hosts like the Frame; downscaled 400×225 via imagemagick).
  Open is a pure store read + a detached rescan that lands on the NEXT open
  (thumbnails too). Enter/click → `remote-connect` picks the client (xfreerdp /
  vncviewer / moonlight); RDP has no key auth so the password is prompted once
  via zenity and cached at `…/qs-remote/<user>@<host>.pass` (mode 600).

- **2026-09-10** (later) — nbt-web **player view v2 + server view** (supersedes
  the form-flow layout below): the player view is a **card grid** —
  `pv-main` auto-fit cards (position, abilities, experience, spawnpoint,
  full-width effects + items) beside a **300px right panel** (`pv-side`:
  quick-actions card on top, vitals below; rarely-touched info lives right,
  frequently-edited left/top). Cards are `cPanel` radius 8 with the dim
  12px title; rows are a **fixed 92px dim label column + controls**
  (72px in the side panel); rows for absent tags are omitted, and
  game-mechanic hint text is gone. Booleans are **switches** (inset pill
  track, `cAccentDark` track + accent knob when on) and ranged values are
  the house **mini slider** in DOM form. Effects are `cInset` **cards that
  flex-fill and wrap** (min 250px): icon + id input side by side, `×`
  top-right, and a **dashed add-card** (matches the drag-panel idiom)
  opening a modal whose effect field is a **datalist combo** (typed input
  - scanned suggestions). Item editing happens in a **modal** (opaque
    `cBg`, accent title, footer = secondary actions left / spacer /
    destructive + primary right): icon preview, count slider capped at
    stack size with a free number box, enchantment rows. Slots show their
    **name underneath** the 58px icon box; armor+offhand hang **vertically
    right of the main grid**. Server overview = `p-card` grid (3D spinning
    skin, name under) + right column of **large full-width file buttons**
    (`sv-file`). Topbar gained back-up/restore; restore is a `cPanel`
    dropdown menu like the tag-search results.

- **2026-09-10** — nbt-web **player view** (re-recording a note lost to a
  merge, extended same day): playerdata files open in an abstracted form
  view with a **full-width accent-underline tab pair** (`player · raw nbt`);
  both views mutate the same model. Form idiom: always-visible **labelled
  `cInset` inputs** (dim label left, dim tiny hint after, `cCritical` border
  on invalid input); boolean tags are **toggle chips** (cPanel, accent
  border + accent label when on). A player's other files (graves, mod data)
  are a **chip strip** above the tabs — active file chip accented, dim
  player name leading. Potion effects are **cPanel card rows** of the same
  fields + chips with a `×` remove. Inventory is ONE pane with a
  **file-browser crumb trail**: a `pv-select` dropdown of root inventories
  as the first crumb, then borderless text crumbs (`/`-separated, current =
  accent + bold), a right-aligned find-item input that **dims (0.22
  opacity) non-matching slots**. Slot cells are 58px `cInset` squares:
  extracted **item icon 34px `image-rendering: pixelated`** (mod-jar
  endpoint → vanilla CDN → text-label fallback), count bottom-right
  `cAccentDark` bold; **items containing sub-inventories get a dashed
  border + ▸ corner marker** and double-click descends. Global tag-path
  fuzzy search lives in the topbar; results are an absolutely-positioned
  `cPanel` dropdown (path dim / value `cAccentDark` / type badge, selected
  row = 3px accent left border), Enter edits the value inline in the row.
  Files deep-link via `#path=<rel>`.

- **2026-09-09** — **nbt-web** (`~/Source/nbt-web`, its own flake, deployed on
  GLaDOS): second standalone **web** UI after the phone-mic page, same rules —
  house palette as CSS vars, `ComicShannsMono Nerd Font` with `ui-monospace`
  fallback, **no nerd glyphs** (plain unicode carets `▸/▾`, text-label
  buttons). Layout: sidebar (fuzzy-find input + indented folder tree, active
  file = accent border + accent text like launcher cells) beside an editor
  pane with a `cPanel`-button topbar (primary action = accent border + accent
  label, destructive hover = `cCritical`). NBT tree rows: dim bordered
  lowercase **type badges**, values in `cAccentDark` (strings plain white in
  dim quotes), click-to-edit swaps in a `cInset` inline input with an
  `cAccentDark` border; row hover reveals its actions (`+`/`×` mini-buttons).
  Modified state uses the semantic **red `#ff6b6b`** ("modified" chip + edited
  values), matching the clipper's old/new pair.

- **2026-09-09** — Audio panel per-app gauges gained a **stream-filter (FX)
  switch** under each gauge's name: a miniature **3-tab strip**
  (`off · voice · +bass`) reusing the accent-underline tab idiom at gauge
  scale (labels `fSize-7`, 2px underline). **Deliberate exception to the
  full-width tab rule:** the tabs are content-sized and **centered under
  the gauge they act on** (Row spacing ~7px) so the switch reads as
  belonging to that app, not as a panel-wide strip. The selected tab —
  including "off" — is plain **accent + bold + accent underline** (house
  active-tab styling, no special off-state colour). Inactive labels dim
  white (0.4, 0.65 on hover) with a faint `cAccentDark` hover underline.
  Clicking a tab sets the preset directly (no blind cycling) with an
  optimistic local selection while the daemon's ~1s apply round-trip
  completes. Text-over-icon exception: preset names are the clearest
  possible label at this size and double as state. While an app is pinned,
  its gauge drives the post-filter trim and shows the blue measured gain
  arc exactly as when balance parks it on a slot. Adjacent gauges' strips
  must never touch: the audio popup widened 440→480 and each strip
  auto-shrinks (visual `scale`) to fit its cell with a guaranteed side gap
  when rows pack 4-5 gauges.

- **2026-09-07** — Modpack site (nix-minecraft `web.nix`): the inset address
  code gets a bold dim **`IP:` label** to its left. Pack-meta badges
  (version + mod count) are **right-aligned** in the pack header. The
  mod-count badge doubles as the **mod-list dropdown toggle** (accent ▸/▾
  suffix, accent border on hover) — the searchable list still opens in its
  usual spot under the pack description; the old "Mod list" summary row is
  gone.

- **2026-09-06** — Modpack site (nix-minecraft `web.nix`): third-party platform
  icons must be the **real brand logos** (vendored in `web-assets/`), never
  hand-drawn SVG recreations. Launcher tabs live **per card** (accent-underline
  style, synced across cards + how-to via one cookie). Cards split into a
  server-info head (title with subtle inset address underneath left; chips over
  players/TPS column right) and a dashed, draggable pack panel (drag = Chromium
  DownloadURL of the active tab's artifact). Cards flow into a 2-column grid
  when the viewport allows (`auto-fill, minmax(520px, 1fr)`).

- **2026-09-06** — **Horizontal battery indicator** (`hm-modules/quickshell/battery.nix`):
  the bar battery is no longer a nerd-font glyph + " NN%" beside it — it's a
  **drawn horizontal battery**: a sunken `cInset` body (radius 3, 1px `cBorder`)
  with a **proportional charge fill** and the **`NN%` centred INSIDE** the body,
  plus a small terminal **nub** on the right. The fill is the level colour at
  **0.55 alpha** (translucent) so the white percentage stays legible even at a
  full/green body; level colour = accent healthy, `cWarning` ≤30 %, `cCritical`
  ≤15 %. There is **no separate bolt glyph** for charging — charge state reads
  from the fill level + colour. Fill width animates 220ms `OutCubic`. Compact/space-saving mode drops
  the inner % and narrows the body (the bar reflow's `pctWidth` = the width that
  saves). The gallery card renders the same battery, scaled up. Pattern: a
  level-meter widget draws its own shape (body+fill+inner-label) rather than
  leaning on a glyph, translucent fill keeps overlaid text readable.
- **2026-09-06** — **Battery outline matches the fill it covers**
  (`hm-modules/quickshell/battery.nix`): the body outline is stroked in a Canvas
  to be the **exact same colour as the section it sits on** — the charged portion
  is the fill colour (the level colour composited at the fill's **0.55 opacity**
  over the `cInset` body) and the empty portion is **white**, with the colour
  break sitting exactly at the charge edge. The fill runs **flush to the body
  edge** (matching `body.radius`, clipped) so the charged section reads as one
  solid object with **no dark perimeter ring**, and the same-coloured outline
  vanishes into it — only reading as a border over the empty track. The nub
  matches the outline's far end: white unless full/100 %, then the fill colour.
  (Supersedes an earlier charging-sweep-animation border, now removed.) Pattern:
  to make a border disappear into a translucent fill, stroke it with the fill's
  _composited_ colour, not the opaque source colour.
- **2026-09-06** — **Widget Gallery v3 — it's a widget MANAGER**
  (`hm-modules/quickshell/gallery.nix`): the gallery popup now lists **every
  registered widget**, not just the parked ones — pinned/on-bar widgets stay
  listed (dimmed to 0.78 so parked ones stand out) so they can be **unpinned
  from here** (previously a pinned widget vanished from the gallery, leaving no
  way to unpin it). The card grid is wrapped in a **Flickable** capped to the
  screen height so the now-longer list scrolls instead of overflowing on tall /
  high-DPI monitors. **All** remaining always-on core widgets are now
  gallery-pinnable (`defaultPinned = true`): **workspaces, clock, system tray,
  audio output, audio input, battery** — each gated on `GalleryState.barVisible(id)`
  (Repeater widgets by emptying their model when parked; clock/workspaces by
  `visible` since invisible-but-positioned keeps the anchor geometry that
  media/rebuild/mediaAvail depend on). Battery ANDs in a `selfShown`
  (`hasBattery`) so it still self-hides on battery-less desktops even when
  pinned; the injected-widget gallery register now carries `comp` + a
  `defaultPinned` flag. The bar-icon parked-count badge meaning is unchanged
  (widgets currently OFF the bar).
- **2026-09-06** — **VMs widget is now gallery-gated** (supersedes the 2026-09-02
  "always on the bar" note): the VM widget parks in the gallery and auto-promotes
  to the bar only while a VM is **active** (running / installing / paused) or when
  the user pins it (`attention = runningCount > 0`, `defaultPinned = false`). Its
  gallery card is the server glyph accented + a live "N VMs active" line. The OS
  **distro-icon set expanded** well beyond the original handful — dedicated
  nf-linux glyphs now cover mint, pop!\_os, zorin, elementary, devuan, deepin,
  kali, mx, endeavouros, garuda, artix, manjaro, alma, rocky, centos, redhat,
  tumbleweed, leap, opensuse, gentoo, alpine, void, solus, mageia, slackware,
  freebsd (server stack still the unknown-OS fallback). Verify new distro
  codepoints against the installed Nerd Font (fontforge) before use — the
  nf-linux block is not fully contiguous (deepin/void/kali/pop/etc. live at
  F31D–F33F, not beside the F30x classics).
- **2026-09-05** — **Voice-call closed captions** (Vesktop plugin,
  `hm-modules/vencord-plugins/ClosedCaptions`): rendered **inside Vesktop** (plain
  DOM). **Exception to the house palette:** because this UI lives _inside_ Discord
  and is distributed to others, it **follows Discord's own theme**, resolved
  **per-token at runtime** (`resolvePalette()` reads `getComputedStyle`): the base
  is Discord-native / Vencord vars (`--background-secondary`, `--text-normal`,
  `--header-primary`, `--brand-experiment` blurple, `--text-muted`, `--text-danger`,
  `--font-primary`), and a **BetterDiscord-style theme overrides only the tokens it
  defines** — MaterialMonokai's RGB-triplet vars (`--accentcolor 157,255,0`,
  `--backgroundsecondary`, `--framecolor`, `--textbrightest/brighter/dark`,
  `--dangercolor`, `--font "Comic Mono"`), wrapped in `rgb(var(...))`. Every token
  has a hard fallback. Lesson: a UI inside another themed app must adopt _that
  app's_ theme vars — and support BOTH the app's native names and the popular
  theme-framework names, per-token, since a theme may set only a subset.
  The primary view is a **docked right sidebar** (not a floating card): it is
  inserted as a **real flex column at the end of Discord's own layout row** (the
  guilds-rail's parent, `[class*="guilds_"]`.parentElement; `flex:0 0 340px`,
  `align-self:stretch`) so Discord's layout engine reserves the space and content
  reflows within the window — the native-sidebar behaviour. Hiding it is
  `display:none` (space returns instantly). Lesson learned the hard way: **reuse
  the app's existing layout containers** — an earlier attempt that shrank
  `#app-mount` via `right:340px` shoved content off-screen; docking as a flex
  sibling is correct. A MutationObserver re-docks it if a re-render detaches it;
  fixed-overlay is the fallback only if the row can't be found. Full height, flush right, single
  left border, themed thin scrollbar; header = accent "Captions" + dim engine
  status + `×`; rows `Name: text` (name bold, interim muted); auto-sticks to
  bottom. **Toggle "CC" buttons are injected into Discord's own control bars** (the
  call-controls bar + the bottom-left voice panel) by **cloning the screen-share
  button** (inherits native size/hover/theme), swapping its icon for "CC" and
  rewiring the click — robust to hashed classes; re-injected via a throttled
  MutationObserver; accent while open. A vertical **"Captions" edge tab** is the
  fallback. The old bottom floating caption strip still exists but is **off by
  default** ("we don't need the pop-ups"). No nerd-font glyphs (font not
  guaranteed when distributed). Header also carries a **processing-level pill**
  (`Auto · Live/Reduced/Finals only`) that turns `--text-danger` when the GPU
  governor has stepped down (e.g. for a running game), click-to-override via an
  in-header dropdown whose High/Medium rows carry a `--text-danger` "more GPU"
  warning sub-label. Pattern: a live self-adjusting status that doubles as its own
  control, with the destructive/expensive options pre-warned inline.
- **2026-09-02** — **VMs bar widget** (`hm-modules/quickshell/vms.nix`): server
  stack glyph `\U000F048B`, accent + a small running-count when any domain is
  live, always on the bar (no gallery parking — "what VMs exist" should be one
  hover away even when nothing runs). Popup rows are house chips (`cPanel`
  face, 1px `cBorder`, `cHover` on hover): state dot + name + dim state label;
  state colours: running = accent, installing/paused = `cWarning`, managed-save
  "saved" = the alternative blue `#00b4d8`, shut off = dim. Each VM is ONE
  **borderless card** — layers are told apart by bg shade alone (popup dark →
  card `cPanel` → preview `cInset`), never by borders — with the live preview
  INSET inside the card under the header row. Action errors (virsh stderr)
  print as a `cCritical` wrap line above the footer — no silent buttons.
  Running rows carry sleep (`\U000F0904`) + power-off (`\U000F0425`, hover
  `cCritical`) icon buttons. Rows LEAD with an
  **OS glyph** (windows `\U000F05B3`, nixos/ubuntu/debian/arch/fedora/tux from
  nf-linux, server stack when unknown) tinted the state colour. Row click
  opens the VM (win11 via `windows` with hibernate-on-close, others via
  `vm-open`); every row gets an icon-only open button (`\U000F040A`
  nf-md-play), running rows also a hibernate button (`\U000F0904`
  nf-md-power_sleep). Running VMs show a **live preview thumbnail** under
  their row (`virsh screenshot` PPM in `$XDG_RUNTIME_DIR`, `cInset` frame,
  click-to-open) and the popup footer shows aggregate running-VM
  `cpu NN% · mem N.N GiB` (dim, right-aligned) — both sampled on a 2s timer
  ONLY while the popup is open (the cast-popup precedent). Membership/state
  data is event-driven per the no-polling rule: a user
  service (`vm-state-watch`) subscribes to `virsh event --all --loop` and
  atomically rewrites `$XDG_RUNTIME_DIR/quickshell-vms.json`; QML just
  FileView-watches it (single watcher, per-screen triggers only read).
- **2026-08-30** (later) — **Widget Gallery v2** — cards render the widget's
  **real bar visual**, not a generic glyph. Each participating widget hands
  `GalleryState.register()` an optional QML **Component** (`comp`); the card
  renders it via a `Loader` (glyph is now only a fallback when no comp is
  supplied). Custom card visuals: **monitors** reuses its miniature
  multi-screen layout; **disk** shows a used/free usage **donut** (reusing
  `PieChart`, `showLegend:false`) with the disk glyph centred; **system** draws
  **overlaid cpu/mem/gpu sparklines** (a small `Canvas`, colours cpu `#3a8eff` /
  mem `#b066ff` / gpu `#3ddc6c`, soft 0.12-alpha fill under each line) over a
  live legend, fed by a new always-on `SystemHistory` singleton (ring buffer,
  2s sample, one sampler for the whole shell). Pin/Silence are now **icon-only
  floating chips** in the card's **top-left / top-right corners** (22px rounded
  squares, absolutely anchored so they never shift the centred visual), NOT a
  bottom control row. Removed the parked-count **badge** on the bar icon and the
  "N widgets" header count (visual clutter). **All** bar widgets are now
  gallery-able for customisation — widgets that were always on the bar
  (media/MPRIS, builds/rebuild, cast, network, notifications) register
  `defaultPinned = true` (a per-widget `defPin` default in `GalleryState`, with
  explicit pin overrides — including explicit `false` — persisted), so nothing
  vanishes after a rebuild until the user deliberately unpins it into the
  gallery. Self-collapsing widgets (media has-a-player, rebuild is-building,
  notif has-notifications) gate through a `galleryShown` bool ANDed with their
  own condition rather than having `visible` clobbered.
- **2026-08-30** — **Widget Gallery** (`hm-modules/quickshell/gallery.nix`): a
  new bar entry point (solid grid glyph `\U000F0570`, nf-md-view_grid) opens a
  popup collecting low-priority widgets that aren't currently pinned to the
  bar, each rendered LARGER and centred inside a `cPanel` **card**
  (radius 8, 1px `cBorder`, `cHover` on hover). Cards are laid out in a 2-column
  `GridLayout`, equal-width (`Layout.preferredWidth: 1`), each showing a big
  centred glyph (~2.1× body) + centred label + a per-widget control row.
  Card body click → opens that widget's own popup (same popup the bar trigger
  uses). Two per-widget toggle buttons follow the house rounded-rect idiom
  (radius 5, `cPanel` face, 1px border, `cHover` on hover): **Pin** (glyph
  `\U000F0403` nf-md-pin) goes **accent border + accent label + a 2px accent
  underline** when active — reusing the accent-underline "active" cue rather
  than a fill flood — and **Silence** (bell `\U000F009A` / bell-off
  `\U000F09A6`) goes **`cWarning` border + label** when active. Silence is only
  rendered while the widget is NOT pinned (pinning already forces bar-visibility
  so silence is meaningless there). A gallery card whose (silenced) widget is
  quietly asking for attention gets an **accent border + accent glyph** — a
  muted-alarm hint; the bar's gallery icon likewise goes accent (not just on
  hover) when any hushed widget wants attention, and carries a small dim count
  badge of parked widgets. The popup is titleless — the header
  "`\U000F0570` gallery" (accent, bold) doubles as the title, per the house
  rule — with a right-aligned dim "N widgets" count and a centred dim empty
  state. Card membership is **frozen while open** (snapshot on open, greyed +
  held-in-place if a widget leaves the gallery mid-view, reconciled on reopen)
  per the live-lists convention. The whole thing routes every participating
  widget's BAR visibility through ONE rule in `GalleryState`:
  `barVisible = pinned || (wantsAttention && !silenced)`; pin + silence persist
  to `$XDG_STATE_HOME/qs-gallery/state.json` (atomic write, read once on
  startup — event-driven, no polling). Wired widgets + their attention
  conditions: disk (≥90% usage), bluetooth (a device connected), recording
  (recording/sharing/recent-clip active), monitors (layout changed in the last
  minute), system (a resource in its red/critical band).
- **2026-08-29** (night, later) — **App icons are data, not code**: the icon
  rules live in `hm-modules/quickshell/app-icons.json` (ordered, first match
  wins; class/classStart/classHas/titleStart/titleHas fields, `"or": true`
  for cross-field OR). Add or change an icon THERE — AppIcons.qml is just the
  matcher. Overwatch got the salt shaker 󱄎 (nf-md-shaker).
- **2026-08-29** (night) — Graph app chips are never icon-only: every chip is
  **icon + text label** — the MPRIS track title when unambiguously
  attributable, the app name otherwise (supersedes the earlier "name lives in
  the tooltip" rule below; an icon-only soundboard chip read as a bare glyph).
  Direct (unbalanced) apps render in their own right-hand **"direct to
  output" group beside the chain**, bottom-aligned with the user-volume-mix
  stage (the level their audio actually rejoins), not in the top well.
  Straight graph connectors are Rectangles, never Canvases (a full-size
  Canvas that misses its first paint composites uninitialized-texture
  garbage on GPU).
- **2026-08-29** (evening) — Batch of signal-flow / icon decisions:
  **Unknown-app icon is the question circle ""** (AppIcons.unknownGlyph —
  compare against the property, never a literal), replacing the old generic
  window box 󰖯; chips/rows with no specific icon still LEAD with this icon +
  the name (never text-only). **Clipper gets 󰿎** (title-based match, before
  the generic quickshell entry). **Info page is transparent**: the graph
  modal's cover rect is `transparent` and the covered body items fade to
  `opacity: 0` while stackOpen (layout intact, nothing ghosts); the info view
  also **auto-closes when the popup closes** — it's a peek, not a mode. The
  **anti-crackle buf controls moved INSIDE the graph's device card** (bar-style
  mini slider + "+X ms" + AUTO switch on the default sink's card); the
  top-of-panel buf row is gone and the xrun guard is ON by default
  (disable-flag semantics). The input tree lists **always-on system taps** as
  a separate dim group ("always-on system taps" caption, per-tap glyph +
  accurate label like "voice assistant (wake)"), fed by `audio-mic-users
--all`'s `sys` rows — real listeners stay red; system taps never count as
  mic-in-use.
- **2026-08-29** (later still) — Signal-flow graph app chips: **icon instead of
  the app-name text** when AppIcons knows the app specifically (generic-window
  fallback "󰖯" = keep the text; the name always lives in the tooltip). An MPRIS
  track title joins the icon ONLY when unambiguously attributable — the app has
  exactly one stream in the graph and exactly one matching player (or exactly
  one that's Playing); anything else stays icon-only, never guess (one Firefox
  can own many sink-inputs behind a single player). The "user volume mix" node
  carries a **mini bar chart** of the post-leveller per-app trims: one narrow
  bar (accent-dark fill, cInset track, faint 100% midline tick — house 0–200%
  range) per slot, the app's glyph beneath, per-bar hover tooltip with name +
  %. The autogain node is labelled "per app" to reflect the one-slot-per-app
  daemon reality.
- **2026-08-29** (later) — Hover popups (shared bar `Popup.qml` + notification
  center) got a **forgiving hover-close model**, four cooperating pieces:
  1. an invisible ~64px (uiScale-scaled) **distance-graded buffer** on the
     sides and bottom (never the top — it would sit over the bar and steal
     its input). The pointer's Chebyshev distance from the popup decides:
     within ~22px (`graceDistance`) an overshoot keeps the popup open for a
     2s grace; further away it's a deliberate move and hard-closes instantly
     (no grace, no debounce — position events are real, unlike hover blips).
     Margins are compensated so the visible popup doesn't move; the buffer
     clamps at screen edges;
  2. a **height high-water mark** while open: the window and hover footprint
     only ever grow mid-session, so live content shrinking under the cursor
     (device unplugged, notification dismissed) can't yank the hover away.
     The vacated area gets the 2s grace, not an indefinite hold — the
     invariant is that **only the visible popup, a pin, or a kbd summon may
     hold a popup open indefinitely**; every invisible region (vacated
     footprint, buffer) is time-bounded so nothing the user can't see can
     leave a menu stuck open;
  3. a 120ms **leave debounce** and 150ms **close debounce**: hover/open
     blips from surface resizes or bar reflows under a moving cursor cause
     zero churn (this was the "spam open/close while moving the mouse" flap);
  4. **anti-stuck resets**: the mark clears when a close survives the
     debounce, and all hover state force-clears when the window actually
     hides, so a stale footprint or missed leave event can never hold a
     popup open.

  Addendum (same day): **popup exclusivity** — the `PopupBus` singleton
  tracks `current`, the most recently opened popup. Every popup computes
  `evicted` (someone else owns the slot, and this popup is neither pinned
  nor keyboard-summoned) and force-hides via `effOpen = open && !evicted` —
  necessary because raw `open` stays asserted for a while through the
  triggers' 300ms hover-off latches, which merely clearing hover holds
  can't defeat (that first attempt let popups stack). Each popup binds
  `pinnedOpen` to its trigger's pin state (hubs use `anyPinned`; network
  includes its exit-node sub-popup); pinned/kbd popups are exempt and an
  evicted-but-still-asserted popup reappears when the owner closes.

  Structural rule learned the hard way: the hover handlers live on the
  content's **ancestor chain** (buffer item ⊃ bg ⊃ controls) — an Item with
  a HoverHandler overlapping the content as a sibling breaks hover delivery
  to the MouseAreas under it (all tooltips vanish, hover-sized controls
  stick) and reads the content's own hover as "left", closing the popup
  mid-use. Invisible zones (buffer, vacated footprint) are derived from the
  ancestor handler's pointer position + geometry, never from extra items.

- **2026-08-29** (later) — Signal-flow graph chips show the **full name, never
  truncated**: one line while it fits, word-wrapped (centred) beyond a width
  cap (`maxW`, 260 × graph scale), with the chip growing in height to fit.
  No ellipsis pre-trimming for graph chips — the hover tooltip is a bonus,
  not the only place the full name lives.
- **2026-08-29** — Signal-flow modal reworked as a **tab-scoped vertical tree**
  (supersedes the 2026-08-28 two-graph layout): the Output tab shows only the
  output path, Input only the mic path, flowing top→bottom. It replaces the
  panel BODY only — tab strip, volume row and buf row stay visible, and the
  volume-row info button doubles as the close toggle (accent fill while open;
  no in-modal title/close header). Every processing stage is its own centred
  node in signal order — output: app chips (slot + measured dB + trim on the
  chip) → autogain → limiter → per-app trim → MIX duplicator (only while on)
  → sink chips, with unbalanced apps bypassing down a dim right-hand lane;
  input: mic chips → blend (only while on) → rnnoise chain (split→rnnoise→mix
  sublabel) → rnnoise_source → listener chips (recording red). NO balance-blue
  here: the active/levelled path uses the accent, neutral nodes are borderless
  cards (blue #00b4d8 stays an alternative accent for the mixer arcs only).
  State pills (BALANCE/XRUN GUARD/MIX) are gone — state lives in-position
  (nodes appear where they act; usb-buffer + xrun-guard as an
  "anti-crackle buffer +X ms · auto" caption under the sinks). Chip rows
  centre via `x: (width - childrenRect.width)/2` on the Flow — fan-in
  canvases must add the flow's x to chip positions. Implementation note: the
  modal is a 1px-high Column child whose cover rect escapes upward
  (`y: bodyTop - stackModal.y`) — do NOT reparent at runtime and do NOT use
  height 0 (both silently stop the subtree painting; see memory
  quickshell-rendering-gotchas).
- **2026-08-29** (later) — Signal-flow graph **house-styled + componentised**:
  both trees are built from shared `StackChip` / `StackNode` / `StackConn`
  inline components — borderless `cPanel` cards, radius 4, white titles, dim
  subs, NO borders and NO accent floods; accent is reserved for KEY info only
  (a balanced chip's measured-gain sub line; recording red on listener chips).
  All edges are dim white. Node text is title + dim subtitle ("autogain" /
  "target -18 LUFS"), never one long line; the trim stage is titled
  "user volume mix". Fan-in canvases OVERLAY their chip Flow (wrapper Item)
  so wrapped rows connect from each chip's true bottom edge — never draw
  fans from a connector strip below the Flow (multi-row chips break). Graph
  tooltips are a single shared quickshell `PopupWindow` anchored to the
  hovered item (panel publishes {item, text, below}); in-scene ButtonTips
  clip at the popup edge and need z hacks — don't use them inside the graph.
  The graph renders at `gScale`/`gFont` (hover scale even in the kbd popup,
  0.75 factor) and lives in a click-drag Flickable that pans when it
  overflows the body.
- **2026-08-28** — **Input (per-mic) balancing removed**: the balance-scales
  toggle now shows on the output tab only, and the mic tiles' blue balance
  mini-speedometer is gone (the input side of the daemon was reverted — it
  fought the user's mic levels). The mic chain is back to plain rnnoise
  (no hpf / AGC / autogain), and the live stack view reflects that.
- **2026-08-28** — Audio panel **live signal-flow modal**: a dim solid info
  glyph () in the panel's bottom-right corner (accent when open, ButtonTip
  "Audio stack (live)") opens a modal that TAKES OVER the whole popup (opaque
  `cBg` surface, popup corner radius, header "signal flow" + `cPanel` close-X
  button). Flow graphs are chip nodes (`cPanel` fill, `cBorder` 1px, radius 5)
  joined by Canvas bezier edges: output = app chips → blue-bordered balance-slot
  chips (`#00b4d8`, live measured dB) → lime-bordered default-sink node;
  input = mic chips → (blend) → rnnoise chip (lime when ON) → lime-bordered
  rnnoise_source node ("N listening" in recording red). Unbalanced edges dim
  white. Footer = AUTO-button-style state chips (BALANCE / XRUN GUARD / MIX).
  Modal reparents onto the popup bg rect to escape the content Column.
- **2026-08-28** — Audio panel **blue balance arcs auto-range**: the inner blue
  gain arc maps into the daemon's auto-calibrated observed min..max dB range
  (min 3 dB span) instead of a fixed 0–200 % scale, with a small **white notch
  on the blue arc marking unity (0 dB)** when the range straddles it. Blue
  `#00b4d8` (from `svPalette`) stays the balance-gain colour.
- **2026-08-26** — **Audio panel → icon gallery** (`hm-modules/quickshell/audio.nix`,
  in progress). Device list became a gallery of **cards**: one transport
  icon-button per device (BT / mesh / local / cast) above a whole-word-trimmed
  name (no mid-word `…`; full name on a top-layer hover tooltip). Same physical
  device on multiple transports merges into one card (normalized-name grouping).
  Cards **fill the width, centre, and expand when few** (so filtered results show
  full titles). Active transport = the house **accent underline**. Search matches
  hidden transport aliases (`bt`/`bluetooth`, `local`, `network`/`tailnet`/`lan`/
  `mesh`). Popup widened to 440px. New **Gauges** convention (above) for the
  volume section. Tooltips use a single top-layer overlay so cards never cover
  them.
- **2026-08-26** — **Live/polled lists must be frozen while open** (new
  Conventions rule, above). Codified after repeated audio-device-list churn:
  atomic persistent store keyed by stable id, absence-as-state (not deletion),
  zero movement until reopen, in-place grey+strikethrough for gone devices,
  instant open from cache, off-model optimistic selection. Also: **networked /
  tailnet-mesh device glyph is now `󰑩` (U+F0469)**, superseding U+F1087.
- **2026-08-25** — **Device-kind glyphs** in the audio panel input/output device
  rows (`hm-modules/quickshell/audio.nix`): each audio source/sink is prefixed by
  a glyph naming _how_ it connects — Bluetooth = `󰂰` (nf-md-bluetooth), **Cast /
  Chromecast = md-cast `U+F0118`** (the SAME glyph the screen-cast bar widget
  uses, `castscreen.nix`; NB U+F0116 renders as a double-`$` in ComicShannsMono —
  use F0118), superseding the old shared monitor glyph `󰍹` for the "Via
  Chromecast" group (header + rows), and **networked / tailnet-mesh device = `󱂇`
  (U+F1087)** for tailnet-audio donated devices (name
  prefixed `tailnet-`). Local hardware keeps the tab default (mic/speaker). Rule:
  a device's transport is legible at a glance from its glyph; the cast symbol is
  reserved for actual Cast targets, the LAN glyph for mesh devices.

- **2026-08-25** — First **mobile web** UI (phone-mic page, `hm-modules/phone-mic/index.html`),
  served to the phone's browser rather than rendered in quickshell — but still
  carries the house palette so it reads as ours: `--bg #192227`, `--panel
#263238`, `--inset #11181c`, `--border #3a4a52`, lime `--accent #9dff00` /
  `--accent-dark #8db946`, `--critical #bf616a`, `ComicShannsMono Nerd Font`
  with a `ui-monospace` fallback (the phone won't have the font installed). One
  big round push-to-talk toggle (180px circle, `cPanel` face + 1px `cBorder`,
  goes accent border + accent label while live), a sunken level meter (accent-dark
  fill in an `cInset`/`cBorder` track, same slider-track look) driven by RMS from
  the AudioWorklet, and a dim status line that turns `cCritical` on error. No
  nerd-font glyphs (can't rely on them on iOS), text labels instead.

- **2026-08-24** — First in-popup sign-in form (Proton Drive, disks popup —
  module since removed in favour of the syncthing mesh, but the patterns
  stand, incl. the Popup `wantKeyboardOnDemand` focus mode it introduced): a
  `cWarning` glyph+title row, a
  dim wrapping explanation paragraph, `cInset` input fields (radius 5, 1px
  `cBorder`, dim placeholder — same as the clipper's inline fields), then a
  button row: primary action = `cPanel` face with an **accent border + accent
  label** (no fill change), secondary (external link, label suffixed `󰏌`)
  = plain `cPanel`/`cBorder`. Failure feedback is a small `cCritical` line
  under the buttons, busy state swaps the button label ("signing in…") and
  disables at `opacity 0.4`. A widget needing attention marks its **bar
  trigger** with a small `cWarning` ``beside its icon. Cloud-storage rows
reuse the partition-row look (name +`used / total NN%` + thin bar).

- **2026-08-24** — Per-player **cast/route button** in the Now-Playing popup
  (`hm-modules/quickshell/audio.nix`, `MediaPlayer.qml`). Each MPRIS row's
  transport strip gains a 4th icon-only button after prev/play-pause/next: the
  solid TV/display glyph `󰍹` (the same device glyph the audio-panel "Via
  Chromecast" group + cast-screen widget use), dim `rgba(1,1,1,0.55)` like the
  transport glyphs, brightening to `cText` when its menu is open and turning
  **accent** when that player is the active per-stream cast. Clicking it toggles
  an inline route dropdown _below_ that player's controls (not a floating menu):
  a dim `cHighlightDark` "Local outputs" sub-header + one tappable row per local
  sink (speaker glyph ``), then a "Cast to" sub-header + one row per
  discoverable Chromecast (`󰍹`); the active cast row gets the accent glyph +
  label + `󰄴` check and a faint `#22ffffff` fill. Rows are `radius 5`,
  `#33ffffff` on hover, 80ms `ColorAnimation`. Picking any row runs
  `castPlayer route "<player>" "<target>"`; routing to a local sink implicitly
  stops the cast. Cast state polled from `castAudio status` every 3s while the
  popup is open. "Which player is casting" is inferred as the single _Playing_
  player while a per-stream cast is live (backend can't attribute the stream to
  a specific MPRIS player).

- **2026-08-23** — Soundboard glyph is the music note `󰎈`. New pickers are
  **quickshell overlays, not tofi**: the soundboard picker
  (`hm-modules/quickshell/soundboard.nix`) reuses the launcher's summon
  pattern — always-mapped PanelWindow, empty input mask while closed,
  exclusive keyboard focus while open, dimmer, the launcher's pill search bar
  — with a vertical result list of `cPanel`-style rows (accent-border +
  accent-tint selection, like the launcher grid cells); directory prefixes of
  list entries render `cTextDim` with the entry name in `cText`. Inline text
  entry inside an existing window (clipper's save-to-soundboard name field)
  is a `cInset` box with 1px `cBorder`, radius 5, dim placeholder — same look
  as the clipper's size box — revealed on demand rather than a separate
  prompt window.
- **2026-08-24** — Audio picker "**Via Chromecast**" group (`hm-modules/quickshell/audio.nix`,
  sink tab only) mirrors the "Via Bluetooth" group: a divider + dim
  `cHighlightDark` section header (`\U000f0379` md-monitor glyph — same device
  glyph the cast-SCREEN widget uses, `castscreen.nix`), then one tappable row per
  network-discoverable Cast device. Clicking a device auto-connects + routes
  (exactly like tapping a BT device); the active cast row gets the accent glyph +
  label + `󰄴` check and a faint `#22ffffff` fill; clicking it again stops. A
  refresh glyph (`\U000f0450`, the shared spinner glyph) sits at the right of the
  header and **spins while an mDNS scan is in flight** — same rotating-refresh
  idiom as BT connect / wifi scan. In-flight start/stop shows that same spinner on
  the row. While a Cast output is active the main volume slider + mute icon drive
  the DEVICE (0–100 range, slider ratio `/100`) instead of the pinned local sink.

- **2026-08-20** — Assistant popup status line shows the **live pipeline
  phase**, uppercase, from the state stream (never the static config
  snapshot): WAITING (for wake word) / LISTENING (recording a command) /
  WRITING (claude turn) / PRONOUNCING (TTS synthesis) / SPEAKING (audio
  actually playing) / MUTED. Status text must reflect what the system is
  doing _right now_ — a label that says LISTENING while idle erodes trust
  in the whole widget.

- **2026-08-20** — Panel headers (assistant popup): secondary detail (e.g. the
  wake words) goes on a **dim subtitle line** (`cTextDim`, ~4px smaller,
  wrapping) under the title, never inline in the title text. Don't offer two
  controls for overlapping state: one user-facing control, with any
  non-user-controlled contributing state shown as a dim note (assistant mute
  = one mic icon + "also muted from the game side" line). Mute controls are
  the **audio-panel mic icon button** (``/``, `cWarning` when
  muted else accent, click toggles) — not a labelled checkbox; icons over
  labels generally. Bar trigger clicks always **pin the popup open/closed**;
  actions live inside the panel, never on the trigger click itself.
  Busy-state animation on bar triggers (assistant): while a request is in
  flight the icon becomes the **rotating refresh-glyph spinner** (`󰑐`,
  ~800ms/rev — the SAME spinner as bluetooth connect / wifi scan; the
  braille spinner stays a text-context/progress thing, not a bar-icon one).
  While producing output (speaking) the icon **breathes** via IconGauge's
  brightness pulse (glow 1.0↔0.72) — never glyph-cycling. Animations bind
  `running:` to their state so an idle trigger costs nothing.

- **2026-08-16** — Multi-mic bar triggers: with AUTO-switch on, **every
  candidate mic stays visible** (priority order) even while idle, and the
  active styling (filter badge, recording red + meter when live) flicks
  between them as the daemon switches — a live view of who's hot. With
  AUTO off: while recording the bar shows one widget per mic actually
  being captured (the feed mic, or every present mic in MIX); idle it
  stays the single default-source trigger as before. Virtual sources
  (rnnoise, combined_mics, delay wrappers, snd_aloop) never appear. All
  triggers open the same audio popup (input tab); labels use the
  bar-style short name. Same hover/pin/underline behaviour as every bar
  trigger.

- **2026-08-10** — Single-choice chip list (guest-audio sink picker in the
  controller-assign popup): full-width `cPanel` chips (radius 5, 1px `cBorder`,
  `cHover` on hover), the selected chip marked by an **accent border + accent
  icon** (no fill change); the whole list dims to `opacity 0.4` while an
  assignment is in flight. The "none" choice ("Follow host default") is a
  first-class chip at the top, not a separate control.

- **2026-08-10** — First drag-and-drop UI (controller-assign popup): two
  side-by-side `cInset` drop columns with 1px `cBorder`, highlighted with the
  accent border + faint fill while a drag hovers; draggable chips are
  `cPanel`-faced rounded rects (radius 5) that get `cHover` fill + accent
  border while dragged, open/closed-hand cursors, and snap back on release.
  Empty columns show a dim "drop here" hint.

- **2026-08-29** — Audio panel toolbar gained a **`SYNC`** toggle beside `MIX`,
  reusing the exact MIX-button idiom (accent `cHighlight` fill when on, `#1a1a1a`
  label on accent, faint `#22ffffff`/`#33ffffff` idle/hover, 120ms colour
  `Behavior`, `ButtonTip`), just wider (40 vs 34) for the 4-letter label.
  Precedent: a **conditional** toolbar toggle — it only renders when its context
  applies (a Chromecast is a member of the live output MIX), rather than showing
  disabled. Match this for other context-only controls.
- **2026-08-07** — General rule: **solid Font Awesome icons** are preferred
  over outline Material Design glyphs (recorded in the Font section).
- **2026-08-07** — Screenshare bar indicator uses the **solid** Font Awesome
  desktop glyph `` (matching the pre-rework ScreenCastIndicator), not the
  outline nerd-monitor `󰍹`. The bar's monitor-layout miniature draws each
  monitor on a **stand** (thin centred neck + wider 1px base, always white —
  the accent stays on the screen) with every base ending on a shared floor
  line just below the lowest monitor — higher-mounted monitors get longer
  necks, and the monitors+stands group is centred in the icon.
- **2026-08-06** — Tabs are now **full-width** (equal `Layout.fillWidth` share),
  superseding the earlier content-sized/left-aligned guidance; they also double
  as the title of titleless popups.
- **2026-08-06** — Icon-only action buttons (save/copy/close = floppy/two-page/X),
  no keyboard-hint text; responsive control bars (hide non-essential readouts +
  shrinkable sliders so buttons stay on-screen when small).
- **2026-08-06** — Initial guide. Codified the accent-underline tab style
  (retrofitted the clipper's clip/audio tabs, which had used a bordered pill, to
  match the audio panel / workspace widget). Recorded the palette, button,
  slider, progress-bar and spinner patterns.
