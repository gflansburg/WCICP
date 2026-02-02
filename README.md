# WCICP

## Overview

This application provides a **fallback ICP display** for the **WinWing/WinCtrl ICP controller**, delivering useful avionics data in **Microsoft Flight Simulator (MSFS)** and **X-Plane** when aircraft are **not officially supported** by WinWing.

Many aircraft expose rich avionics data through SimConnect, FSUIPC, or X-Plane datarefs but display **no information at all** on the WinWing/WinCtrl ICP unless explicitly supported by SimAppPro.  
This project fills that gap by presenting a **common, aircraft-agnostic ICP display** that works across *all* aircraft.

**SimAppPro is not required.**

---

## Purpose

The goal of this application is simple:

> Ensure the WinWing/WinCtrl ICP always shows meaningful information.

Rather than relying on aircraft-specific integrations, the app exposes a** Common Navigation Information (CNI)** display and related pages—an aircraft-agnostic abstraction inspired by, but not limited to, the F-16’s Communications, Navigation, and Identification concept—that every aircraft can support.

---

## Built-In Displays

The following displays are included out of the box:

- **CNI** – Common Ntrument Information (primary overview)
- **COM1** – COM1 radio status and frequencies
- **COM2** – COM2 radio status and frequencies
- **IFF** – Identification / transponder information
- **NAV1** – NAV1 radio and information
- **NAV2** – NAV2 radio and information
- **GPS** – GPS waypoint information
- **AP** – Autopilot derived data
- **SWCH** – Switch states
- **LGHT** – Light states
- **WX** – Weather (OAT, wind, pressure, QNH)
- **TIME** – Time and location
- **INFO** – System and position information
- **HROT** – Helicoptor rotor systems
- **HDIS** – Helicoptor dynamics and instruments
- **HCTL** – Helicoptor flight controls
- **BALN** – Balloon envelope, burner, vent, and thermal status
- **AIRS** – Airship gas compartment, pressure, volume, weight, and mast systems

Each display is:
- Aircraft-agnostic  
- Hardware-accurate in layout  
- Consistent in units and formatting  

---

## Custom Displays

In addition to built-in displays, the application supports **user-defined custom ICP displays**.

Users can:
- Create custom layouts
- Select from all available avionics and system fields
- Control positioning, inversion, padding, and formatting
- Build displays without modifying application code

This allows the ICP to be tailored to individual workflows or aircraft while still using the same data backend.

---

## Joystick & Button Mapping

Joystick button mapping is supported **for ICP display control only**.

Buttons may be mapped to:
- Select a specific built-in display
- Select a custom display
- Cycle forward or backward through displays

No simulator control inputs are generated.  
Button mapping exists solely to control **which ICP display is shown**.

---

## Simulator Support

- **Microsoft Flight Simulator (MSFS)**
  - SimConnect
  - FSUIPC
- **X-Plane**
  - Native datarefs

The provider-based architecture allows additional simulators to be added if desired.

---

## Units & Globalization

The application supports multiple unit systems:

- **Aviation** (ft, NM, kts, °F, inHg)
- **Metric** (m, km, km/h, °C, hPa)

Raw values are preserved internally, with formatted values derived consistently for display.  
This ensures accuracy for both gauges and human-readable output.

---

## Altitude Modeling

Altitude values are intentionally separated to avoid ambiguity:

- **MSL** – Indicated altitude based on Kohlsman setting
- **AGL** – Height above ground level
- **TALT** – True altitude derived from actual static pressure

This avoids misleading “everything matches” behavior and keeps preview data meaningful.

---

## Bridge Architecture & Reliability

The application uses a lightweight external bridge process to communicate with the WinWing ICP.

Features include:
- Automatic launch and connection
- Service-style crash recovery
- Limited restart attempts to prevent runaway failures
- Graceful fallback when the bridge is unavailable

If the bridge is down, the application continues running safely without attempting to write to the ICP.

---

## What This Application Is Not

- Not a replacement for aircraft-specific SimAppPro profiles
- Not a study-level avionics simulation
- Not tied to any specific aircraft

It is intentionally **generic, predictable, and robust**.

---

## Status

Actively developed and used.

Built to solve a practical problem:  
Flying aircraft with a dark ICP despite the simulator providing all the necessary data.
