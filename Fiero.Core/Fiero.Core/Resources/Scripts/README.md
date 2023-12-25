# Scripts

All Ergo scripts automatically import `core.ergo`, which in turn imports all other modules in the `core/` subdirectory.

Some modules provide behavior that might not be required by all scripts, usually by subscribing to specific system events.
These modules are found in the `optin/` subdirectory and must be imported explicitly.

## Directives
  - `:- subscribe/2`: routes system events to the current script.
	- Handlers have the form: `S:E(E_event { })` where `S` is the system name and `E` is the event name. The argument is a dict.
  - `:- observe/1`: routes data change events to the current script.
	- Handlers have the form: `data:X_changed(OldValue, NewValue)` where `X` is the name of the datum.

## Core

- `data`: provides access to the game's event-driven data management system through the `GameDataStore`.
  - `get/2`: retrieves the value of a game datum by name. 
  - `set/3`: compares the current value of a datum with a control value: if they are equal, the new value is set. 

## Optin

- `input`: routes input events to the current script and provides access to the input management system.
  - `key_state/2`: gets whether a certain key is `down`, `up`, `pressed` or `released`.
  - `input:keyboard_event(Key, Type, Predicate)`: an individual route. Can be asserted statically, or dynamically (via `bind/3` and `unbind/3`).
- `event`: routes custom script events to the current script and allows raising events. Not required to route regular system events.
  - `meta:script_event_raised/3`: subscribed by the event module and handled implicitly. Raised by `raise/3` in the case where a system event is not matched.
  - `subscribed/2`: asserted automatically by the `:- subscribe/2` directive.
	- used by `event`'s handler of `meta:script_event_raised/3` to check whether the current script subscribes to a custom event.
  - `raise/3`: built-in used to raise events. Can raise both system events and custom script events.
