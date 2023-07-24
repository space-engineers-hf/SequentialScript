# Sequential Script (Pressurized Door Example)

[< User Manual](README.md)

Components:

- 1 Slide Door. Door is disabled to force to open it using the sequence.
- 2 Air Vent Full
- 1 Oxygen Tank
- 2 Sci-Fi button panels for open/close door and show status.

Sequence:

- Open
    - Set Air Vents in "Depressurize mode". Display "Depressutizing..." in the button panel.
    - Enable and open Door.
    - Display "Open" in the button panel.
- Close
    - Close Door.
    - Disable Door and set Air Vents in "Pressurize mode". Display "Pressurizing..." in the button panel.
    - Display "Closed" in the button panel.
- Switch
    - If the door is open, close it
    - If the door is closed, open it

Video:

![Video](examples_pressurized_door_demo.gif)

Sequence: 

```
[OPEN]
run
 *Sliding Door Buttons* -> Display /Background:Orange /Text:"Depresurizing..."
 Air Vent Full A -> Depressurize // Depressurize and wait until oxygen is 0
 Air Vent Full B -> Depressurize
as @room_depressurized

when @room_depressurized
run
 Sliding Door -> Enable
 Sliding Door -> Open
as @door_opened

when @door_opened
run
 *Sliding Door Buttons* -> Display /Background:Green /Text:"Open"
as @done

[CLOSE]
run
 Sliding Door -> Close
as @door_closed

when @door_closed
run
 Sliding Door -> Disable
 *Sliding Door Buttons* -> Display /Background:Orange /Text:"Presurizzing..."
 Air Vent Full A -> Pressurize // Pressurize and wait until room is pressurized.
 Air Vent Full B -> Pressurize
as @room_pressurized

when @room_pressurized
run
 *Sliding Door Buttons* -> Display /Background:Red /Text:"Closed"
as @done

[SWITCH]
if #CLOSE
 #OPEN
else
 #CLOSE
end if
```
