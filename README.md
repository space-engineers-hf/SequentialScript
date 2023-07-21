# SequentialScript

Example:

```
when @previous_action1, @previous_action2
run
 Block name 1 -> Action /NoCheck /Argument1 /Argument2:MyValue /Argument3:"My value with spaces"
 Block name 2 -> Action /NoCheck /Argument1 /Argument2:MyValue /Argument3:"My value with spaces"
as @action_block
```

Syntax elements:

- when: optional clausule, previous action blocks whose actions must have all met their finish condition. If this clausule is not added to the action block, it stats at the begining.
- run [content] as [@alias|none]
    - content: list of actions of the action block.
    - @alias: name for the action block. This name is used for others actions blocks in their "when" clausule.



## Action list

| Block type            | Action name            | Description                                     | Finish condition              | Arguments |
| --------------------- | ---------------------- | ----------------------------------------------- | ----------------------------- | --------- |
| Any functional block  | Enable / On            | Switches on the block                           | Block enabled                 |           |
|                       | Disable / Off          | Switches off the block                          | Block disabled                |           |
| Door (all types)      | Open                   | Opens the door                                  | Door fully opened             |           |
|                       | Close                  | Close the door                                  | Door fully closed             |           |
| Air Vent              | Pressurize             | Change block mode to pressurize mode            | Block status is 'pressurized' |           |
|                       | Depressurize           | Change block mode to depressurize mode          | Oxygen level is 0             |           |
| Merge block           | Enable / On / Lock     | Switches on the block                           | Merge state is locked         |           |
|                       | Disable / Off / Unlock | Switches off the block                          | Merge state is not locked     |           |
| Rotor                 | Forward                | Sets positive displacement                      | Immediately                   |           |
|                       | Back                   | Sets negative displacement                      | Immediately                   |           |
| Piston                | Extend                 | Sets positive displacement                      | Piston is fully extended      |           |
|                       | Retract                | Sets netative displacement                      | Piston is fully retracted     |           |
| Sound block / Jukebox | Play                   | Starts sound                                    | Immediately                   |           |
|                       | Stop                   | Stops sound                                     | Immediately                   |           |
| Timer block           | Start                  | Begins timer countdown                          | Timer countdown ends          |           |
|                       | Stop                   | Stops current timer countdown                   | Timer countdown ends          |           |
|                       | Trigger                | Triggers timer inmediatelly, skips<br>countdown | Immediately                   |           |
