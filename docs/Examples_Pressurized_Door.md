# Sequential Script (Pressurized Door Example)

[< User Manual](README.md)


Components:

- 1 Sliding Door. The door is disabled to ensure it can only be opened using the sequence.
- 2 Air Vents Full
- 1 Oxygen Tank
- 2 Sci-Fi button panels to open/close the door and display status.



Sequence:

- Open
    - Set Air Vents to "Depressurize mode". Display "Depressurizing..." on the button panel.
    - Enable and open the Door.
    - Display "Open" on the button panel.
- Close
    - Close the Door.
    - Disable the Door and set Air Vents to "Pressurize mode". Display "Pressurizing..." on the button panel.
    - Display "Closed" on the button panel.
- Switch
    - If the door is open, close it.
    - If the door is closed, open it.



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
