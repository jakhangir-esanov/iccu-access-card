#!/usr/bin/env bash
set -euo pipefail

BACKUP_SCRIPT=/opt/iccu/backups/backup.sh
FILES_DIR=/opt/iccu/files
API_SERVICE=iccu_iccu-api
CONFIRMATION=TOZALA

container=$(docker ps -qf name=tofan_postgres | head -n1)
if [ -z "$container" ]; then
    echo "postgres container not found" >&2
    exit 1
fi

psql_iccu() {
    docker exec -i "$container" psql -U postgres -d iccu -v ON_ERROR_STOP=1 "$@"
}

echo "iccu bazasidagi hozirgi yozuvlar:"
psql_iccu -At -F ' | ' -c "
    SELECT 'kitobxonlar', count(*) FROM iccu.readers
    UNION ALL SELECT 'arizalar', count(*) FROM iccu.registration_requests
    UNION ALL SELECT 'rasmlar', count(*) FROM iccu.stored_files
    UNION ALL SELECT 'foydalanuvchilar (qoladi)', count(*) FROM iccu.users;"
echo "Rasm fayllari: $(find "$FILES_DIR" -mindepth 1 -type f | wc -l)"
echo
echo "Kitobxonlar, arizalar va rasmlar butunlay o'chiriladi."
echo "Karta raqami 0000001 dan, ariza kodi 0001 dan qayta boshlanadi."
echo "Foydalanuvchilar va ularning parollari qoladi."
read -r -p "Davom etish uchun $CONFIRMATION deb yozing: " answer
if [ "$answer" != "$CONFIRMATION" ]; then
    echo "Bekor qilindi."
    exit 1
fi

echo "1/5 Zaxira nusxa olinmoqda..."
"$BACKUP_SCRIPT"

echo "2/5 API to'xtatilmoqda..."
docker service scale "$API_SERVICE"=0

restore_api() {
    echo "API qayta ishga tushirilmoqda..."
    docker service scale "$API_SERVICE"=1
}
trap restore_api EXIT

echo "3/5 Baza tozalanmoqda..."
psql_iccu <<'SQL'
BEGIN;
TRUNCATE iccu.registration_requests, iccu.readers, iccu.stored_files;
ALTER SEQUENCE iccu.card_number_seq RESTART WITH 1;
ALTER SEQUENCE iccu.registration_code_seq RESTART WITH 1;
COMMIT;
SQL

echo "4/5 Rasm fayllari o'chirilmoqda..."
sudo find "$FILES_DIR" -mindepth 1 -delete

echo "5/5 Tekshirish:"
psql_iccu -At -F ' | ' -c "
    SELECT 'kitobxonlar', count(*) FROM iccu.readers
    UNION ALL SELECT 'arizalar', count(*) FROM iccu.registration_requests
    UNION ALL SELECT 'rasmlar', count(*) FROM iccu.stored_files;"
echo "Rasm fayllari: $(find "$FILES_DIR" -mindepth 1 -type f | wc -l)"
echo "Tayyor. Zaxira: /opt/iccu/backups/db (eng oxirgi iccu-*.dump)"
