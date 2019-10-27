#!/bin/bash
 
###########################
####### LOAD CONFIG #######
###########################

SCRIPTPATH=$(cd ${0%/*} && pwd -P)
source $SCRIPTPATH/backup.config

BACKUP_FILENAME=$(basename $FILE_ROOT).tar.gz
BACKUP_SQLNAME=$(basename $FILE_ROOT).tar.gz

if [ $# -ne 1 ]; then
	echo "usage: $0 <archive_dir>"
	exit 1
fi

BACKUP_ROOT=$1
BACKUP_FILE=$BACKUP_ROOT/$BACKUP_FILENAME

 
############################
#### PRE-RESTORE CHECKS ####
############################
 
# Make sure we're running as the required backup user
if [ "$BACKUP_USER" != "" -a "$(id -un)" != "$BACKUP_USER" ]; then
	echo "This script must be run as $BACKUP_USER. Exiting." 1>&2
	exit 1;
fi;
 
###########################
### INITIALISE DEFAULTS ###
###########################
 
if [ ! $HOSTNAME ]; then
	HOSTNAME="localhost"
fi;
 
if [ ! $USERNAME ]; then
	USERNAME="postgres"
fi;

###########################
##### RESTORE DATABASE ####
###########################

for file in `ls $BACKUP_ROOT/*.custom`; do
	DBNAME=$(basename ${file/.custom})

	DO_CLEAN="--create --clean --if-exists"
	if [ "$DBNAME" = "postgres" ]; then # no need to drop this database
		DO_CLEAN=--clean
	fi

	if !	pg_restore -h $HOSTNAME -U $USERNAME $DO_CLEAN --dbname=postgres --exit-on-error "$file" >/dev/null; then
		echo "[!!ERROR!!] Error restoring database $DBNAME" 1>&2
		exit $?
	else
		echo "Database $DBNAME Restored!"
	fi
done;

###########################
###### RESTORE FILES ######
###########################
 
mkdir -p "$FILE_ROOT"
tar -xzpf "$BACKUP_FILE" --same-owner --overwrite-dir -C $(dirname $FILE_ROOT)

echo "File backup restored!"
