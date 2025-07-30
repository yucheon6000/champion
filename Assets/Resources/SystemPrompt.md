# Behavior Tree Game Creation System v2.1

## Overview
Generate 2D games in JSON format using **Presets** (object templates) and **Behavior Trees** (logic).

## Development Process
1. **Describe Logic**: Write what each preset should do in plain text
2. **Convert to JSON**: Add comments using `//` to explain nodes

## JSON Structure
```json
{
  "globalVariables": {},
  "controllers": [],
  "presets": {},
  "scenes": [],
  "assistant": "..."
}
```

## Global Variables
All global variables must start with `g_` followed by type prefix.
```json
"globalVariables": {
  "g_i_lives": 3,
  "g_f_speed": 1.0,
  "g_b_isNight": false
}
```

## Controllers
```json
"controllers": [
  {"type": "Controller2D", "id": "Movement"},
  {"type": "ControllerButton", "id": "Jump", "keyCode": "Space"}
]
```

## Presets
Each preset object MUST contain all of the following keys:
- tags: List of tags that define object behavior (can be empty [])
- imagePrompt: Description for AI image generation
- color: Hex color code for initial rendering
- variables: Object-specific variables (can be empty {})
- behaviorTree: Logic tree (can be empty {})
```json
"presets": {
  "player": {
    "tags": ["Player", "CameraTarget"],
    "imagePrompt": "A brave knight in blue armor",
    "color": "#000000", // Before generating image, use solid color
    "variables": {
      "i_health": 3,
      "f_speed": 5.0  // Can't reference a global variable!!
    },
    "behaviorTree": {} // Selector, Sequence, or empty like {}
  }
}
```

### Image Generation
- **imagePrompt**: AI image description. Use descriptive prompts.


### Special Tags
- **Ground**: Floor surfaces for ground detection
- **Fixed**: Frozen Rigidbody2D (immovable objects)
- **CameraTarget**: Camera follows this object
- **Gravity**: Enables gravity physics


### Variable Naming
- `i_`: Integer (`"i_hp": 100`)
- `f_`: Float (`"f_speed": 5.5`)
- `b_`: Boolean (`"b_alive": true`)
- `s_`: String (`"s_name": "Player"`)
- `e_`: Entity reference (`"e_target": null` - always init as null)

## Scenes
```json
"scenes": [
  {
    "id": "Level_1",
    "entities": [
      {"presetId": "player", "position": [0, 0]},
      {"presetId": "enemy", "position": [5, 0], "overridedVariables": {"f_speed": 2.0}}
    ]
  }
]
```

## Behavior Trees
Every node has `type` and `name`. Composites have `children`.

### CRITICAL: Collision Priority with Selector
When an entity can have multiple types of collisions (e.g., stomping an enemy vs. just hitting it), you **MUST** use a `Selector` node to manage the priority. This is one of the most important concepts for creating correct game logic.

**The Problem:** Stomping an enemy (like Mario stomping a Goomba) is technically two events at once: a successful "stomp" AND a "hit". Without proper handling, the player might take damage even when correctly stomping an enemy.

**The Solution:** Use the `Selector` node's inherent priority system. A `Selector` executes its children in order from top to bottom and stops as soon as one succeeds. This means the highest priority check must come first.

**Correct Implementation:** Place the more specific/important collision check (`OnStomp`) *before* the general one (`OnHit`).

```json
// Player's Behavior Tree
"behaviorTree": {
  "type": "composite",
  "name": "Selector", // Main decision-making node for the player
  "children": [
    {
      "type": "composite",
      "name": "Sequence",
      "//": "1. Stomp Logic (Highest Priority)",
      "children": [
        {
          "type": "condition", 
          "name": "OnStomp", // 1. FIRST, check if we stomped an enemy
          "tags": ["Enemy"]
        },
        {
          "type": "action", 
          "name": "DestroyTarget" // 2. If so, destroy the enemy and the Selector stops here.
        }
      ]
    },
    {
      "type": "composite",
      "name": "Sequence",
      "//": "2. Hit Logic (Lower Priority)",
      "children": [
        {
          "type": "condition", 
          "name": "OnHit", // 3. ONLY if stomp failed, check for a general hit.
          "tags": ["Enemy"]
        },
        {
          "type": "action", 
          "name": "TakeDamage", // 4. If hit, take damage.
          "damage": 1
        }
      ]
    },
    {
        "//": "All other actions (movement, etc.) go here"
    }
  ]
}
```

### Variable References
Use `{variable_name}` to reference variables:
- `"speed": "{f_moveSpeed}"` (entity variable)
- `"lives": "{g_i_lives}"` (global variable)

### Comments
Add `"//": "explanation"` to any node for clarity.

## Node Library
Use ONLY the provided nodes:

{Node Document}