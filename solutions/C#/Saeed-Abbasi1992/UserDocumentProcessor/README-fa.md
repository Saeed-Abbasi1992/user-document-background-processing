# پردازشگر اسناد کاربر

## توضیحات پروژه
سیستم مدیریت کاربران که امکان ثبت‌نام از طریق API و آپلود اسناد را فراهم می‌کند. سیستم پردازش پس‌زمینه اسناد، ارسال اعلان‌ها و پاکسازی شبانه فایل‌ها را انجام می‌دهد.

## ویژگی‌ها / نکات برجسته
- ثبت‌نام کاربر با آپلود سند
- پردازش پس‌زمینه اسناد با تاخیر 30 ثانیه‌ای
- ارسال پیام خوش‌آمدگویی و پیام اتمام پردازش
- پاکسازی شبانه فایل‌های قدیمی و بدون مالک
- سیاست تکرار خودکار برای همه Job‌ها (2 بار: 5 دقیقه و 10 دقیقه)
- Health Check: بررسی Liveness و Readiness
- Logging ساخت‌یافته برای Job‌ها و پاکسازی
- **داشبورد Hangfire** برای مانیتورینگ Job‌ها
- **Swagger / OpenAPI** برای مستندات API

## تکنولوژی‌ها / پیش‌نیازها
- **زبان و فریم‌ورک:** C#, .NET 9
- **کتابخانه‌ها:** Hangfire, MediatR, EF Core, FluentValidation, Swagger
- **سیستم عامل:** Windows / Linux / macOS

## شروع به کار / راه‌اندازی
1. کلون کردن ریپازیتوری:
```bash
git clone https://github.com/<org>/<repo>.git
cd <repo>
```

2. پیکربندی:
- به‌روزرسانی connection string در `appsettings.json`
- تنظیم مسیر ذخیره فایل‌ها

3. ساخت و اجرا:
```bash
dotnet build
dotnet run
```

4. Health Checks:
- Liveness: `/health/self`
- Readiness: `/health/ready`

5. **داشبورد Hangfire** در مسیر `/hangfire`
6. **Swagger UI** در مسیر `/swagger`

## Endpoint‌های API
### ثبت‌نام کاربر
**POST** `/api/users/register`
- **ورودی:** `name`، `email`، `document` (فایل)
- **خروجی:**
```json
{
  "userId": "<GUID>",
  "status": "Registered",
  "message": "User registered successfully."
}
```

## Job‌های پس‌زمینه
1. **Job پیام خوش‌آمدگویی:** بلافاصله بعد از ثبت‌نام اجرا می‌شود
2. **Job پردازش سند:** 30 ثانیه بعد از ثبت‌نام اجرا می‌شود و سند را به PDF تبدیل می‌کند (شبیه‌سازی مجاز)
3. **Job پیام اتمام پردازش:** بعد از پردازش سند اجرا می‌شود و پیام اتمام را ارسال می‌کند
4. **Job پاکسازی شبانه:** هر روز ساعت 00:00 اجرا می‌شود و فایل‌های قدیمی یا بدون مالک را حذف می‌کند
5. **سیاست Retry:** حداکثر 2 بار تلاش مجدد؛ Retry #1 = 5 دقیقه، Retry #2 = 10 دقیقه

## تست‌ها
- Unit Tests و Integration Tests با xUnit
- اجرای تست‌ها:
```bash
dotnet test
```

## ساختار پوشه‌ها / معماری
- **Api:** کنترلرها و مدل‌های درخواست/پاسخ
- **Application:** Commands، Handlers، DTOها، اینترفیس‌ها و سرویس‌ها
- **Domain:** Entities، Enums، اینترفیس‌های Repository
- **Infrastructure:** persistence با EF Core، ذخیره فایل، Job‌های پس‌زمینه و سرویس‌ها
- **Tests:** تست‌های Unit و Integration

پروژه از معماری **Clean Architecture** پیروی می‌کند.

## Migration دیتابیس
### با استفاده از PMC:
```powershell
Add-Migration InitialCreate -Project UserDocumentProcessor.Infrastructure -StartupProject UserDocumentProcessor.API -OutputDir Persistence/Migrations
Update-Database -Project UserDocumentProcessor.Infrastructure -StartupProject UserDocumentProcessor.API
```

### با استفاده از CLI:
```bash
dotnet ef migrations add InitialCreate \
    --project UserDocumentProcessor.Infrastructure \
    --startup-project UserDocumentProcessor.API \
    --output-dir Persistence/Migrations

dotnet ef database update \
    --project UserDocumentProcessor.Infrastructure \
    --startup-project UserDocumentProcessor.API
```

## یادداشت‌ها / بهبودهای آتی
- افزودن پشتیبانی از ایمیل و SMS برای ارسال اعلان‌ها

