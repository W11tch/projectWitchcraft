# Project Configuration Snapshot
**Generated on:** 08/09/2025 13:00:02
---

## Tags and Layers
### Tags
- `Untagged`
- `Respawn`
- `Finish`
- `EditorOnly`
- `MainCamera`
- `Player`
- `GameController`

### Layers
- **Layer 0:** `Default`
- **Layer 1:** `TransparentFX`
- **Layer 2:** `Ignore Raycast`
- **Layer 3:** `Water`
- **Layer 4:** `UI`
- **Layer 5:** `BuildingBlocks`
- **Layer 6:** `Ground`
- **Layer 7:** `Preview`
- **Layer 8:** `Player`

---

## Physics Settings
- **Gravity:** `(0.00, -9.81, 0.00)`
- **Default Max Depenetration Velocity:** `10`
- **Sleep Threshold:** `0,005`

### Collision Matrix
| Layer | Collides With |
|---|---|
| `Default` | `Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`, `BuildingBlocks`, `Ground`, `Player` |
| `TransparentFX` | `Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`, `BuildingBlocks`, `Ground`, `Player` |
| `Ignore Raycast` | `Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`, `BuildingBlocks`, `Ground`, `Player` |
| `Water` | `Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`, `BuildingBlocks`, `Ground`, `Player` |
| `UI` | `Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`, `BuildingBlocks`, `Ground`, `Player` |
| `BuildingBlocks` | `Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`, `BuildingBlocks`, `Ground`, `Player` |
| `Ground` | `Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`, `BuildingBlocks`, `Ground`, `Player` |
| `Preview` |  |
| `Player` | `Default`, `TransparentFX`, `Ignore Raycast`, `Water`, `UI`, `BuildingBlocks`, `Ground`, `Player` |

---

## Input System Configuration
### Global Settings
- **Update Mode:** `ProcessEventsInDynamicUpdate`

### Found Input Actions
#### InputSystem_Actions (`Assets/InputSystem_Actions.inputactions`)
- **Action Map:** `Player`
  - **Action:** `Move` (`Value` with `Vector2`)
    - **Binding:** `<Gamepad>/leftStick`
    - **Binding:** `Dpad`
    - **Binding:** `<Keyboard>/w`
    - **Binding:** `<Keyboard>/upArrow`
    - **Binding:** `<Keyboard>/s`
    - **Binding:** `<Keyboard>/downArrow`
    - **Binding:** `<Keyboard>/a`
    - **Binding:** `<Keyboard>/leftArrow`
    - **Binding:** `<Keyboard>/d`
    - **Binding:** `<Keyboard>/rightArrow`
    - **Binding:** `<XRController>/{Primary2DAxis}`
    - **Binding:** `<Joystick>/stick`
  - **Action:** `SelectHotbarSlot` (`Button` with ``)
    - **Binding:** `<Keyboard>/1`
    - **Binding:** `<Keyboard>/2`
    - **Binding:** `<Keyboard>/3`
    - **Binding:** `<Keyboard>/4`
    - **Binding:** `<Keyboard>/5`
    - **Binding:** `<Keyboard>/6`
    - **Binding:** `<Keyboard>/7`
    - **Binding:** `<Keyboard>/8`
    - **Binding:** `<Keyboard>/9`
    - **Binding:** `<Keyboard>/0`
  - **Action:** `Destroy` (`Button` with ``)
    - **Binding:** `<Keyboard>/f`
    - **Binding:** `<Gamepad>/leftShoulder`
  - **Action:** `Cancel` (`Button` with ``)
    - **Binding:** `<Keyboard>/{Cancel}`
    - **Binding:** `<Gamepad>/buttonEast`
  - **Action:** `Rotate` (`Button` with ``)
    - **Binding:** `<Keyboard>/r`
    - **Binding:** `<Gamepad>/rightShoulder`
  - **Action:** `Place` (`Button` with ``)
    - **Binding:** `<Mouse>/leftButton`
    - **Binding:** `<Gamepad>/buttonSouth`
  - **Action:** `Attack` (`Button` with ``)
    - **Binding:** `<Gamepad>/buttonWest`
    - **Binding:** `<Mouse>/leftButton`
    - **Binding:** `<Touchscreen>/primaryTouch/tap`
    - **Binding:** `<Joystick>/trigger`
    - **Binding:** `<XRController>/{PrimaryAction}`
    - **Binding:** `<Keyboard>/enter`
  - **Action:** `Interact` (`Button` with ``)
    - **Binding:** `<Keyboard>/e`
    - **Binding:** `<Gamepad>/buttonNorth`
  - **Action:** `Previous` (`Button` with ``)
    - **Binding:** `<Keyboard>/1`
    - **Binding:** `<Gamepad>/dpad/left`
  - **Action:** `Next` (`Button` with ``)
    - **Binding:** `<Keyboard>/2`
    - **Binding:** `<Gamepad>/dpad/right`
  - **Action:** `Look` (`Value` with `Vector2`)
    - **Binding:** `<Gamepad>/rightStick`
    - **Binding:** `<Pointer>/delta`
    - **Binding:** `<Joystick>/{Hatswitch}`
  - **Action:** `Pause` (`Button` with ``)
    - **Binding:** ``
    - **Binding:** `<Keyboard>/escape`
  - **Action:** `ToggleInventory` (`Button` with ``)
    - **Binding:** ``
    - **Binding:** `<Keyboard>/tab`
- **Action Map:** `UI`
  - **Action:** `Navigate` (`PassThrough` with `Vector2`)
    - **Binding:** `2DVector`
    - **Binding:** `<Gamepad>/leftStick/up`
    - **Binding:** `<Gamepad>/rightStick/up`
    - **Binding:** `<Gamepad>/leftStick/down`
    - **Binding:** `<Gamepad>/rightStick/down`
    - **Binding:** `<Gamepad>/leftStick/left`
    - **Binding:** `<Gamepad>/rightStick/left`
    - **Binding:** `<Gamepad>/leftStick/right`
    - **Binding:** `<Gamepad>/rightStick/right`
    - **Binding:** `<Gamepad>/dpad`
    - **Binding:** `2DVector`
    - **Binding:** `<Joystick>/stick/up`
    - **Binding:** `<Joystick>/stick/down`
    - **Binding:** `<Joystick>/stick/left`
    - **Binding:** `<Joystick>/stick/right`
    - **Binding:** `2DVector`
    - **Binding:** `<Keyboard>/w`
    - **Binding:** `<Keyboard>/upArrow`
    - **Binding:** `<Keyboard>/s`
    - **Binding:** `<Keyboard>/downArrow`
    - **Binding:** `<Keyboard>/a`
    - **Binding:** `<Keyboard>/leftArrow`
    - **Binding:** `<Keyboard>/d`
    - **Binding:** `<Keyboard>/rightArrow`
  - **Action:** `Submit` (`Button` with `Button`)
    - **Binding:** `*/{Submit}`
  - **Action:** `Cancel` (`Button` with `Button`)
    - **Binding:** `*/{Cancel}`
  - **Action:** `Point` (`PassThrough` with `Vector2`)
    - **Binding:** `<Mouse>/position`
    - **Binding:** `<Pen>/position`
    - **Binding:** `<Touchscreen>/touch*/position`
  - **Action:** `Click` (`PassThrough` with `Button`)
    - **Binding:** `<Mouse>/leftButton`
    - **Binding:** `<Pen>/tip`
    - **Binding:** `<Touchscreen>/touch*/press`
    - **Binding:** `<XRController>/trigger`
  - **Action:** `RightClick` (`PassThrough` with `Button`)
    - **Binding:** `<Mouse>/rightButton`
  - **Action:** `MiddleClick` (`PassThrough` with `Button`)
    - **Binding:** `<Mouse>/middleButton`
  - **Action:** `ScrollWheel` (`PassThrough` with `Vector2`)
    - **Binding:** `<Mouse>/scroll`
  - **Action:** `TrackedDevicePosition` (`PassThrough` with `Vector3`)
    - **Binding:** `<XRController>/devicePosition`
  - **Action:** `TrackedDeviceOrientation` (`PassThrough` with `Quaternion`)
    - **Binding:** `<XRController>/deviceRotation`
#### DefaultInputActions (`Packages/com.unity.inputsystem/InputSystem/Plugins/PlayerInput/DefaultInputActions.inputactions`)
- **Action Map:** `Player`
  - **Action:** `Move` (`Value` with `Vector2`)
    - **Binding:** `<Gamepad>/leftStick`
    - **Binding:** `Dpad`
    - **Binding:** `<Keyboard>/w`
    - **Binding:** `<Keyboard>/upArrow`
    - **Binding:** `<Keyboard>/s`
    - **Binding:** `<Keyboard>/downArrow`
    - **Binding:** `<Keyboard>/a`
    - **Binding:** `<Keyboard>/leftArrow`
    - **Binding:** `<Keyboard>/d`
    - **Binding:** `<Keyboard>/rightArrow`
    - **Binding:** `<XRController>/{Primary2DAxis}`
    - **Binding:** `<Joystick>/stick`
  - **Action:** `Look` (`Value` with `Vector2`)
    - **Binding:** `<Gamepad>/rightStick`
    - **Binding:** `<Pointer>/delta`
    - **Binding:** `<Joystick>/{Hatswitch}`
  - **Action:** `Fire` (`Button` with `Button`)
    - **Binding:** `<Gamepad>/rightTrigger`
    - **Binding:** `<Mouse>/leftButton`
    - **Binding:** `<Touchscreen>/primaryTouch/tap`
    - **Binding:** `<Joystick>/trigger`
    - **Binding:** `<XRController>/{PrimaryAction}`
- **Action Map:** `UI`
  - **Action:** `Navigate` (`PassThrough` with `Vector2`)
    - **Binding:** `2DVector`
    - **Binding:** `<Gamepad>/leftStick/up`
    - **Binding:** `<Gamepad>/rightStick/up`
    - **Binding:** `<Gamepad>/leftStick/down`
    - **Binding:** `<Gamepad>/rightStick/down`
    - **Binding:** `<Gamepad>/leftStick/left`
    - **Binding:** `<Gamepad>/rightStick/left`
    - **Binding:** `<Gamepad>/leftStick/right`
    - **Binding:** `<Gamepad>/rightStick/right`
    - **Binding:** `<Gamepad>/dpad`
    - **Binding:** `2DVector`
    - **Binding:** `<Joystick>/stick/up`
    - **Binding:** `<Joystick>/stick/down`
    - **Binding:** `<Joystick>/stick/left`
    - **Binding:** `<Joystick>/stick/right`
    - **Binding:** `2DVector`
    - **Binding:** `<Keyboard>/w`
    - **Binding:** `<Keyboard>/upArrow`
    - **Binding:** `<Keyboard>/s`
    - **Binding:** `<Keyboard>/downArrow`
    - **Binding:** `<Keyboard>/a`
    - **Binding:** `<Keyboard>/leftArrow`
    - **Binding:** `<Keyboard>/d`
    - **Binding:** `<Keyboard>/rightArrow`
  - **Action:** `Submit` (`Button` with `Button`)
    - **Binding:** `*/{Submit}`
  - **Action:** `Cancel` (`Button` with `Button`)
    - **Binding:** `*/{Cancel}`
  - **Action:** `Point` (`PassThrough` with `Vector2`)
    - **Binding:** `<Mouse>/position`
    - **Binding:** `<Pen>/position`
    - **Binding:** `<Touchscreen>/touch*/position`
  - **Action:** `Click` (`PassThrough` with `Button`)
    - **Binding:** `<Mouse>/leftButton`
    - **Binding:** `<Pen>/tip`
    - **Binding:** `<Touchscreen>/touch*/press`
    - **Binding:** `<XRController>/trigger`
  - **Action:** `ScrollWheel` (`PassThrough` with `Vector2`)
    - **Binding:** `<Mouse>/scroll`
  - **Action:** `MiddleClick` (`PassThrough` with `Button`)
    - **Binding:** `<Mouse>/middleButton`
  - **Action:** `RightClick` (`PassThrough` with `Button`)
    - **Binding:** `<Mouse>/rightButton`
  - **Action:** `TrackedDevicePosition` (`PassThrough` with `Vector3`)
    - **Binding:** `<XRController>/devicePosition`
  - **Action:** `TrackedDeviceOrientation` (`PassThrough` with `Quaternion`)
    - **Binding:** `<XRController>/deviceRotation`
