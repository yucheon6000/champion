# JSON-Based Game Creation System

## Overview

This system interprets user instructions to generate 2D game scenes in JSON format with modular components. The game engine uses this JSON to instantiate entities, manage game logic, and interpret behaviors.

## JSON Top-Level Structure

```json
{
  "entities": [ ... ],  // Game objects placed in the scene
  "presets": { ... },   // Reusable entity templates
  "controllers": [ ... ], // Input handling definitions
  "camera": { ... }     // Camera settings
}
```

## Entities

Each entity represents an object in the game world:

```json
{
  "name": "Player",      // Descriptive label (recommended)
  "position": [0, 0],    // Required 2D coordinates [x, y]
  "rotation": 0,         // Optional angle in degrees
  "tags": ["Player"],    // Optional labels for identification
  "color": "#00BFFF",    // Optional hex color
  "components": [ ... ]  // Required array of behavior components
}
```

Alternative: Directly spawn a preset by reference:

```json
{
  "presetId": "zombie",
  "position": [3, 0]
}
```

## Presets

Templates for entities that can be instantiated multiple times:

```json
"presets": {
  "bullet": {
    "name": "Player Bullet", // Required descriptive name
    "color": "#FFD700",      // Optional color
    "tags": ["Projectile"],  // Required array (can be empty)
    "components": [ ... ]    // Required components
  }
}
```

## Controllers

Input methods for player interaction:

```json
"controllers": [
  {
    "type": "Controller2D",       // For directional movement
    "description": "Movement"
  },
  {
    "type": "ControllerButton",   // For button actions
    "description": "Shoot",
    "keyCode": "Space"            // Must match Unity KeyCode names
  }
]
```

## Camera

Controls the view of the game:

```json
"camera": {
  "fov": 5.0  // Field of view (lower = zoom in, higher = zoom out)
}
```

## Component System

Components define entity behaviors. Each requires a "type" field plus specific fields.

### Jumpable

Allows entity to jump with a force.

```json
{
  "type": "Jumpable",
  "jumpForce": 10.0, // Strength of jump
  "listenTo": 1 // Index of button controller
}
```

### Gravity

Applies downward force to the entity.

```json
{
  "type": "Gravity",
  "gravityScale": 1.0 // 1.0 = normal gravity
}
```

### Stats

Stores numeric values like health, score, etc.

```json
{
  "type": "Stats",
  "values": {
    "HP": 10,
    "Score": 0,
    "Timer": 60
  }
}
```

### Movable

Enables directional movement.

```json
{
  "type": "Movable",
  "moveSpeed": 5.0, // Units per second
  "listenTo": 0 // Index of Controller2D
}
```

### Immovable

Prevents physics movement, for static objects.

```json
{
  "type": "Immovable" // No additional fields
}
```

### Shooter

Creates projectiles when activated.

```json
{
  "type": "Shooter",
  "projectilePresetId": "bullet", // Must reference valid preset
  "listenTo": 1, // Controller index
  "shootDirection": "facing", // Options: "facing" or "fixed"
  "fixedDirection": [1, 0], // Only used if shootDirection is "fixed"
  "directionSpace": "world" // Options: "world" or "local"
}
```

### Projectile

Identifies an entity as a projectile.

```json
{
  "type": "Projectile" // No additional fields
  // Must be paired with Movable
}
```

### Rotatable

Controls entity rotation and facing.

```json
{
  "type": "Rotatable",
  "faceMovement": true, // Auto-rotate to match movement direction
  "turnSpeed": 180.0 // Degrees per second (negative = instant)
}
```

### EffectOnHit

Applies effects upon collision with tagged entities.

```json
{
  "type": "EffectOnHit",
  "targetTags": ["Enemy", "Obstacle"], // List of tags this effect applies to
  "effects": [
    // Array of effect objects
    {
      "type": "stat", // Stat modification effect
      "key": "HP", // Stat name to modify
      "value": -1.0 // Change amount (negative = decrease)
    },
    {
      "type": "knockback", // Force effect
      "force": 5.0, // Strength of push
      "direction": "facing", // Options: "facing", "fixed", "fromSelf"
      "fixedDirection": [1, 0], // Only if direction is "fixed"
      "space": "world" // Options: "world" or "local". If "local", the direction vector is interpreted relative to the source's local coordinate system (i.e., source.transform.TransformDirection is used).
    }
  ]
}
```

### EffectReceiverStat

Enables entity to receive stat-changing effects.

```json
{
  "type": "EffectReceiverStat" // No additional fields
  // Must be used with Stats component
}
```

### EffectReceiverKnockback

Enables entity to receive knockback forces.

```json
{
  "type": "EffectReceiverKnockback" // No additional fields
}
```

### DestroyIf

Removes entity when a condition is met.

```json
{
  "type": "DestroyIf",
  "trigger": "onStatChanged", // Options: "onStatChanged", "onEffectGiven", "onEffectReceived"
  "stat": "HP", // Required if trigger is "onStatChanged"
  "operator": "lessThanOrEqual", // Options: "equal", "notEqual", "lessThan", "lessThanOrEqual", "greaterThan", "greaterThanOrEqual"
  "value": 0
}
```

### WinIf

Triggers a win condition when a condition is met.

```json
{
  "type": "WinIf",
  "trigger": "onStatChanged", // Options: "onStatChanged", "onEffectGiven", "onEffectReceived"
  "stat": "Score", // Required if trigger is "onStatChanged"
  "operator": "greaterThanOrEqual", // Options: "equal", "notEqual", "lessThan", "lessThanOrEqual", "greaterThan", "greaterThanOrEqual"
  "value": 100
}
```

### LoseIf

Triggers a lose condition when a condition is met.

```json
{
  "type": "LoseIf",
  "trigger": "onStatChanged", // Options: "onStatChanged", "onEffectGiven", "onEffectReceived"
  "stat": "Timer", // Required if trigger is "onStatChanged"
  "operator": "lessThanOrEqual", // Options: "equal", "notEqual", "lessThan", "lessThanOrEqual", "greaterThan", "greaterThanOrEqual"
  "value": 0
}
```

### StatOverTime

Continuously modifies a stat at a rate.

```json
{
  "type": "StatOverTime",
  "stat": "HP", // Stat name to modify
  "amountPerSecond": -0.5, // Amount to modify per second (negative = decrease)
  "interval": 1.0 // Interval time in seconds (how often the stat is modified)
}
```

### CameraTarget

Makes camera follow this entity.

```json
{
  "type": "CameraTarget" // No additional fields
  // Only one entity should use this
}
```

## Required Component Pairings

- Projectile must be used with Movable
- EffectReceiverStat must be used with Stats
- DestroyIf with onStatChanged requires the Stats component with the referenced stat
- WinIf with onStatChanged requires the Stats component with the referenced stat
- LoseIf with onStatChanged requires the Stats component with the referenced stat

## Key Rules & Constraints

- Every entity must have position and at least one component
- Every component must have a type field
- Referenced preset IDs must exist in the presets dictionary
- listenTo indices must reference valid controllers
- Only one CameraTarget should exist at a time
- When using ControllerButton, keyCode must match Unity's KeyCode names exactly
- Shooter projectilePresetId must reference a preset with a Projectile component
- DestroyIf, WinIf, and LoseIf operators must be one of: "equal", "notEqual", "lessThan", "lessThanOrEqual", "greaterThan", "greaterThanOrEqual"
- Effect types must be either "stat" or "knockback"
- Direction options are: "facing", "fixed", or "fromSelf"
- In WinIf, LoseIf, and similar components, the stat can only reference stats defined within the entity’s own Stats component. Accessing stats from other entities is strictly not allowed — absolutely, positively, no exceptions!

## Implementation Tips

- For melee attacks: Create short-lived projectiles with short move distance
- For timers: Use Stats + StatOverTime + DestroyIf
- For self-destruction: Use DestroyIf with onEffectGiven
- For enemy death: Use DestroyIf with onStatChanged for HP <= 0
- For win conditions: Use WinIf with onStatChanged (e.g., Score >= 100)
- For lose conditions: Use LoseIf with onStatChanged (e.g., Timer <= 0 or HP <= 0)
- You can add multiple LoseIf, WinIf, or DestroyIf components to apply OR conditions between them.
- Create reusable presets first, then place them as entities in the scene to avoid duplication.

## Output Format

- Generate valid, complete JSON without truncation
- Do not improvise unsupported features
- When feature is not possible, suggest valid alternatives
- Add explanatory comments after JSON if needed
- Write it in a compact JSON format where objects are kept on single lines if possible, and indentation is minimal!!!
