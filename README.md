# Sequential Script

This tool allows you to create complex sequences that would typically require **multiple timer blocks** by using **only one programmable block** and writing some instructions in its "Custom Data" section.

For example, you can set up a button that depressurizes a room before opening the door.

```
[OPEN_DOOR]
run
 Air Vent -> Depressurize
as @room_depressurized

when @room_depressurized
 Door -> Enable
 Door -> Open
as @door_opened
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

## Download Source Code

Requirements:
 - Visual Studio 2022
 - Download and install https://github.com/malware-dev/MDK-SE

Download both this repository and the [CommonScript](https://github.com/space-engineers-hf/CommonScript) repository into the same parent folder.
For example:

```
repos
└── CommonScript
└── SequentialScript
```

## User Documentation

You can find documentation on how to use SequentialScript at the following [link](docs/README.md).