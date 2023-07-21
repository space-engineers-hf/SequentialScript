# SequentialScript

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
