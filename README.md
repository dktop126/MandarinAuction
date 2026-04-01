# Mandarin Auction

Система аукционов для продажи картинок мандаринов.

## Описание

Веб-приложение с REST API для проведения аукционов. Пользователи могут:
- Регистрироваться и входить через OTP код
- Просматривать активные аукционы
- Делать ставки на мандарины
- Выкупать лоты по фиксированной цене

**Особенности:**
- Мандарины автоматически генерируются каждые 10 минут
- Испорченные мандарины удаляются фоновым процессом
- Аукционы автоматически завершаются по истечении времени
- Оптимистичная блокировка для предотвращения конфликтов при ставках

## Технологии

**Backend:**
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- PostgreSQL 15
- MediatR (CQRS pattern)
- JWT Authentication

**Архитектура:**
- Clean Architecture (Domain, Application, Infrastructure, API)
- Domain-Driven Design
- CQRS (Command Query Responsibility Segregation)
- Repository Pattern
- Background Jobs (IHostedService)

## Структура проекта

```
MandarinAuction/
├── MandarinAuction.API/            # REST API контроллеры, middleware
├── MandarinAuction.Application/    # Use cases (Commands, Queries), DTOs
├── MandarinAuction.Domain/         # Бизнес-логика, сущности, исключения
├── MandarinAuction.Infrastructure/ # БД, внешние сервисы, фоновые задачи
└── docker-compose.yml              # Docker Compose конфигурация
```

## Быстрый старт

### Запуск через Docker Compose (рекомендуется)

**Требования:**
- Docker Desktop
- Docker Compose

```bash
# 1. Клонировать репозиторий
git clone https://github.com/dktop126/MandarinAuction.git
cd MandarinAuction

# 2. Запустить все сервисы
docker-compose up --build

# API будет доступен на http://localhost:5000
# Frontend будет доступен на http://localhost:8080
# PostgreSQL на localhost:5432
```

При первом запуске:
- Автоматически создастся база данных
- Применятся все миграции
- Запустятся фоновые задачи генерации мандаринов

**Остановка:**
```bash
# Остановить контейнеры
docker-compose down

# Остановить и удалить данные БД
docker-compose down -v
```

**Проверка логов:**
```bash
# Логи API
docker-compose logs api

# Логи PostgreSQL
docker-compose logs postgres

# Следить за логами в реальном времени
docker-compose logs -f api
```

### Локальный запуск (без Docker)

**Требования:**
- .NET 8.0 SDK
- PostgreSQL 15

**Шаги:**

1. **Запустить PostgreSQL:**
```bash
# Через Docker
docker run -d -p 5432:5432 \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=mandarin_auction \
  postgres:15

# Или установить PostgreSQL локально
```

2. **Настроить подключение:**

Отредактируйте `MandarinAuction.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=mandarin_auction;Username=postgres;Password=0000"
  }
}
```

3. **Применить миграции:**
```bash
cd MandarinAuction.API
dotnet ef database update
```

4. **Запустить приложение:**
```bash
dotnet run
```

API будет доступен на `http://localhost:5000`

## API Документация

### Swagger UI

Интерактивная документация API доступна по адресу:
```
http://localhost:5000/swagger
```

### Frontend UI

Веб-интерфейс для работы с аукционами:
```
http://localhost:8080
```

**Возможности:**
- Просмотр активных аукционов
- Регистрация и вход через OTP код
- Размещение ставок
- Выкуп лотов
- Таймер обратного отсчета

### Основные Endpoints

#### Аутентификация

**Запросить OTP код:**
```http
POST /api/auth/request-code
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Войти с OTP кодом:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "code": "123456"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com"
}
```

#### Аукционы

**Получить список активных аукционов:**
```http
GET /api/auction

Response:
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "mandarinId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "mandarinName": "Мандарин #1",
    "currentPrice": 100.00,
    "startingPrice": 100.00,
    "buyoutPrice": 500.00,
    "minBidIncrement": 10.00,
    "endTime": "2026-04-01T12:00:00Z",
    "status": "Active",
    "bidCount": 5
  }
]
```

**Сделать ставку (требуется авторизация):**
```http
POST /api/auction/{id}/bids
Authorization: Bearer <token>
Content-Type: application/json

{
  "amount": 150.00
}

Response:
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "currentPrice": 150.00,
  ...
}
```

**Выкупить лот (требуется авторизация):**
```http
POST /api/auction/{id}/buyout
Authorization: Bearer <token>

Response:
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "currentPrice": 500.00,
  "status": "Finished",
  ...
}
```

## Архитектурные решения

### Clean Architecture

Проект разделен на 4 слоя с четкими границами ответственности:

- **Domain** - бизнес-логика, сущности (Auction, Mandarin, Bid, User)
- **Application** - use cases через CQRS (Commands, Queries, Handlers)
- **Infrastructure** - реализация технических деталей (БД, email, фоновые задачи)
- **API** - REST контроллеры, middleware

### CQRS через MediatR

Разделение операций чтения и записи:
- **Commands** - изменяют состояние (PlaceBidCommand, BuyoutCommand)
- **Queries** - только читают данные (GetActiveAuctionsQuery)

### Domain-Driven Design

Бизнес-логика находится в доменных сущностях:
```csharp
public class Auction
{
    public void PlaceBid(Guid bidderId, decimal amount)
    {
        // Валидация и бизнес-правила внутри сущности
        if (Status == AuctionStatus.Closed)
            throw new AuctionClosedException();
        
        if (amount < CurrentPrice + MinBidIncrement)
            throw new BidTooLowException();
        
        CurrentPrice = amount;
        LastBidderId = bidderId;
    }
}
```

### Background Jobs

Фоновые задачи через `IHostedService`:
- **MandarinGeneratorHostedService** - генерация новых мандаринов каждые 10 минут
- **SpoilageCleanupHostedService** - удаление испорченных мандаринов
- **AuctionCompletionHostedService** - завершение истекших аукционов

## Тестирование

```bash
# Запустить все тесты
dotnet test

# С покрытием кода
dotnet test --collect:"XPlat Code Coverage"
```

## Автор

Коротыч Даниель
