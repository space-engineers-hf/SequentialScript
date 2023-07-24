# SequentialScript

It allows creating complex sequences that typically require **multiple timer blocks** by using **only one programming block** and writing some instructions in its "Custom Data" section.

For example, it is possible to command button that depressurize the room before open the door.

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

Other example is a hinge starting to move after some piston is fully extended.
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

## Download source code

Requirements:
 - Visual Studio 2022
 - Download and install https://github.com/malware-dev/MDK-SE

Download both, this repository and [CommonScript](https://github.com/space-engineers-hf/CommonScript) repository in the same parent folder.
For example:

```
repos
└─ CommonScript
└─ SequentialScript
```

## User documentation

Documentation about how to use SequentialScript is in the following [link](docs/README.md).