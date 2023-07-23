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

| Block type             | Action name            | Description                                                     | Finish condition                                                                                 | Arguments       | Remarks                                                                                               |
| ---------------------- | ---------------------- | --------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ | --------------- | ----------------------------------------------------------------------------------------------------- |
| Any functional block   | Enable / On            | Switches on the block                                           | Block enabled                                                                                    |                 |                                                                                                       |
|                        | Disable / Off          | Switches off the block                                          | Block disabled                                                                                   |                 |                                                                                                       |
| Door (all types)       | Open                   | Opens the door                                                  | Door fully opened                                                                                |                 |                                                                                                       |
|                        | Close                  | Close the door                                                  | Door fully closed                                                                                |                 |                                                                                                       |
| Air Vent               | Pressurize             | Change block mode to pressurize mode                            | Block status is 'pressurized'                                                                    |                 |                                                                                                       |
|                        | Depressurize           | Change block mode to depressurize mode                          | Oxygen level is 0                                                                                |                 |                                                                                                       |
| Oxygen / Hydrogen Tank | Stockpile              | Force to the tank to fill itself<br>It will not release any gas | Stockpile mode enabled and tank fully filled                                                     |                 |                                                                                                       |
|                        | Auto                   | Tank loads and unloads gas automatically                        | Stockpile mode disabled                                                                          |                 |                                                                                                       |
| Merge block            | Enable / On / Lock     | Switches on the block                                           | Merge state is locked (green color)                                                              |                 |                                                                                                       |
|                        | Disable / Off / Unlock | Switches off the block                                          | Merge state is not locked (not green color)                                                      |                 |                                                                                                       |
| Connector              | Connect / Lock         |                                                                 | Connector is connected (green color)                                                             |                 |                                                                                                       |
|                        | Disconnect / Unlock    |                                                                 | Connector is not connected (not green color)                                                     |                 |                                                                                                       |
| Rotor                  | Forward                | Sets positive displacement                                      | Movement is positive<br>If upper limit is set, it checks that the angle has reached to the limit |                 |                                                                                                       |
|                        | Back                   | Sets negative displacement                                      | Movement is negative<br>If lower limit is set, it checks that the angle has reached to the limit |                 |                                                                                                       |
| Piston                 | Extend                 | Sets positive displacement                                      | Piston is fully extended                                                                         |                 |                                                                                                       |
|                        | Retract                | Sets netative displacement                                      | Piston is fully retracted                                                                        |                 |                                                                                                       |
| Sound block / Jukebox  | Play                   | Starts sound                                                    | Immediately                                                                                      |                 |                                                                                                       |
|                        | Stop                   | Stops sound                                                     | Immediately                                                                                      |                 |                                                                                                       |
| Timer block            | Start                  | Begins timer countdown                                          | Timer countdown ends                                                                             |                 |                                                                                                       |
|                        | Stop                   | Stops current timer countdown                                   | Timer countdown ends                                                                             |                 |                                                                                                       |
|                        | Trigger                | Triggers timer inmediatelly, skips<br>countdown                 | Immediately                                                                                      |                 |                                                                                                       |
| Programmable block     | Run                    |                                                                 | Programable block ends running                                                                   |                 |                                                                                                       |
| Battery                | Recharge               |                                                                 |                                                                                                  |                 |                                                                                                       |
|                        | Discharge              |                                                                 |                                                                                                  |                 |                                                                                                       |
|                        | Auto                   |                                                                 |                                                                                                  |                 |                                                                                                       |
| Light (all types)      | Set                    | Changes some property or properties in the block                | All properties already changed                                                                   | /COLOR:\<name\> | Sets the light color with the specified name<br>Example: RED                                          |
|                        |                        |                                                                 |                                                                                                  | /COLOR:\<html\> | Sets the light color with the specified html hex code<br>Example: #FF0000                             |
|                        |                        |                                                                 |                                                                                                  | /COLOR:\<rgb\>  | Sets the light color with the specified RGB color (RED;GREEN;BLUE)<br>Example: 250;128;114            |
|                        |                        |                                                                 |                                                                                                  | /COLOR:\<argb\> | Sets the light color with the specified ARGB color (ALPHA;RED;GREEN;BLUE)<br>Example: 200;250;128;114 |
