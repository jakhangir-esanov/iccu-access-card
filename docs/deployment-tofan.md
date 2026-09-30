# ICCU — tofan serverida vaqtinchalik ko'rsatish

> **Vaqtinchalik.** Bu qo'llanma va `deploy/tofan/` papkasi faqat `deploy/tofan-demo` branch'ida turadi. Maqsad: tizimni ma'muriyatga ko'rsatib tasdiqlatish. Doimiy o'rnatish Markazning o'z serverida bo'ladi, u [deployment.md](deployment.md) va `deploy/`ning qolgan fayllarida yozilgan. Bu branch `main`ga qo'shilmaydi.

ICCU tofan ishlab turgan Hetzner serverga, uning yoniga qo'shiladi. Server, Docker Swarm, `tofan-net` tarmog'i, Postgres, nginx, certbot va firewall allaqachon sozlangan (`tofan` reposidagi `DEPLOYMENT.md`). Bu qo'llanma faqat ICCU uchun qo'shiladigan narsalarni yozadi.

| Ma'lumot | Qiymat |
|---|---|
| Server | `157.90.117.20` (tofan bilan bitta) |
| Foydalanuvchi | `deploy` |
| Ish papkasi | `/opt/iccu` |
| Manzil | https://iccu.157.90.117.20.sslip.io |
| Stack | `iccu`: `iccu-api`, `iccu-web` |
| Tarmoq | `tofan-net` (tofan'niki) |
| Baza | tofan'ning `postgres` xizmatida alohida `iccu` bazasi va foydalanuvchisi |
| nginx | tofan'ning `nginx` xizmati, `/opt/tofan/nginx/conf.d/50-iccu.conf` |

---

## 0. Umumiy tuzilma

ICCU o'z Postgres'i va nginx'ini ko'tarmaydi. Server 80 va 443 portlarini tofan nginx'i band qilgan, ikkinchi nginx u portlarni ololmaydi. Alohida Postgres esa 4 GB RAM'li serverda ortiqcha xotira yeydi.

ICCU xizmatlari `tofan-net` tarmog'iga ulanadi. Shunda tofan nginx'i ularni `iccu-api` va `iccu-web` nomi bilan, API esa bazani `postgres` nomi bilan topadi. Tofan stack fayllariga tegilmaydi.

```text
Internet
   |  :443
   v
[ tofan nginx ]
   |-- api./id./portainer./pgadmin.  -> tofan xizmatlari (o'zgarmaydi)
   |-- iccu.157.90.117.20.sslip.io
          |-- /api/hubs/  -> iccu-api:8080/hubs/   (WebSocket)
          |-- /api/       -> iccu-api:8080/        (/api olib tashlanadi)
          |-- /           -> iccu-web:8080         (React build)
              |
        tofan-net (overlay)
              |
      [ postgres:5432 ]
        |-- tofan, keycloak  (tofan)
        |-- iccu             (ICCU)
```

UI va API bitta host'da turadi, shuning uchun CORS kerak emas va `iccu_refresh` cookie'si `/api/auth` yo'lida ishlaydi.

**Admin API IP bo'yicha cheklanmagan.** Server kutubxona tarmog'ida emas, kutubxonaning ochiq IP'si hali ma'lum emas. Hozircha API'ni faqat login himoya qiladi (JWT, 5 urinishdan keyin 15 daqiqa blok). IP ma'lum bo'lgach `50-iccu.conf`da `/api/public/` uchun alohida `location` ochiladi, `/api/` va `/api/hubs/` bloklariga esa `allow <IP>; deny all;` qo'shiladi.

## 1. Papkalar

```bash
sudo mkdir -p /opt/iccu/{stacks,env,files,backups}
sudo chown -R deploy:deploy /opt/iccu
sudo chown 1654:1654 /opt/iccu/files
```

`1654` — .NET konteyneridagi `app` foydalanuvchisining UID'si. API yuklangan rasmlarni `/opt/iccu/files`ga yozadi.

> **Muhim.** API stack'ida `/opt/iccu/files` mount'i bo'lishi majburiy. Mount tushib qolsa, rasmlar konteyner ichiga yoziladi va keyingi deploy'da jimgina yo'qoladi. Buning oldini olish uchun `Storage:RootPath` bo'sh bo'lsa API ataylab ishga tushmaydi.

Repodagi fayllarni serverga ko'chiring (kompyuterda, repo ildizida):

```bash
scp deploy/tofan/api.yml deploy/tofan/web.yml deploy@157.90.117.20:/opt/iccu/stacks/
scp deploy/tofan/backup.sh deploy@157.90.117.20:/opt/iccu/backups/
scp deploy/tofan/50-iccu.conf deploy@157.90.117.20:/tmp/
```

`50-iccu.conf` hozircha `/tmp`da qoladi, uni sertifikat olingandan keyin qo'yamiz (4-bo'lim).

## 2. iccu.env fayli

Parollar `/opt/iccu/env/iccu.env` faylida turadi. Faqat hex ishlatiladi, chunki maxsus belgilar connection string'ni buzadi.

```bash
cd /opt/iccu && cat > env/iccu.env <<EOF
ICCU_API_IMAGE=ejakhangir/iccu:latest
ICCU_WEB_IMAGE=ejakhangir/iccu-ui:latest

ICCU_DB_PASSWORD=$(openssl rand -hex 16)
ICCU_JWT_SIGNING_KEY=$(openssl rand -hex 32)
EOF

sudo chown -R root:deploy /opt/iccu/env
sudo chmod 750 /opt/iccu/env
sudo chmod 640 /opt/iccu/env/iccu.env
```

| O'zgaruvchi | Kim ishlatadi | API ichidagi kalit |
|---|---|---|
| `ICCU_DB_PASSWORD` | API; Postgres'dagi `iccu` foydalanuvchisi shu parol bilan yaratiladi | connection string'dagi `Password` |
| `ICCU_JWT_SIGNING_KEY` | API, kamida 32 bayt | `Jwt:SigningKey` |

Tahrirlash: `sudo nano /opt/iccu/env/iccu.env`.

## 3. Bazani yaratish

Tofan Postgres'ining init skripti faqat bo'sh volume'da bir marta ishlagan, shuning uchun `iccu` bazasi qo'lda yaratiladi:

```bash
cd /opt/iccu && set -a && . env/iccu.env && set +a \
  && docker exec $(docker ps -qf name=tofan_postgres) psql -U postgres -v ON_ERROR_STOP=1 \
     -c "CREATE USER iccu WITH PASSWORD '$ICCU_DB_PASSWORD';" \
     -c "CREATE DATABASE iccu OWNER iccu;"
```

Tekshirish:

```bash
cd /opt/iccu && set -a && . env/iccu.env && set +a \
  && docker run --rm --network tofan-net -e PGPASSWORD="$ICCU_DB_PASSWORD" \
     postgres:17-alpine psql -h postgres -U iccu -d iccu -c "select 'ok';"
```

Jadvallarni API birinchi ishga tushganda migratsiya bilan o'zi yaratadi (Hangfire'ning `hangfire` sxemasi ham shu bazada).

Keyin `env/iccu.env`da `ICCU_DB_PASSWORD`ni o'zgartirish bazadagi parolni o'zgartirmaydi. Kerak bo'lsa: `ALTER USER iccu WITH PASSWORD '...';`.

## 4. Sertifikat va nginx

Tofan'ning mavjud sertifikati `iccu.` nomini qamramaydi, ICCU uchun alohida sertifikat olinadi. Tofan nginx'idagi `00-acme.conf` 80-portda `/.well-known/acme-challenge/`ni allaqachon xizmat qiladi.

```bash
docker run --rm \
  -v /opt/tofan/certbot/letsencrypt:/etc/letsencrypt \
  -v /opt/tofan/certbot/www:/var/www/certbot \
  certbot/certbot certonly --webroot -w /var/www/certbot \
  -d iccu.157.90.117.20.sslip.io
```

Sertifikat `/opt/tofan/certbot/letsencrypt/live/iccu.157.90.117.20.sslip.io/`ga tushadi. Tofan'ning haftalik `certbot renew` cron'i uni ham yangilaydi, alohida cron kerak emas.

> **Tartib muhim.** Konfigni sertifikatdan oldin qo'ymang. Sertifikat fayli bo'lmasa `nginx -t` yiqiladi, nginx qayta ishga tushsa esa tofan ham to'xtaydi.

```bash
mv /tmp/50-iccu.conf /opt/tofan/nginx/conf.d/
docker exec $(docker ps -qf name=tofan_nginx) nginx -t \
  && docker exec $(docker ps -qf name=tofan_nginx) nginx -s reload
```

`50-iccu.conf` tofan'ning `00-acme.conf` faylidagi `map $http_upgrade $connection_upgrade`dan foydalanadi (SignalR WebSocket uchun). Bu `map`ni ICCU konfigida qayta yozmang, nginx takroriy o'zgaruvchi xatosini beradi.

`iccu-api` va `iccu-web` hali ko'tarilmagan bo'lsa ham nginx ishga tushadi: upstream nomi `resolver` orqali so'rov paytida aniqlanadi, shuning uchun u vaqtgacha faqat shu host 502 qaytaradi.

## 5. Image yig'ish

Image'lar Docker Hub'da, tofan bilan bir xil hisobda: backend `ejakhangir/iccu`, frontend `ejakhangir/iccu-ui`, teg `latest`. Kompyuterda:

```bash
cd D:/Projects/iccu-access-card
docker build -t ejakhangir/iccu:latest .
docker push ejakhangir/iccu:latest

cd D:/Projects/iccu-access-card-ui
docker build -t ejakhangir/iccu-ui:latest .
docker push ejakhangir/iccu-ui:latest
```

Serverdagi `docker login` tofan uchun qilingan, u yetadi.

## 6. Ishga tushirish

```bash
cd /opt/iccu && set -a && . env/iccu.env && set +a \
  && docker stack deploy --with-registry-auth -c stacks/api.yml iccu \
  && docker stack deploy --with-registry-auth -c stacks/web.yml iccu
```

Image yopiq bo'lgani uchun `--with-registry-auth` majburiy.

Tekshirish:

```bash
docker service ls
docker service logs iccu_iccu-api --since 3m --tail 50
curl -s -o /dev/null -w "%{http_code}\n" https://iccu.157.90.117.20.sslip.io/
curl -s -o /dev/null -w "%{http_code}\n" https://iccu.157.90.117.20.sslip.io/api/auth/me
```

Birinchisi `200` (UI), ikkinchisi `401` qaytarishi kerak: API ishlayapti, lekin login talab qiladi. Tofan manzillari avvalgidek ochilayotganini ham tekshiring.

Boshlang'ich ma'lumot, jumladan birinchi administrator, yaratilmaydi. **Birinchi administrator qanday qo'shilishi hali belgilanmagan.**

## 7. Yangi versiya

Kompyuterda build va push (5-bo'lim), keyin serverda o'sha deploy buyrug'i:

```bash
cd /opt/iccu && set -a && . env/iccu.env && set +a \
  && docker stack deploy --with-registry-auth -c stacks/api.yml iccu
```

Teg `latest` bo'lib qolsa ham Swarm registry'dan image digest'ini qayta so'raydi va o'zgargan bo'lsa yangisini tortadi. UI uchun `stacks/web.yml`. `order: start-first` tufayli yangi konteyner health check'dan o'tmaguncha eskisi o'chmaydi, yangi versiya yiqilsa Swarm orqaga qaytaradi (`failure_action: rollback`).

## 8. Zaxira nusxa

```bash
chmod +x /opt/iccu/backups/backup.sh
/opt/iccu/backups/backup.sh && ls -lh /opt/iccu/backups/db
(crontab -l 2>/dev/null; echo "45 2 * * * /opt/iccu/backups/backup.sh >> /opt/iccu/backups/backup.log 2>&1") | crontab -
```

Skript har kuni tunda, tofan zaxirasidan 15 daqiqa keyin ishlaydi:
- `iccu` bazasini tofan Postgres'idan `pg_dump` bilan zaxiralaydi (custom format);
- rasmlar papkasini `rsync` bilan nusxalaydi;
- 14 kundan eski dump'larni o'chiradi.

Tofan'dagi kabi zaxira hozircha faqat shu serverda. Uni serverdan tashqariga ko'chirish hali qilinmagan.

Bazani zaxiradan tiklash:

```bash
docker exec -i $(docker ps -qf name=tofan_postgres) \
  pg_restore -U postgres -d iccu --clean --if-exists < /opt/iccu/backups/db/iccu-YYYYMMDD-HHMM.dump
```

## 9. Kundalik amallar

```bash
docker service logs iccu_iccu-api --since 10m --tail 50
docker service update --force iccu_iccu-api
docker exec -it $(docker ps -qf name=tofan_postgres) psql -U postgres -d iccu
docker stats --no-stream
```

ICCU'ni butunlay olib tashlash tofan'ga tegmaydi: `docker stack rm iccu`, keyin `50-iccu.conf`ni o'chirib nginx'ni reload qilish.

### Demo ma'lumotlarini tozalash

`deploy/tofan/clear-data.sh` kitobxonlar, arizalar va rasmlarni o'chiradi, karta raqamini `0000001`dan va ariza kodini `0001`dan qayta boshlaydi. Foydalanuvchilar qoladi. Skript avval sonlarni ko'rsatadi, `TOZALA` deb yozilmaguncha hech narsaga tegmaydi, keyin `backup.sh` bilan zaxira oladi va tozalash paytida API'ni to'xtatib turadi. `backup.sh` ishga tushiriladigan bo'lishi kerak (8-bo'lim, `chmod +x`).

```bash
scp deploy/tofan/clear-data.sh deploy@157.90.117.20:/opt/iccu/backups/
chmod +x /opt/iccu/backups/clear-data.sh
/opt/iccu/backups/clear-data.sh
```

Rasm fayllari konteyner foydalanuvchisiga (UID 1654) tegishli, shuning uchun ularni o'chirishda `sudo` paroli so'ralishi mumkin. Qaytarish: bazani eng oxirgi `iccu-*.dump`dan tiklash (8-bo'lim) va `sudo rsync -a /opt/iccu/backups/files/ /opt/iccu/files/`.

## 10. Cheklovlar

- **API replikasi 1 ta.** SignalR xabarlari bitta jarayon ichida yuboriladi. Replika ko'paytirilsa Redis backplane kerak bo'ladi.
- **API xotirasi 512 MB bilan cheklangan.** Server 4 GB, uni tofan API, Keycloak va Postgres bilan bo'lishadi.
- **Swarm bitta node'da.** Baza va rasmlar shu node'ning diskida turadi.
