# Hospitium.ReviewService - API Endpoints Summary

**Service:** Review Service  
**Base URL:** `http://localhost:5004/api/reviews` (or via Gateway: `http://localhost:5000/api/reviews`)  
**Target Framework:** .NET 8  
**Port:** 5004

---

## Overview

The Review Service handles hotel reviews and ratings. It's an independent microservice that:
- ✅ Stores hotel reviews with comments
- ✅ Allows users to add quick ratings without comments
- ✅ Publishes `ReviewAddedEvent` to update hotel ratings in real-time
- ✅ Provides review history for hotels

---

## API Endpoints

### 1. Add Review (Full Review with Comment)

```
POST /api/reviews
```

**Authentication:** ✅ Required (Any authenticated user)

**Request:**
```json
{
  "hotelId": 1,
  "rating": 5,
  "comment": "Excellent hotel with great service!"
}
```

**Response (200 OK):**
```json
{
  "reviewId": 1,
  "hotelId": 1,
  "userId": 1,
  "userName": "John Doe",
  "rating": 5,
  "comment": "Excellent hotel with great service!",
  "createdAt": "2024-02-01T10:30:00Z"
}
```

**Validation:**
- `rating`: Must be integer 1-5
- `comment`: Max 1000 characters
- `hotelId`: Must exist

---

### 2. Rate Hotel (Quick Rating Only)

```
POST /api/reviews/rate?hotelId=1&rating=5
```

**Authentication:** ✅ Required (Any authenticated user)

**Query Parameters:**
- `hotelId`: int (required) - Hotel ID to rate
- `rating`: int (required) - Rating value 1-5

**Response (200 OK):**
```json
{
  "reviewId": 2,
  "hotelId": 1,
  "userId": 1,
  "userName": "john@example.com",
  "rating": 5,
  "comment": "Rating only",
  "createdAt": "2024-02-01T11:00:00Z"
}
```

**Use Case:** When user wants to quickly rate without writing a comment

---

### 3. Get Hotel Reviews

```
GET /api/reviews/hotel/{hotelId}
```

**Authentication:** ❌ None (Public endpoint)

**Path Parameter:**
- `hotelId`: int (required) - Hotel ID

**Response (200 OK):**
```json
[
  {
    "reviewId": 1,
    "hotelId": 1,
    "userId": 1,
    "userName": "John Doe",
    "rating": 5,
    "comment": "Excellent hotel with great service!",
    "createdAt": "2024-02-01T10:30:00Z"
  },
  {
    "reviewId": 2,
    "hotelId": 1,
    "userId": 2,
    "userName": "Jane Smith",
    "rating": 4,
    "comment": "Good location and clean rooms",
    "createdAt": "2024-02-01T09:15:00Z"
  }
]
```

**Features:**
- ✅ Sorted by most recent first
- ✅ Shows userName for privacy (not full user details)
- ✅ Public endpoint (no auth needed)

---

## Model Structure

### Review Model

| Field | Type | Constraints |
|-------|------|-------------|
| `reviewId` | int | Primary key, auto-increment |
| `hotelId` | int | Foreign key (required) |
| `userId` | int | User who wrote review (required) |
| `userName` | string | Display name (required) |
| `rating` | int | 1-5 (required) |
| `comment` | string | Max 1000 chars (required) |
| `createdAt` | DateTime | UTC timestamp |

---

## Event Integration

### ReviewAddedEvent (Published)

Triggered when a review is added:

```csharp
{
  "HotelId": 1,
  "Rating": 5
}
```

**Consumed by:** HotelService.ReviewAddedConsumer

**Action:** Updates hotel's average rating and total review count

---

## cURL Examples

### Add Full Review
```bash
curl -X POST http://localhost:5004/api/reviews \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {your_token}" \
  -d '{
    "hotelId": 1,
    "rating": 5,
    "comment": "Excellent service and beautiful rooms!"
  }'
```

### Quick Rate Hotel
```bash
curl -X POST "http://localhost:5004/api/reviews/rate?hotelId=1&rating=4" \
  -H "Authorization: Bearer {your_token}"
```

### Get All Reviews for Hotel
```bash
curl -X GET http://localhost:5004/api/reviews/hotel/1 \
  -H "Accept: application/json"
```

---

## Database Schema

**Table:** Reviews

```sql
CREATE TABLE Reviews (
    ReviewId INT PRIMARY KEY IDENTITY(1,1),
    HotelId INT NOT NULL,
    UserId INT NOT NULL,
    UserName NVARCHAR(MAX) NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(1000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL
);
```

---

## Error Responses

### 401 Unauthorized
```json
{
  "message": "Authorization header missing or invalid"
}
```

### 400 Bad Request - Invalid Rating
```json
{
  "message": "Rating must be between 1 and 5"
}
```

### 400 Bad Request - Invalid Comment Length
```json
{
  "message": "Comment must not exceed 1000 characters"
}
```

### 500 Internal Server Error
```json
{
  "message": "An error occurred while processing your request"
}
```

---

## Service Configuration

**Program.cs Setup:**
- ✅ JWT Authentication with same configuration as other services
- ✅ MassTransit RabbitMQ for event publishing
- ✅ Entity Framework Core with SQL Server
- ✅ Swagger/OpenAPI documentation
- ✅ CORS enabled for API Gateway

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=ReviewDb;..."
  },
  "Jwt": {
    "Key": "[same as other services]",
    "Issuer": "Hospitium",
    "Audience": "HospitiumUsers"
  }
}
```

---

## Integration Points

### Receives From
- **Booking Service**: When user books → can write review for that hotel
- **Auth Service**: User registration/login for authentication

### Publishes To
- **HotelService**: `ReviewAddedEvent` → Updates hotel rating
- **NotificationService**: Could send review notifications (if implemented)

---

## Summary

| Endpoint | Method | Auth | Purpose |
|----------|--------|------|---------|
| `/api/reviews` | POST | ✅ | Add full review with comment |
| `/api/reviews/rate` | POST | ✅ | Add quick rating only |
| `/api/reviews/hotel/{id}` | GET | ❌ | Get all reviews for hotel |

**Total Endpoints:** 3  
**Authenticated Endpoints:** 2  
**Public Endpoints:** 1

---

**Updated:** 2024  
**Version:** 1.0
