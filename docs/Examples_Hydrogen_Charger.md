# Sequential Script (Hydrogen Charger Example)

[< User Manual](README.md)

Components:

- 6 Large Hydrogen Thrusters for waste the hydrogen from the tank.
- 1 Piston
- 1 Connector
- 1 Small Hydrogen Tank 
- 1 Button Panel
- 1 LCD Panel for showing a message with the current progress.

Sequence:

- Disable Hydrogen Thrusters
- Extend pinston until max position and connect both connectors. Display "Connecting..." in the LCD.
- Enable "Stock pile" in the Small Hydrogen Tank. Display "Recharging..." in the diplay.
- When the Hydrogen is full, disable "Stock pile" in the Small Hydrogen Tank, disconnect connectors and retract piston. Display "Disconnecting..." in the LCD.
- Show "Disconnected".

Video:

![Video](examples_hydrogen_charger_demo.gif)

Sequence: 

```
[HYDROGEN_STOCKPILE]
run
 Hydrogen Example (Thruster) -> Disable
as @thrusters_disabled

run
 Hydrogen Example (Status LCD) -> Set /Background:#FF4600 /Text:"Connecting..."
 Hydrogen Example (Piston) -> Extend
as @piston_extended

when @piston_extended
run
 Hydrogen Example (Connector) -> Connect
as @connector_locked

when @connector_locked
run
 Hydrogen Example (Status LCD) -> Set /Background:Blue /Text:"Recharging..."
 Hydrogen Example (Tank) -> Stockpile // Enables "Stockpile" mode and waits until tank is full
as @tank_full

when @tank_full
run
 Hydrogen Example (Status LCD) -> Set /Background:#FF4600 /Text:"Disconnecting..."
 Hydrogen Example (Tank) -> Auto // Disables "Stockpile" mode
 Hydrogen Example (Connector) -> Disconnect
 Hydrogen Example (Piston) -> Retract
as @piston_retracted

when @piston_retracted
run
 Hydrogen Example (Status LCD) -> Set /Background:Red /Text:"Disconnected"
as @done
```
