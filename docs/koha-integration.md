# ICCU — Koha integratsiyasi

Kutubxonada Koha ishlaydi. ICCU'da ro'yxatga olingan kitobxonlar Kohaga o'tishi kerak. Buning uchun ICCU API'da Koha uchun alohida ikki endpoint bor. Koha (yoki uning plagini, cron skripti) shu endpoint'larga ulanib ma'lumotni o'zi oladi. ICCU Kohaga hech narsa yubormaydi.

| | |
|---|---|
| Manzil | `https://<server>/api/koha/...` |
| Tarmoq | Koha serveri kutubxona tarmog'ida turadi, shuning uchun mavjud `/api/` IP allowlist'i yetadi, nginx o'zgarmagan |
| Autentifikatsiya | HTTP Basic, login `koha`, parol `ICCU_KOHA_PASSWORD` |
| Sinxronlash | Har safar to'liq ro'yxat, sahifalab |

## Endpoint'lar

### `GET koha/readers?first=0&rows=1000`

O'chirilmagan barcha kitobxonlar, karta raqami bo'yicha o'sish tartibida. `rows` ko'pi bilan 1000. Javob `PagedList` shaklida, `Result`ga o'ralmagan:

```json
{
  "data": [
    {
      "cardNumber": "0000001",
      "category": 1,
      "lastName": "Karimova",
      "firstName": "Gulnoza",
      "middleName": "Anvar qizi",
      "birthDate": "2004-05-17",
      "gender": 1,
      "citizenship": 0,
      "phone": "+998905551234",
      "issuedOn": "2026-09-26",
      "expiresOn": "2028-09-26"
    }
  ],
  "totalCount": 1
}
```

Hamma kitobxonni olish uchun `first`ni `rows` qadamida oshirib, `first >= totalCount` bo'lguncha so'rov yuboriladi.

ICCU'da o'chirilgan kitobxon ro'yxatda chiqmaydi.

### `GET koha/readers/{cardNumber}/photo`

Kitobxon rasmi, yuklanganidek (JPEG, PNG yoki WebP), `Content-Type` bilan. `cardNumber` nollar bilan ham, nollarsiz ham beriladi (`0000001` yoki `1`). Kitobxon topilmasa yoki o'chirilgan bo'lsa `404 Reader.NotFound`.

## Koha maydonlariga moslik

| ICCU | Koha (`borrowers`) | Izoh |
|---|---|---|
| `cardNumber` | `cardnumber` | 7 xonali, hech qachon o'zgarmaydi. Kitobxonni aniqlash kaliti |
| `lastName` | `surname` | |
| `firstName` | `firstname` | |
| `middleName` | `othernames` | Bo'sh bo'lishi mumkin |
| `birthDate` | `dateofbirth` | `YYYY-MM-DD` |
| `gender` | `sex` | `0` erkak (`M`), `1` ayol (`F`). Eski yozuvlarda `null` |
| `phone` | `phone` | `+998XXXXXXXXX` yoki xalqaro raqam |
| `category` | `categorycode` | Quyidagi jadval bo'yicha Koha kategoriya kodiga moslanadi |
| `issuedOn` | `dateenrolled` | Karta berilgan yoki oxirgi uzaytirilgan sana |
| `expiresOn` | `dateexpiry` | |
| `citizenship` | Patron attribute | `0` O'zbekiston fuqarosi, `1` chet el fuqarosi. Eski yozuvlarda `null` |
| rasm | `patronimage` | |

`branchcode` ICCU'da yo'q, Koha tomonida bitta filial kodi qo'yiladi.

Toifalar (`category`):

| Qiymat | Toifa |
|---|---|
| 0 | O'quvchi |
| 1 | Talaba |
| 2 | Magistr |
| 3 | PhD |
| 4 | DSc |
| 5 | Professor |
| 6 | Xodim |
| 7 | Foydalanuvchi |

## Tekshirish

```bash
curl -u koha:<parol> "https://<server>/api/koha/readers?rows=2"
curl -u koha:<parol> -o 0000001.jpg "https://<server>/api/koha/readers/0000001/photo"
```

Parolsiz yoki noto'g'ri parol bilan `401` va `WWW-Authenticate: Basic` qaytadi.

## Xavfsizlik

- Login va parol `Koha:Username` va `Koha:Password` sozlamalaridan olinadi. Ikkalasi bo'sh bo'lsa API ishga tushmaydi.
- Taqqoslash vaqtga bog'liq bo'lmagan usulda qilinadi (SHA-256 hash'lar `FixedTimeEquals` bilan).
- Basic sxema faqat `koha/` endpoint'larida ishlaydi (`Policies.Koha`). Koha paroli bilan admin endpoint'lariga kirib bo'lmaydi, admin JWT bilan esa `koha/` endpoint'lariga kirib bo'lmaydi.
- Basic parol har so'rovda yuboriladi, shuning uchun faqat HTTPS orqali ishlatiladi.
