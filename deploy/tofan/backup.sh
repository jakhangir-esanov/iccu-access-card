#!/usr/bin/env bash
set -euo pipefail

BACKUP_DIR=/opt/iccu/backups
FILES_DIR=/opt/iccu/files
RETENTION_DAYS=14

stamp=$(date +%Y%m%d-%H%M)
container=$(docker ps -qf name=tofan_postgres | head -n1)

if [ -z "$container" ]; then
    echo "$(date -Is) postgres container not found" >&2
    exit 1
fi

mkdir -p "$BACKUP_DIR/db" "$BACKUP_DIR/files"

dump="$BACKUP_DIR/db/iccu-$stamp.dump"
docker exec "$container" pg_dump -U postgres -d iccu -Fc > "$dump.tmp"
mv "$dump.tmp" "$dump"

rsync -a "$FILES_DIR/" "$BACKUP_DIR/files/"

find "$BACKUP_DIR/db" -name 'iccu-*.dump' -mtime +"$RETENTION_DAYS" -delete

echo "$(date -Is) backup done: $dump"
