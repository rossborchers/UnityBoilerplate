# Unity Boilerplate

__WIP__
Useful scaffolding for building games. All the things I keep on doing over and over.

Contains:

### Bootstrapper
- LevelLoadData - Scriptable object that stores stacks of scenes that can be additivley loaded by a GameManager that extends BoostrapperBase. 
- BoostrapperBase - Inherit your game manager from this to gain level stack loading.
### Managers
- Manager - A base class for all managers (Managers in this instance are pure c# scripts that are updated by the GameManager). Provides static getters for managers by type.
- StateManager - A lightweight callback based state machine that provides on entered, on update and on exited for each state.
  - Uses a builder pattern to build behaviour.



