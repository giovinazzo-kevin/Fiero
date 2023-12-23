# Scripting

Scripting in the FieroEngine is done with Ergo.
Each game system has its own DSL for writing scripts. 
For example:
  - The UI system implements a layouting DSL that makes it possible to define complex windows in Ergo.
  - ScriptEffects implement an event-driven DSL that makes it possible to define complex actor behavior in Ergo.

The reason for doing this is two-fold: on the one hand, it makes modding extremely easy. On the other hand, it allows me to optimize Ergo by thoroughly testing it in various scenarios.


