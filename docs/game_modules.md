# Game modules

## Directory structure

The directory structure for given tasks are following:

```
- [task root dir]/
  |- src/[bot id]/ - directory containing extracted submission files
  |  \- .metadata? - additional data about the submission (language, user name etc.)
  |- bin/[bot id]/ - directory to put compiled files in (will be used when match is ran)
  |- in/ 
  | |- config.json - game module configuration for this task
  | \-... additional files requested by the configuration (extracted to this folder)
  |- out/ - place where output files will be stored
  | |- check.[bot id].txt - output of the checker stage of validation
  | |- compile.[bot id].txt - output of the compiler stage of validation
  | |- validate.[bot id].txt - output of the validator stage of validation
  | |- execute.[bot id].txt - logs produced by bot during the match
  | |- execute.txt - logs produced by game module during the match
  | |- results.json - file with detailed results of the match
  | \.. - any other output files (replays etc)
```

Note that some files are not produced when only validation of submission is performed.

## Game module interface

This section lists the expected behavior from the game module implementation. Following behavior is
same for all entry points.

Logs:
- Standard tex output should be logged to stdout, this should preferably be short.
- Stderr stream can be used for more detailed logs, which can be used for debugging.

Exit value:
- `0` if the action finished successfully
- `200` if the the action finished successfully, but with negative result. This value should be used
  to indicate e.g. that the validation was unsuccessfull.
- Any other value indicates an error inside the game module, This should happen only when the game
  module's implementation is buggy.

### Checker entrypoint

Input arguments:

    [In folder] [Bot src folder]

Outputs: 
- None

### Compiler entrypoint

Input arguments:

    [In folder] [Bot src folder] [Bot bin folder]

Outputs: 
- Store any files needed for execution to provided bin folder

### Validator entrypoint

Input arguments:

    [In folder] [Bot bin folder] 

Outputs: 
- None

### Executor entrypoint

Input arguments:

    [In folder] [Bot 0 bin folder]...[Bot N bin folder] [Out folder]

Outputs:
- If applicable, store bot-specific logs into provided output folder, names should be in format
  `bot.[index].log`, where `[index]` is the zero-based index of the folder the bot came from.
- store result.json to output directory
