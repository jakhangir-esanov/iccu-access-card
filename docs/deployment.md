# ICCU — Serverga o'rnatish (Docker Swarm)

Markazning o'z serverida, bitta Swarm node'da ICCU tizimini noldan ishga tushirish qo'llanmasi.

| Ma'lumot | Qiymat |
|---|---|
| Orkestratsiya | Docker Swarm, bitta node |
| Ish papkasi | `/opt/iccu` |
| Xizmatlar | `postgres`, `iccu-api`, `nginx` |
| Tarmoq | `iccu-net` (overlay) |

---

## 0. Umumiy tuzilma

Tashqaridan faqat nginx ko'rinadi (80 va 443 portlar). Postgres va API faqat ichki `iccu-net` tarmog'ida turadi.

```text
Telefonlar (QR) va kutubxona kompyuterlari
   |  :443
   v
[ nginx ]  TLS, frontend statik fayllari
   |-- /                  -> /opt/iccu/web (React build)
   |-- /api/public/       -> iccu-api:8080/public/   hamma uchun ochiq (QR anketa)
   |-- /api/, /api/hubs/  -> iccu-api:8080/          faqat kutubxona tarmog'idan (/api olib tashlanadi)
        |
    iccu-net (overlay)
        |
   [ postgres:5432 ]  tashqariga chiqmaydi
```

nginx portlari `mode: host` bilan ochiladi. Swarm'ning ingress tarmog'i mijoz IP'sini almashtirib yuboradi, u holda IP allowlist ham, rate limiting ham ishlamay qoladi.

Admin API'ni kutubxona tarmog'i bilan cheklash `deploy/nginx/conf.d/iccu.conf` faylidagi `geo $is_library_network` blokida sozlanadi. U yerda standart xususiy tarmoqlar yozilgan. **Kutubxonaning haqiqiy IP diapazonini qo'yish shart.**

## 1. Docker va Swarm

```bash
docker swarm init --advertise-addr <SERVER_IP>
docker network create --driver overlay --attachable iccu-net
```

## 2. Papkalar

```bash
sudo mkdir -p /opt/iccu/{stacks,env,files,backups,web,nginx/conf.d,nginx/certs,nginx/acme}
sudo chown -R deploy:deploy /opt/iccu
sudo chown 1654:1654 /opt/iccu/files
```

`1654` — .NET konteyneridagi `app` foydalanuvchisining UID'si. API yuklangan fayllarni `/opt/iccu/files`ga yozadi.

> **Muhim.** API stack'ida `/opt/iccu/files` mount'i bo'lishi majburiy. Mount tushib qolsa, rasmlar konteyner ichiga yoziladi va keyingi deploy'da jimgina yo'qoladi. Buning oldini olish uchun `Storage:RootPath` bo'sh bo'lsa API ataylab ishga tushmaydi.

Repodagi fayllarni serverga ko'chiring:

| Repodagi fayl | Serverdagi joyi |
|---|---|
| `deploy/stacks/*.yml` | `/opt/iccu/stacks/` |
| `deploy/nginx/conf.d/iccu.conf` | `/opt/iccu/nginx/conf.d/` |
| `deploy/backup/backup.sh` | `/opt/iccu/backups/backup.sh` |
## 3. iccu.env fayli

Parollar `tofan`dagidek `/opt/iccu/env/iccu.env` faylida turadi. Deploy oldidan fayl `set -a && . env/iccu.env` bilan yuklanadi, stack fayllari qiymatlarni `${...}` orqali oladi. Parollarni qo'lda yozmang, buyruq ularni o'zi yaratadi. Faqat hex ishlatiladi, chunki maxsus belgilar connection string'ni buzadi.

```bash
cd /opt/iccu && cat > env/iccu.env <<EOF
ICCU_API_IMAGE=registry.example.uz/iccu-api:v1.0.0

ICCU_DB_PASSWORD=$(openssl rand -hex 16)
ICCU_JWT_SIGNING_KEY=$(openssl rand -hex 32)
ICCU_KOHA_PASSWORD=$(openssl rand -hex 24)
EOF
```

| O'zgaruvchi | Kim ishlatadi | API ichidagi kalit |
|---|---|---|
| `ICCU_DB_PASSWORD` | Postgres (`POSTGRES_PASSWORD`) va API | connection string'dagi `Password` |
| `ICCU_JWT_SIGNING_KEY` | API, kamida 32 bayt | `Jwt:SigningKey` |
| `ICCU_KOHA_PASSWORD` | API; Koha administratoriga beriladi (login `koha`) | `Koha:Password` |

Faylni faqat root tahrirlaydigan, `deploy` esa faqat o'qiy oladigan qiling:

```bash
sudo chown -R root:deploy /opt/iccu/env
sudo chmod 750 /opt/iccu/env
sudo chmod 640 /opt/iccu/env/iccu.env
```

`ICCU_DB_PASSWORD` faqat birinchi deploy'da, Postgres volume'i bo'sh bo'lganda o'rnatiladi. Keyin uni faylda o'zgartirish bazadagi parolni o'zgartirmaydi.

## 4. TLS sertifikati

nginx `/opt/iccu/nginx/certs/fullchain.pem` va `privkey.pem` fayllarini kutadi. Uchta yo'l bor:

1. **Domen bor**: Let's Encrypt (certbot, `tofan`dagi kabi webroot usuli, webroot: `/opt/iccu/nginx/acme`).
2. **Ochiq IP bor, domen yo'q**: `<IP>.sslip.io` nomi bilan Let's Encrypt (`tofan`da ishlatilgan usul).
3. **Faqat kutubxona Wi-Fi'si**: Markazning ichki CA'si yoki self-signed sertifikat. Bu holda telefonlar ogohlantirish ko'rsatadi, CA'ni qurilmalarga o'rnatish kerak bo'ladi.

HTTPS'siz ishlatmang: anketa orqali shaxsiy ma'lumotlar va rasm yuboriladi.

## 5. Ishga tushirish

Ketma-ketlik: avval Postgres, keyin API, oxirida nginx.

```bash
cd /opt/iccu && set -a && . env/iccu.env && set +a
docker stack deploy -c stacks/postgres.yml iccu
docker stack deploy --with-registry-auth -c stacks/api.yml iccu
docker stack deploy -c stacks/nginx.yml iccu
```

API ishga tushganda migratsiyalarni o'zi qo'llaydi. Boshlang'ich ma'lumot, jumladan birinchi administrator, yaratilmaydi. **Birinchi administrator qanday qo'shilishi hali belgilanmagan.**

Tekshirish:

```bash
docker service ls
docker service logs iccu_iccu-api --tail 50
curl -k https://localhost/api/auth/me
```

Oxirgi buyruq 401 qaytarishi kerak: API ishlayapti, lekin login talab qiladi.

## 6. Image yig'ish va yangilash

Kompyuterda, repo ildizida:

```bash
docker build -t registry.example.uz/iccu-api:v1.0.1 .
docker push registry.example.uz/iccu-api:v1.0.1
```

Serverda `env/iccu.env` ichidagi `ICCU_API_IMAGE`ni yangilab, API stack'ini qayta deploy qiling. `order: start-first` sozlamasi tufayli yangi konteyner health check'dan o'tmaguncha eskisi o'chmaydi. Yangi versiya yiqilsa, Swarm avtomatik orqaga qaytaradi (`failure_action: rollback`).

## 7. Zaxira nusxa

```bash
chmod +x /opt/iccu/backups/backup.sh
crontab -e
```

```text
30 2 * * * /opt/iccu/backups/backup.sh >> /opt/iccu/backups/backup.log 2>&1
```

Skript har kuni tunda ishlaydi:
- bazani `pg_dump` bilan zaxiralaydi (custom format);
- rasmlar papkasini `rsync` bilan nusxalaydi;
- 14 kundan eski dump'larni o'chiradi.

Zaxira nusxani boshqa diskka yoki serverga ham ko'chirib turing.

Bazani zaxiradan tiklash:

```bash
docker exec -i $(docker ps -qf name=iccu_postgres) pg_restore -U iccu -d iccu --clean < iccu-YYYYMMDD-HHMM.dump
```

## 8. Cheklovlar

- **API replikasi 1 ta.** SignalR xabarlari bitta jarayon ichida yuboriladi. Replika ko'paytirilsa Redis backplane kerak bo'ladi.
- **Swarm bitta node'da.** Postgres va rasmlar shu node'ning diskida turadi. Ko'p node'ga o'tilsa, rasmlar uchun MinIO yoki umumiy disk kerak bo'ladi (`IFileStore`ning boshqa implementatsiyasi).
