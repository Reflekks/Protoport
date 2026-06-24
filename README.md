<div align="center">

# Protoport

**Automatic ProtonVPN port forwarding sync for qBittorrent on Windows**

![Windows](https://img.shields.io/badge/Windows-10%2F11-blue?logo=windows)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green)

</div>

---

ProtonVPN periodically rotates its forwarded port. When that happens, qBittorrent stops receiving incoming connections and your speeds tank or you lose connection entirely. Protoport watches the ProtonVPN log in the background, detects the port change the moment it happens, updates `qBittorrent.ini`, and restarts qBittorrent — all automatically, with no credentials required.

## Features

- **Instant Detection** — tails the ProtonVPN log file in real time
- **Direct ini Editing** — no Web UI, no passwords, no API keys
- **Clean Restart** — gracefully closes qBittorrent, writes the new port, then relaunches
- **Tray Notifications** — Windows balloon notification on every port change
- **Status Window** — shows current port, last change time, and a live activity log
- **Silent Background** — lives in the system tray, stays out of your way

## Requirements

- Windows 10 or 11
- [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (or build self-contained — see below)
- [ProtonVPN](https://protonvpn.com/download) desktop app with port forwarding enabled
- [qBittorrent](https://www.qbittorrent.org/)

> **No credentials needed.** Protoport reads the ProtonVPN log directly and edits `qBittorrent.ini` on disk — it never touches the qBittorrent Web UI.

## Installation

### Option A — Download the release (recommended)

1. Download `Protoport.exe` from the [Releases](../../releases) page
2. Place it anywhere you like
3. Run it — the tray icon will appear immediately

### Option B — Build from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

## Attribution

### The App Itself

- Started as my own idea as I wanted an app that didn't require qBit Web UI creds
- Claude (free version) walked me through the entire coding process
- Further development will be done within a locally-hosted LLM

### Icon

- Shield by Andi wiyanto from [Noun Project](https://thenounproject.com/browse/icons/term/shield/) (CC BY 3.0)

- Science [Atom] by Gregor Cresnar from [Noun Project](https://thenounproject.com/browse/icons/term/science/) (CC BY 3.0)

- Created in Affinity Photo
