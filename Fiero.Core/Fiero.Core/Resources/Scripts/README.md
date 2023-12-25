# Scripts

All Ergo scripts automatically import `core.ergo`, which in turn imports all other modules in the `core/` subdirectory.

Some modules provide behavior that might not be required by all scripts. 
These modules are found in the `optin/` subdirectory and must be imported explicitly.

## Core


## Optin

- `input`: routes input events to the current script.
  - `input:keyboard_event(Key, Type, Predicate)`: an individual route. Can be asserted statically, or dynamically (via `bind/3` and `unbind/3`).
- `event`: routes custom script events to the current script and allows raising events. Not required to route regular system events.
  - `subscribed/2`: asserted automatically by the `subscribe/2` directive; enables scripts to send and receive custom events by recycling the system event mechanism.
  - `raise/3`: built-in used to raise events. Can raise both system events and custom script events.
