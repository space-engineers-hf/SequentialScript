# SequentialScript (User documentation)

- [About](#about)
- [Syntax](#syntax)
    - [Blocks](#blocks)
    - [Sequence](#sequence)
    - [Arguments](#arguments)
    - [Delay](#delay)
    - [Comments](#comments)
- [Action list](#action-list)
- [Known issues](#known-issues)
- Examples
    - [Hydrogen charger](Examples_Hydrogen_Charger.md)
    - [Pressurized Door](Examples_Pressurized_Door.md)

---

## About


This script allows you to create complex sequences that would typically require **multiple timer blocks** by using **only one programmable block** and writing some instructions in its "Custom Data" section.

For example, you can set up a button that depressurizes the room before opening the door.

```
[OPEN_DOOR]
run
 Air Vent -> Depressurize
as @room_depressurized

when @room_depressurized
 Door -> Enable
 Door -> Open
@as @door_opened
```

Another example is a hinge that starts to move after a piston is fully extended.
```
[PISTON_ROTOR]
run
 Piston -> Extend
as @piston_extended

when @piston_extended
run
 Hinge -> Forward
as @hinge_moved

```

---

## Syntax

The syntax is divided into the following elements:
- **Command**: The name of the set of actions to be performed.
- **Instruction block**: A set of actions that should start simultaneously. This is equivalent to a **Timer Block**. Instruction blocks can be initiated when the command is executed or upon the completion of other instruction blocks.
- **Instruction**: Actions that must be carried out within the instruction block. This is equivalent to the actions placed inside a **Timer Block**. Actions are defined in the following [table](#action-list).

![Syntax](syntax.png)

![Command Syntax](command_syntax.png)

### Blocks
In the **instruction** region, it is possible to set individual blocks or groups.

Individual block:
```
Industrial Hydrogen Thruster -> Action
```

Block group:
```
*All Industrial Hydrogen Thrusters* -> Action
```
![All Industrial Hydrogen Thrusters](block_name_group.png)

If there are many blocks with the same name, individual block name will apply to all of them.
```
Industrial Hydrogen Thruster -> Enable
```
![Industrial Hydrogen Thruster](block_name_same.png)



### Sequence

An **instruction block** is considered finished when all its instructions have met their finish condition.

![Instruction Block Done](sequence_example_done.png)

All **instruction blocks** must have an *alias*. This alias can be used by other **instruction blocks** to specify that they cannot start until this one has finished.

Use the *when* clause to make an **instruction block** wait until one or more other **instruction blocks** have finished.
```
when @previous_action1
run
 <actions>
as @action_block
```

If an **instruction block** must run at the beginning of the command, omit the *when* clause.
```
run
 <actions>
as @first_block
```

If the **instruction block** must wait for multiple other **instruction blocks**, list all aliases separated by commas.
```
when @previous_action1, @previous_action2
run
 <actions>
as @action_block
```

It is also possible to run multiple **instruction blocks** when one **instruction block** has finished.
```
run
 <actions>
as @first_block

when @first_block
run
 <actions>
as @second_block

when @first_block
run
 <actions>
as @thrid_block
```

The following example is a combination of all the previous cases:
```
[COMMAND_NAME]
run
 Block 1 -> Action A
as @groupA

when @groupA
run
 Block 2 -> Action B
as @groupB

when @groupB
run
 *Block Group* -> Action C
as @groupC

when @groupA
run
 Block 3 -> Action X
 Block 4 -> Action Y
as @groupD

when @groupC, @groupD
run
 Block 5 -> Action Z
as @groupE
```

![Sequence Example Mix](sequence_example_mix.png)

### Arguments
Actions can have arguments:
```
run
 Block -> Action /Argument1 /Argument2:MyValue /Argument3:"My value with spaces"
as @action_block
```

#### NoWait
Sometimes, you may want the **instruction block** to be considered finished even if some actions are still running. In these cases, you can use the "/NoWait" argument:
```
when @previous_instruction_block
run
 Block 1 -> Action A
 Block 2 -> Action B /NoWait
 *Block group* -> Action C
as @instruction_block
```

![Instruction Block NoWait](sequence_example_done_nowait.png)

#### Wait
Similarly, you can set the maximum time (in milliseconds) that the action should wait before being considered finished. If the action ends earlier, it will be taken into account; otherwise, it will be ignored after the time expires.

The following example waits for the block to depressurize for up to 3 seconds. If the room needs only 1 second to depressurize, the door will open at that moment, but if it takes more than 3 seconds, the door will open anyway when the time expires.
```
run
 Air Vent 1 -> Depressurize /Wait:3000
as @room_depressurized

when @room_depressurized
run
 Door 1 --> Open
as @done
```

#### Custom-defined arguments by action
Some actions have their own arguments. These arguments are defined in the [action list](#action-list).

### Delay
Include the "DELAY" instruction (in milliseconds) when it is necessary to wait for a specific time before a sequence block is done.

The following example waits for 3 seconds before switching on the second light.
```
[COMMAND_NAME]
run
 Interior Light 1 -> Enable
 delay 3000 // Waits for 3 seconds.
as @light1_on

when @light1_on
run
 Interior Light 2 -> Enable
as @light2_on
```

### Comments

Sometimes it is necessary to write text in the sequence to explain what it is doing. You can write comments by typing // at the beginning of the line.
```
[COMMAND_NAME]
// Start automatically.
run
 Interior Light 1 -> Enable
 Piston 1 -> Extend // Extends piston and waits until it reaches the max value.
as @piston_extended

// Starts when piston is fully extended.
when @piston_extended
run
 Interior Light 1 -> Disable
 // Hinge max degrees is 90º.
 Hinge 1 -> Forward
 Hinge 2 -> Forward
as @done
```

---

## Action list

| Block type                    | Action name            | Description                                                      | Finish condition                                                                                 | Arguments             | Remarks                                                                                   |
| ----------------------------- | ---------------------- | ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ | --------------------- | ----------------------------------------------------------------------------------------- |
| Any functional block          | Enable / On            | Turns the block on                                               | Block enabled                                                                                    |                       |                                                                                           |
|                               | Disable / Off          | Turns the block off                                              | Block disabled                                                                                   |                       |                                                                                           |
| Door (all types)              | Open                   | Opens the door                                                   | Door fully opened                                                                                |                       |                                                                                           |
|                               | Close                  | Closes the door                                                  | Door fully closed                                                                                |                       |                                                                                           |
|                               | Toggle                 | Switches the door state between open and closed                  | Door is open or closed (not moving)                                                              |                       |                                                                                           |
| Air Vent                      | Pressurize             | Sets the block to pressurize mode                                | Block status is 'pressurized'                                                                    |                       |                                                                                           |
|                               | Depressurize           | Sets the block to depressurize mode                              | Oxygen level is 0                                                                                |                       |                                                                                           |
| Oxygen / Hydrogen Tank        | Stockpile              | Forces the tank to fill itself<br>It will not release any gas    | Stockpile mode enabled and tank fully filled                                                     |                       |                                                                                           |
|                               | Auto                   | Tank automatically loads and unloads gas                         | Stockpile mode disabled                                                                          |                       |                                                                                           |
| Merge block                   | Enable / On / Lock     | Turns the block on                                               | Merge state is locked (green color)                                                              |                       |                                                                                           |
|                               | Disable / Off / Unlock | Turns the block off                                              | Merge state is not locked (not green color)                                                      |                       |                                                                                           |
| Connector                     | Connect / Lock         | Connects the connector                                           | Connector is connected (green color)                                                             |                       |                                                                                           |
|                               | Disconnect / Unlock    | Disconnects the connector                                        | Connector is not connected (not green color)                                                     |                       |                                                                                           |
|                               |                        |                                                                  |                                                                                                  | /FULL                 | Waits until the connector is out of range of the other connector (white color)             |
| Rotor                         | Forward                | Moves in the positive direction                                  | Movement is positive<br>If an upper limit is set, checks that the angle has reached the limit    |                       |                                                                                           |
|                               | Back                   | Moves in the negative direction                                  | Movement is negative<br>If a lower limit is set, checks that the angle has reached the limit     |                       |                                                                                           |
|                               | Detach                 | Detaches the head from the base                                  | There is no head attached to the base                                                            |                       |                                                                                           |
|                               | Set                    | Changes one or more properties of the block                      |                                                                                                  |                       |                                                                                           |
|                               |                        |                                                                  |                                                                                                  | /MAX:<number>         | Sets the maximum displacement of the rotor                                                 |
|                               |                        |                                                                  |                                                                                                  | /MIN:<number>         | Sets the minimum displacement of the rotor                                                 |
| Piston                        | Extend                 | Moves in the positive direction                                  | Piston is fully extended                                                                         |                       |                                                                                           |
|                               | Retract                | Moves in the negative direction                                  | Piston is fully retracted                                                                        |                       |                                                                                           |
|                               | Reverse                | Reverses piston direction                                        | Piston is fully extended or retracted (not moving)                                               |                       |                                                                                           |
|                               | Set                    | Changes one or more properties of the block                      |                                                                                                  |                       |                                                                                           |
|                               |                        |                                                                  |                                                                                                  | /MAX:<number>         | Sets the maximum displacement of the piston                                                |
|                               |                        |                                                                  |                                                                                                  | /MIN:<number>         | Sets the minimum displacement of the piston                                                |
| Sound block / Jukebox         | Play                   | Starts playing sound                                             | Immediately                                                                                      |                       |                                                                                           |
|                               | Stop                   | Stops playing sound                                              | Immediately                                                                                      |                       |                                                                                           |
| Timer block                   | Start                  | Starts the timer countdown                                       | Timer countdown ends                                                                             |                       |                                                                                           |
|                               | Stop                   | Stops the current timer countdown                                | Timer countdown stops                                                                            |                       |                                                                                           |
|                               | Trigger                | Instantly triggers the timer, skipping the countdown             | Immediately                                                                                      |                       |                                                                                           |
| Programmable block            | Run                    | Runs the programmable block                                      | Programmable block finishes running                                                              |                       |                                                                                           |
| Battery                       | Recharge               | Sets battery to recharge mode                                   | Battery is fully charged                                                                         |                       |                                                                                           |
|                               | Discharge              | Sets battery to discharge mode                                  | Battery is empty                                                                                 |                       |                                                                                           |
|                               | Auto                   | Sets battery to auto mode                                       | Immediately                                                                                      |                       |                                                                                           |
| Light (all types)             | Set                    | Changes one or more properties of the block                      | All properties already changed                                                                   |                       |                                                                                           |
|                               |                        |                                                                  |                                                                                                  | /COLOR:<color>        | Sets the light color<br>See [color](#color) type                                          |
| LCDs & other display surfaces | Display                | Changes one or more properties of the block                      | All properties already changed                                                                   |                       |                                                                                           |
|                               |                        |                                                                  |                                                                                                  | /INDEX:<number>       | Required if the block has several displays (like cockpits)                                 |
|                               |                        |                                                                  |                                                                                                  | /BACKGROUND:<color>   | Sets the background color of the display<br>See [color](#color) type                      |
|                               |                        |                                                                  |                                                                                                  | /COLOR:<color>        | Sets the text color of the display<br>See [color](#color) type                            |
|                               |                        |                                                                  |                                                                                                  | /TEXT:<string>        | Sets the text to show on the display                                                       |
| Thrusters                     | Set                    | Changes one or more properties of the block                      | All properties already changed                                                                   |                       |                                                                                           |
|                               |                        |                                                                  |                                                                                                  | /OVERRIDE:<number>    | Sets the override value.<br>Must be between 0 and 1, where 0 is disabled and 1 is 100%    |
| Warhead                       | Arm                    | Arms the warhead                                                 | Warhead is armed                                                                                 |                       |                                                                                           |
|                               | Disarm                 | Disarms the warhead                                              | Warhead is disarmed                                                                              |                       |                                                                                           |
|                               | Detonate               | Detonates the warhead                                            | Immediately                                                                                      |                       |                                                                                           |
|                               | Start                  | Starts the timer countdown                                       | Timer countdown ends                                                                             |                       |                                                                                           |
|                               | Stop                   | Stops the current timer countdown                                | Timer countdown stops                                                                            |                       |                                                                                           |

**Trick:** If there are some action that is not in the *action list*, is is possible to use a timer block. Build a timer block, add the action (or actions) and run it using the following sentence. This will run the timer block actions when the previous **instruction block** has finished.
```
when @previous_instruction_block
run
 My Timer Block -> Trigger
as @instruction_block
```

## Color
Some actions allow you to change the color (for example, lights and LCDs).

There are several ways to set the color:

| Type | Format                 | Description                                                                |
| ---- | ---------------------- | -------------------------------------------------------------------------- |
| Name | /COLOR:RED             | Sets the light color using the specified name.                             |
| HTML | /COLOR:#FF0000         | Sets the light color using the specified HTML hex code.                    |
| RGB  | /COLOR:250;128;114     | Sets the light color using the specified RGB values (RED;GREEN;BLUE).      |
| ARGB | /COLOR:200;250;128;114 | Sets the light color using the specified ARGB values (ALPHA;RED;GREEN;BLUE)|

Space Engineers uses the RGB option in-game:

![Ingame color RGB](ingame_color_rgb.png)

**Warning:** Due to limitations in Space Engineers, some colors may not display properly. The best way to choose a color is to test it in-game first.

## Known Issues

**Issue 1**

*Message:* Script execution terminated, script is too complex. Please edit and rebuild the script.

*Description:* This occurs when the command has too many instructions.