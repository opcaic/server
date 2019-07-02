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

This section lists the expected behavior from the game module implementation:

### Checker entrypoint

Input arguments:
> [In folder] [Bot src folder]

Outputs: 
- None, only log to stdout

Exit value: nonzero when failed

### Compiler entrypoint

Input arguments:
> [In folder] [Bot src folder] [Bot bin folder]

Outputs: 
- Store any files needed for execution to provided bin folder
- log to stdout

Exit value: nonzero when failed

### Validator entrypoint

Input arguments:
> [In folder] [Bot bin folder] 

Outputs: 
- None, only log to stdout

Exit value: nonzero when failed

### Executor entrypoint

Input arguments:
> [In folder] [bin root folder] [out folder]

Outputs:
- Store logs to out directory (perhaps even bot-specific logs)
- store result.json to output directory

Exit value: nonzero when failed
